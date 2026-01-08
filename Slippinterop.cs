using ExternalMeleeTool.Marshaling;
using ExternalMeleeTool.MeleeTypes;
using System.Diagnostics;
using System.Drawing;
using System.Numerics;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool;

#pragma warning disable IDE0079 // seriously. why?
#pragma warning disable CA2020, SYSLIB1054 // avoids marshaling warnings which are not necessary

[StructLayout(LayoutKind.Sequential)]
struct MEMORY_BASIC_INFORMATION {
    public IntPtr BaseAddress;
    public IntPtr AllocationBase;
    public uint AllocationProtect;
    public ushort PartitionId;
    public IntPtr RegionSize;
    public uint State;
    public uint Protect;
    public uint Type;
}
public class Slippinterop {
    const uint MEM_COMMIT = 0x1000;
    const uint PAGE_READWRITE = 0x04;
    const uint PAGE_WRITECOPY = 0x08;
    const uint PAGE_EXECUTE_READWRITE = 0x40;
    const uint PAGE_EXECUTE_WRITECOPY = 0x80;

    static Process? _process;
    static IntPtr _dolphin;

    /// <summary>Location of GALE01 in system memory.</summary>
    public static long GALE01 { get; private set; } = 0;
    /// <summary>If GALE01 has been found in system memory.</summary>
    public static bool IsConnected => _process != null && !_process.HasExited && GALE01 != 0;

    /// <summary>
    /// Attempts to connect to a running instance of Slippi Dolphin and locate Melee's GALE01 module in memory using an AoB scan.
    /// </summary>
    /// <returns><c>true</c> if the scan was successful, otherwise <c>false</c>.</returns>
    public static bool Connect(params string[] gameIds) {
        try {
            _process = Process.GetProcessesByName("Slippi Dolphin").FirstOrDefault();
            if (_process == null) return false;

            _dolphin = _process.Handle;
            GALE01 = GameSignatureScan(gameIds);
        } catch {
            // prevent any errors
            return false;
        }

        return GALE01 != 0;
    }

    /// <summary>
    /// Unpacks the bits of a byte into an array of bytes, where each entry is either 1 or 0 representing each bit.
    /// </summary>
    /// <param name="value">The byte to unpack.</param>
    /// <returns>1 or 0 in each array entry, representing each bit.</returns>
    public static byte[] Unpack(byte value) {
        var bits = new byte[8];
        for (int i = 0; i < 8; i++) {
            bits[i] = (byte)((value >> i) & 1);
        }
        return bits;
    }
    // high-level
    /// <summary>
    /// Loads the player block at the given location in memory.
    /// </summary>
    /// <param name="slot">The fighter slot to load.</param>
    /// <returns>The loaded fighter block.</returns>
    public static FighterData GetMeleeFighterBlock(FighterMemorySlot slot) {
        long block = (long)slot;
        var fd = new FighterData {
            SlotKind      = (SlotKind)ReadS32(block + 0x8),
            Team          = (SlotTeam)ReadU8(block + 0x47),
            Direction     = ReadF32(block + 0x40),
            Percent       = ReadS16(block + 0x60),
            Stocks        = ReadS8(block + 0x8E),
            // 0x0D == 1 = not transformed
            // 0x0C == 1 = transformed (or other equals 0)
            IsTransformed = ReadU8(block + 0x0C) == 1
        };
        // these positions are part of the same union
        // 0x1c = transformed char pos
        // 0x10 = main player pos
        fd.Position = fd.IsTransformed ? ReadVec3(block + 0x1C) : ReadVec3(block + 0x10);

        var kind = (CKind)ReadS32(block + 0x4);
        fd.CharKind = fd.IsTransformed ? FighterData.SubCharMap[kind] : kind;

        //if (pb.SlotKind != SlotKind.None)
        //Console.WriteLine(ReadU8(block + 0x0C) + " " + ReadU8(block + 0x0D));
        /*Console.WriteLine($"--------------------" +
            $"x0E={ReadS16(block + 0x0E)}" +
            $"\nx45={ReadU8(block + 0x45)}" +
            $"\nx4C={ReadS8(block + 0x4C)}" +
            $"\nx4D={ReadS8(block + 0x4D)}" +
            $"\nx4E={ReadS8(block + 0x4E)}" +
            $"\nx4F={ReadS8(block + 0x4F)}" +
            $"\nx50={ReadF32(block + 0x50)}");*/

        /* NOTES:
            * the GObj array at 0xB0 is: [0] = the main player GObj [1] the following character GObj
            * I could access the memory of the GObj by:
            * gobj_ptr = ReadU32(block + 0xB0) <-- reads address of gobj
            * user_data = ReadU32(gobj_ptr + 0x2C) <-- x2C is the offset of 'user_data' in HSD_GObj
            * .. if we *know* the user_data is a Fighter... we can do something like:
            * self_vel = ReadVec3(user_data + 0x84) <-- x80 is the offset of 'self_vel' in Fighter struct
        */

        fd.GObj = ReadPtr(block + 0xB0);
        fd.Fighter = ReadPtr(fd.GObj + 0x2C);
        fd.AnimState = (FtAnimState)ReadS32(fd.Fighter + 0x10);
        fd.VelocitySelf = ReadVec3(fd.Fighter + 0x80);
        fd.VelocityKnockback = ReadVec3(fd.Fighter + 0x8C);
        fd.LStick = ReadVec2(fd.Fighter + 0x620);
        fd.CStick = ReadVec2(fd.Fighter + 0x638);
        fd.ButtonsCurrent = ReadU32(fd.Fighter + 0x65C);
        fd.ButtonsOnInput = ReadU32(fd.Fighter + 0x668);

        fd.Bones = ReadPtr(fd.Fighter + 0x5E8);
        fd.Attr = ReadStruct<FtCommonAttr>(fd.Fighter + 0x110);
        /*nint jobj_parent = ReadPtr(head_jobj + 0xC);

        while (jobj_parent!= MeleeConstants.ROM_SIZE) {
            var vec = ReadVec3(jobj_parent + 0x38);
            Console.WriteLine(vec); // position
            jobj_parent = ReadPtr(jobj_parent + 0xC);
        }*/

        /*var trans = ReadVec3(head_jobj + 0x38);
        var quat = ReadQuat(head_jobj + 0x1C);
        Console.WriteLine(trans + "                ");
        Console.WriteLine(quat + "                ");*/

        // let's get the ECB data!
        // since it's not a pointer, we don't need to ReadPtr.
        // but, offsets are better gotten via the raw offset instead of adding to the coll_data offset
        // nint coll_data = fd.Fighter + 0x6F0;
        fd.ECB = ReadStruct<ECB>(fd.Fighter + 0x794);

        
        //WriteF32(fd.Fighter + 0x89C, 2);

        return fd;
    }

    /// <summary>
    /// Loads the current match settings.
    /// </summary>
    public static MatchData GetMatchData() {
        var data = new MatchData {
            IsTeams = ReadU8(MeleeConstants.START_MELEE_RULES + 0x8) == 1,
            Fighters = new FighterData[4]
        };

        data.Fighters[0] = GetMeleeFighterBlock(FighterMemorySlot.IndexOne);
        data.Fighters[1] = GetMeleeFighterBlock(FighterMemorySlot.IndexTwo);
        data.Fighters[2] = GetMeleeFighterBlock(FighterMemorySlot.IndexThree);
        data.Fighters[3] = GetMeleeFighterBlock(FighterMemorySlot.IndexFour);
        return data;
    }
    // move back to this later
    public static StageData GetStageData(GlobalMeleeData gmd) {
        const nint stinfo = MeleeConstants.STAGE_INFO;
        nint coll_data_ptr = ReadPtr(stinfo + 0x6AC);

        int vertCount = ReadS32(coll_data_ptr + 0x4);

        if (gmd.MajorScene != MeleeConstants.MAJOR_SCENE_INGAME
            // i feel like in most cases if data isnt initialized and it reads garbage
            // it won't be near 0 or below 1000, so check it
            || vertCount <= 0 || vertCount >= 1000)
            return default;

        nint grParam_ptr = ReadPtr(stinfo + 0x6B0);

        int lineCount = ReadS32(coll_data_ptr + 0xC);

        /*nint joints_ptr = ReadPtr(coll_data_ptr + 0x24);
        //int num_joints = ReadS32(coll_data_ptr + 0x28);
        // int test = ReadS16(joints_ptr + 0x2);
        float lBound = ReadF32(joints_ptr + 0x14);
        float bBound = ReadF32(joints_ptr + 0x18);
        float rBound = ReadF32(joints_ptr + 0x1C);
        float tBound = ReadF32(joints_ptr + 0x20);
        int vtx_count = ReadS16(joints_ptr + 0x26);
        int joint_count = ReadS32(coll_data_ptr + 0x28);*/

        // read Vec2* at the start of the MapCollData*
        nint vertices = ReadPtr(coll_data_ptr);
        nint mapLines = ReadPtr(coll_data_ptr + 0x8);

        var verts = new Vector2[vertCount];
        var lines = new StageLineMap[lineCount];

        for (int i = 0; i < vertCount; i++) {
            // Marshal.SizeOf<Vector2>()?
            verts[i] = ReadVec2(vertices + (i * 8)); // 8 bytes per Vector2
        }
        for (int i = 0; i < lineCount; i++) {
            lines[i] = new StageLineMap(
                ReadU16(mapLines +       (i * StageLineMap.SIZE)),   // start idx
                ReadU16(mapLines + 0x2 + (i * StageLineMap.SIZE))    // end idx
            );
        }

        var data = new StageData {
            StageId = (ExternalStageId)ReadU16(MeleeConstants.START_MELEE_RULES + 0xE),
            // Scale = stageScale,
            GroundParams = ReadStruct<GrGroundParam>(grParam_ptr),
            BlastZone = ReadBoundingRect(stinfo + 0x74),
            // 0x0 = camerainfo
            CameraInfo = ReadStruct<StageCameraInfo>(stinfo),
            VertexCount = vertCount,
            LineCount = lineCount,
            MapLines = lines,
            Vertices = verts
        };

        return data;
    }

    /// <summary>
    /// Loads global melee data.
    /// </summary>
    public static GlobalMeleeData GetGlobalData() {
        var data = new GlobalMeleeData {
            MinorScene = ReadU8(MeleeConstants.MINOR_SCENE),
            MajorScene = ReadU8(MeleeConstants.MAJOR_SCENE)
        };
        return data;
    }

    public static SlippiOnlineData GetOnlineData(GlobalMeleeData gmd) {
        var data = new SlippiOnlineData {
            ClientPort = SlippiOnlineData.GetClientPort(gmd),
            ClientControllerPort = ReadU8(ReadPtr(SlippiConstants.ONLINE_DATA_BLOCK + 0x2)),
            InOnlineMatch = SlippiOnlineData.IsSlippiOnline(gmd),
            Frame = ReadU8(ReadPtr(SlippiConstants.ONLINE_DATA_BLOCK + 0x3))
        };
        return data;
    }

    /// <summary>
    /// A function to set Melee's Develop camera position, focus, and FOV.
    /// </summary>
    /// <param name="eye">The origin of the camera.</param>
    /// <param name="focus">The location for the camera to look at.</param>
    /// <param name="fov">The field-of-view of the camera.</param>
    /// <remarks>This function is typically called from <see cref="MeleeFreeCamera.SetCam"/>.</remarks>
    public static void SetMeleeCamera(Vector3 eye, Vector3 focus, float fov) {
        // the payload of bytes to send into melee's memory
        List<byte> payload = [];

        // important to write the focus first since it's *before* the eye in memory
        payload.AddRange(FloatToBigEndian(focus.X));
        payload.AddRange(FloatToBigEndian(focus.Y));
        payload.AddRange(FloatToBigEndian(focus.Z * -1)); // invert z to match melee

        // eye/origin, written after
        payload.AddRange(FloatToBigEndian(eye.X));
        payload.AddRange(FloatToBigEndian(eye.Y));
        payload.AddRange(FloatToBigEndian(eye.Z * -1));

        // camera fov
        payload.AddRange(FloatToBigEndian(fov));

        byte[] data = [.. payload];
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + MeleeConstants.CAM_START), data, data.Length, out _);
    }
    /// <summary>
    /// Sets the type of camera melee will use.
    /// </summary>
    /// <param name="type">The kind of camera melee will use to set its render matrices to.</param>
    public static void SetCameraType(CameraKind type) {
        // 0x08 = develop camera offset
        WriteU8(MeleeConstants.CAM_TYPE, (byte)type);
    }

    // non-api

    // big endian since GC architecture is big endian... very important
    static byte[] FloatToBigEndian(float val) {
        byte[] b = BitConverter.GetBytes(val);
        Array.Reverse(b);
        return b;
    }

    // scan's melee's AoB
    static long PerformAoBScan(params byte[][] patterns) {
        long maxAddress = 0x7FFFFFFF0000; // max by default, but isn't used if scan fails
        long currentAddress = 0;

        // why does ide say this is useless, xd
        var memInfo = new MEMORY_BASIC_INFORMATION();

        // Loop through the process memory pages
        while (currentAddress < maxAddress &&
               SysLib.VirtualQueryEx(_dolphin, (IntPtr)currentAddress, out memInfo, (uint)Marshal.SizeOf(memInfo)) != 0) {

            /* checks if:
             * 1) memory is actually WRITABLE memory
             * 2) if memory has read/write permissions to prevent memory access violations
             */
            bool isWritable = (memInfo.Protect & (PAGE_READWRITE | PAGE_WRITECOPY | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY)) != 0;

            if (memInfo.State == MEM_COMMIT && isWritable) {
                // dolphin mem1 is always 32mb, aka 0x2000000
                // aka... the length of RAM, where ROM is 0x80000000 long.
                if (memInfo.RegionSize == MeleeConstants.RAM_SIZE) {
                    // if we're here, we've found the 32MB section of the GC ram.
                    // now check and assign to GALE01 (the game's code)
                    for (int i = 0; i < patterns.Length; i++) {
                        var curPattern = patterns[i];
                        var buffer = new byte[curPattern.Length];
                        if (SysLib.ReadProcessMemory(_dolphin, memInfo.BaseAddress, buffer, curPattern.Length, out _)) {
                            if (PatternMatch(buffer, curPattern)) {
                                return memInfo.BaseAddress;
                            }
                        }
                    }
                }
            }

            // if we fail finding GALE01 in any way we just try again in the next region
            long regionSize = memInfo.RegionSize;
            if (regionSize == 0) break;
            currentAddress = memInfo.BaseAddress + regionSize;
        }

        return 0; // :(
    }
    static bool PatternMatch(byte[] data, byte[] pattern) {
        if (data.Length < pattern.Length) return false;
        for (int i = 0; i < pattern.Length; i++) {
            // this ensures we found GALE01's header, since we check the first 8 bytes found in melee's memory (typically!)
            if (data[i] != pattern[i]) return false;
        }
        return true;
    }
    static long GameSignatureScan(params string[] gameIds) {
        // "GALE01" + 0x00 + 0x02
        // byte[] pattern = [0x47, 0x41, 0x4C, 0x45, 0x30, 0x31, 0x00, 0x02];
        // byte[] pattern = "Super Smash Bros. Melee".Select(x => (byte)x).ToArray();

        // hypothetically...
        //var gtme01 = "GTME01".Select(x => (byte)x).ToArray();
        //long gtme = PerformAoBScan(gtme01);

        // boi what is this code
        var patterns = gameIds.Select(x => x.Select(y => (byte)y).ToArray()).ToArray();
        return PerformAoBScan(patterns);
        //var pattern = gameIds.Select(x => (byte)x).ToArray();
        //return PerformAoBScan(pattern);
    }

    // api but low-level

    #region Memory Read/Write

    #region Primitive Reads
    /// <summary>Reads a signed 8-bit integer from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static sbyte ReadS8(long offset) {
        byte rawValue = ReadU8(offset);
        return (sbyte)rawValue;
    }
    /// <summary>Reads an unsigned 8-bit integer from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static byte ReadU8(long offset) {
        byte[] buffer = new byte[1];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 1, out _);
        return buffer[0];
    }
    /// <summary>Reads a signed 16-bit integer from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static short ReadS16(long offset) {
        byte[] buffer = new byte[2];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 2, out _);
        Array.Reverse(buffer);
        return BitConverter.ToInt16(buffer, 0);
    }
    /// <summary>Reads an unsigned 16-bit integer from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static ushort ReadU16(long offset) => (ushort)ReadS16(offset);

    /// <summary>Reads a signed 32-bit integer from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static int ReadS32(long offset) {
        byte[] buffer = new byte[4];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 4, out _);
        Array.Reverse(buffer);
        return BitConverter.ToInt32(buffer, 0);
    }
    /// <summary>Reads an unsigned 32-bit float from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static uint ReadU32(long offset) => (uint)ReadS32(offset);

    /// <summary>Reads an memory address from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset, and the ROM size is subtracted after to return a GC pointer.</remarks>
    public static nint ReadPtr(long offset) => (nint)(ReadU32(offset) - MeleeConstants.ROM_SIZE);

    /// <summary>Reads a 32-bit float from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static float ReadF32(long offset) {
        byte[] buffer = new byte[4];
        // read 4 bytes for a 32 bit single
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 4, out _);
        Array.Reverse(buffer); // Big Endian -> Little Endian
        return BitConverter.ToSingle(buffer, 0);
    }
    #endregion

    #region Non-Primitive Reads
    /// <summary>Reads two (2) 32-bit floats in sequential order from a given GALE01 offset to construct a <see cref="Vector2"/>.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static Vector2 ReadVec2(long offset) {
        byte[] buffer = new byte[8];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 8, out _);

        byte[] xB = buffer[0..4]; Array.Reverse(xB);
        byte[] yB = buffer[4..8]; Array.Reverse(yB);

        return new Vector2(
            BitConverter.ToSingle(xB, 0),
            BitConverter.ToSingle(yB, 0)
        );
    }
    /// <summary>Reads three (3) 32-bit floats in sequential order from a given GALE01 offset to construct a <see cref="Vector3"/>.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static Vector3 ReadVec3(long offset) {
        // 4 bytes per float
        byte[] buffer = new byte[12];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 12, out _);

        // no need to invoke GObj_GetPlayerBlock cuz we have the offsets already stored above
        byte[] xB = buffer[0..4]; Array.Reverse(xB);
        byte[] yB = buffer[4..8]; Array.Reverse(yB);
        byte[] zB = buffer[8..12]; Array.Reverse(zB);

        return new Vector3(
            BitConverter.ToSingle(xB, 0),
            BitConverter.ToSingle(yB, 0),
            BitConverter.ToSingle(zB, 0)
        );
    }

    public static Quaternion ReadQuat(long offset) {
        byte[] buffer = new byte[16];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 16, out _);
        byte[] xB = buffer[0..4]; Array.Reverse(xB);
        byte[] yB = buffer[4..8]; Array.Reverse(yB);
        byte[] zB = buffer[8..12]; Array.Reverse(zB);
        byte[] wB = buffer[12..16]; Array.Reverse(wB);

        return new Quaternion(
            BitConverter.ToSingle(xB, 0),
            BitConverter.ToSingle(yB, 0),
            BitConverter.ToSingle(zB, 0),
            BitConverter.ToSingle(wB, 0)
        );
    }

    /*public static Matrix3x4 ReadMatrix3x4(long offset) {
        byte[] buffer = new byte[3 * 4 * 4]; // 48 bytes
        SysLib.ReadProcessMemory(
            _dolphin,
            (IntPtr)(GALE01 + offset),
            buffer,
            buffer.Length,
            out _
        );
        // can probably just ReadF32...?
        float ReadF(int i) {
            byte[] f = buffer[i..(i + 4)];
            Array.Reverse(f);
            return BitConverter.ToSingle(f, 0);
        }
        return new Matrix3x4 {
            // row 0
            M11 = ReadF(0),
            M12 = ReadF(4),
            M13 = ReadF(8),
            M14 = ReadF(12),

            // row 1
            M21 = ReadF(16),
            M22 = ReadF(20),
            M23 = ReadF(24),
            M24 = ReadF(28),

            // row 2
            M31 = ReadF(32),
            M32 = ReadF(36),
            M33 = ReadF(40),
            M34 = ReadF(44),
        };
    }*/

    static BoundingRect ReadBoundingRect(long offset) {
        byte[] buffer = new byte[16];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 16, out _);

        return new BoundingRect {
            Left = ReadF32(offset),
            Right = ReadF32(offset + 4),
            Top = ReadF32(offset + 8),
            Bottom = ReadF32(offset + 12),
        };
    }

    #endregion

    #region Primitive Writes

    public static void WriteS8(long offset, sbyte value) {
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), [(byte)value], 1, out _);
    }
    public static void WriteU8(long offset, byte value) => WriteS8(offset, (sbyte)value);
    public static void WriteS16(long offset, short value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), bytes, 2, out _);
    }
    public static void WriteU16(long offset, ushort value) => WriteS16(offset, (short)value);
    public static void WriteS32(long offset, int value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), bytes, 4, out _);
    }
    public static void WriteU32(long offset, uint value) => WriteS32(offset, (int)value);
    public static void WriteF32(long offset, float value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), bytes, 4, out _);
    }

    #endregion

    #region Non-Primitive Writes

    public static void WriteVec3(long offset, Vector3 vec) {
        byte[] xB = BitConverter.GetBytes(vec.X); Array.Reverse(xB);
        byte[] yB = BitConverter.GetBytes(vec.Y); Array.Reverse(yB);
        byte[] zB = BitConverter.GetBytes(vec.Z); Array.Reverse(zB);

        // the payload of bytes to send into melee's memory
        List<byte> payload = [];
        payload.AddRange(xB);
        payload.AddRange(yB);
        payload.AddRange(zB);

        byte[] data = [.. payload];
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), data, data.Length, out _);
    }

    #endregion

    public static unsafe T ReadStruct<T>(nint ptr) where T : struct {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];

        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + ptr), buffer, size, out _);

        // commented for now?
        // this puts the correct bytes in the correct fields, but they are backward (cuz we're in big endian world right now)
        // var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        T result = Unsafe.As<byte, T>(ref buffer[0]);

        // reverses endianness of the field
        EndiannessMarshaler.FixEndianness(ref result);

        return result;
    }

    public static unsafe void WriteStruct<T>(nint ptr, T value) where T : struct {
        T copy = value;

        // lil endian to beeg endian
        EndiannessMarshaler.FixEndianness(ref copy);

        // prep buffer
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];

        // copies struct data to byte array
        fixed (byte* bPtr = buffer) {
            Unsafe.Copy(bPtr, ref copy);
        }

        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + ptr), buffer, size, out _);
    }

    #endregion
}