using ExternalMeleeTool.Marshaling;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Utilities;
using System.Diagnostics;
using System.Numerics;
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
public class Dolphinterop {
    // store local copies of fighterdata to reduce overhead?

    const uint MEM_COMMIT = 0x1000;
    const uint PAGE_READWRITE = 0x04;
    const uint PAGE_WRITECOPY = 0x08;
    const uint PAGE_EXECUTE_READWRITE = 0x40;
    const uint PAGE_EXECUTE_WRITECOPY = 0x80;

    static Process? _process;
    static IntPtr _dolphin;

    /// <summary>Where melee's ROM starts.</summary>
    public static long MeleeROM { get; private set; }
    /// <summary>Location of (typically) GALE01 in system memory. (MeleeROM + 0x80000000)</summary>
    public static long MeleeRAM { get; private set; } = 0;
    public static string GameId { get; private set; } = string.Empty;
    /// <summary>If GALE01 has been found in system memory.</summary>
    public static bool IsConnected => _process != null && !_process.HasExited && MeleeRAM != 0;

    /// <summary>
    /// Attempts to connect to a running instance of Slippi Dolphin and locate Melee's GALE01 module in memory using an AoB scan.
    /// </summary>
    /// <returns><c>true</c> if the scan was successful, otherwise <c>false</c>.</returns>
    public static bool Connect(params string[] gameIds) {
        try {
            _process = Process.GetProcessesByName("Slippi Dolphin").FirstOrDefault();
            
            // if Ishiiruka check fails, try mainline
            _process ??= Process.GetProcessesByName("Slippi_Dolphin").FirstOrDefault();

            if (_process == null) return false;

            _dolphin = _process.Handle;
            var result = GameSignatureScan(gameIds);
            MeleeRAM = result.Offset;
            MeleeROM = MeleeRAM - MeleeGlobals.ROM_SIZE;
            GameId = result.GameId ?? string.Empty;
        } catch {
            // prevent any errors
            return false;
        }

        return MeleeRAM != 0;
    }

    // high-level
    /// <summary>
    /// Loads the player block at the given location in memory.
    /// </summary>
    /// <param name="slot">The fighter slot to load.</param>
    /// <returns>The loaded fighter block.</returns>
    public static FighterData GetMeleeFighterBlock(FighterMemorySlot slot) {
        Ptr32 block = (uint)slot;
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

        var kind = (CKind)ReadS32(block + 0x4);

        if (fd.IsTransformed) {
            if (FighterData.SubCharMap.TryGetValue(kind, out CKind value)) {
                fd.CharKind = value;
            }
        }
        else {
            fd.CharKind = kind;
        }


        // pointer storage
        fd.GObjPtr = ReadPtr(block + 0xB0);
        fd.FighterPtr = ReadPtr(fd.GObjPtr + 0x2C);
        fd.AnimState = (FtAnimState)ReadS32(fd.FighterPtr + 0x10);

        // hurt capsule stuff
        // fd.Hurtboxes = new FighterHurtCapsule[15];
        for (int i = 0; i < FighterData.FighterHurtCapsuleBuffer15.LENGTH; i++) {
            var readOffset = fd.FighterPtr + 0x11A0 + (FighterHurtCapsule.SIZE * i);
            fd.Hurtboxes[i] = Read<FighterHurtCapsule>(readOffset);
        }

        for (int i = 0; i < FighterData.HitCapsuleBuffer4.LENGTH; i++) {
            var readOffset = fd.FighterPtr + 0x914 + (HitCapsule.SIZE * i);
            fd.Hitboxes[i] = Read<HitCapsule>(readOffset);
        }

        // these positions are part of the same union
        // 0x1c = transformed char pos
        // 0x10 = main player pos
        fd.PositionPtr = fd.IsTransformed ? block + 0x1C : block + 0x10;
        fd.Position = ReadVec3(fd.PositionPtr);
        fd.VelocitySelf = ReadVec3(fd.FighterPtr + 0x80);
        fd.Knockback = ReadVec3(fd.FighterPtr + 0x8C);
        fd.ShieldHealth = ReadF32(fd.FighterPtr + 0x1998);

        fd.Input = Read<GCInput>(fd.FighterPtr + 0x620);

        fd.BonesPtr = ReadPtr(fd.FighterPtr + 0x5E8);
        fd.Attr = Read<FtCommonAttr>(fd.FighterPtr + 0x110);
        /*nint jobj_parent = ReadPtr(head_jobj + 0xC);

        while (jobj_parent!= MeleeConstants.ROM_SIZE) {
            var vec = ReadVec3(jobj_parent + 0x38);
            Console.WriteLine(vec); // position
            jobj_parent = ReadPtr(jobj_parent + 0xC);
        }*/

        fd.Grounded = ReadS32(fd.FighterPtr + 0xE0);
        fd.CollDataPtr = fd.FighterPtr + 0x6F0;
        fd.CollData = Read<CollData>(fd.CollDataPtr);

        return fd;
    }

    /// <summary>
    /// Loads the current match settings.
    /// </summary>
    public static MatchData GetMatchData() {
        var data = new MatchData {
            IsTeams = ReadU8(MeleeGlobals.START_MELEE_RULES + 0x8) == 1,
            // this frame parameter needs a lot of help...
            Frame = ReadS16(MeleeGlobals.MATCH_INFO + 0x2C /*0x46b6cc*/), // ReadS16(MeleeConstants.MATCH_INFO + 0x2C), //
            Fighters = new FighterData[4],
            // and not == 1? who tf made this crap?
            IsPaused = ReadU8(MeleeGlobals.PAUSE_BIT) == 2
        };

        //Console.WriteLine("sfe: " + data.Frame);
        //Console.WriteLine("f_c: " + ReadS32(0x8046b6c4));
        //Console.WriteLine("t_s: " + ReadS32(0x8046b6c8));

        data.Fighters[0] = GetMeleeFighterBlock(FighterMemorySlot.IndexOne);
        data.Fighters[0].Port = 0;
        data.Fighters[1] = GetMeleeFighterBlock(FighterMemorySlot.IndexTwo);
        data.Fighters[1].Port = 1;
        data.Fighters[2] = GetMeleeFighterBlock(FighterMemorySlot.IndexThree);
        data.Fighters[2].Port = 2;
        data.Fighters[3] = GetMeleeFighterBlock(FighterMemorySlot.IndexFour);
        data.Fighters[3].Port = 3;
        return data;
    }
    // move back to this later
    public static StageData GetStageData() {
        const uint stinfo = MeleeGlobals.STAGE_INFO;
        Ptr32 coll_data_ptr = ReadPtr(stinfo + 0x6AC);
        // read as a sanity check first and foremost to prevent bad data reads
        int vertCount = ReadS32(coll_data_ptr + 0x4);

        // i feel like in most cases if data isnt initialized and it reads garbage
        // it won't be near 0 or below 1000, so check it
        if (vertCount <= 0 || vertCount >= 1000)
            return default;

        Ptr32 grParam_ptr = ReadPtr(stinfo + 0x6B0);

        //int lineCount = ReadS32(coll_data_ptr + 0xC);
        //int jointCount = ReadS32(coll_data_ptr + 0x28);

        /*var collJointHead = Read<CollJoint>(MeleeGlobals.COLL_JOINT_HEAD);
        var nextColl = Read<CollJoint>(collJointHead.next, -MeleeGlobals.ROM_SIZE);

        int num = 0;

        // linked list traversal...
        // does the head have zero important data???
        // TODO: get working. fails
        while (nextColl.next != 0) {
            if (nextColl.Equals(collJointHead))
                break;

            //var s = nextColl.FieldsToString() + "\norig:\n" + collJointHead.FieldsToString();

            nextColl = Read<CollJoint>(nextColl.next, -MeleeGlobals.ROM_SIZE);

            var jobj = Read<HSD_JObj>(nextColl.x20_jobj_ptr, -MeleeGlobals.ROM_SIZE);
            //Console.WriteLine($"{jobj.FieldsToString()}");

            num++;
        }*/

        // Console.WriteLine(num);

        var coll = Read<MapCollData>(coll_data_ptr);

        // NAOT sanity check?
        // update: NAOT always makes vert_count garbo data. why tf?
        if (coll.vert_count > 1000)
            return default;

        var verts = new Vector2[coll.vert_count];
        var lines = new MapLine[coll.line_count];
        // var joints = new MapJoint[coll.joint_count];

        // vert_count is seemingly garbage data when using NAOT.... what?
        for (int i = 0; i < coll.vert_count; i++) {
            // subtract rom size cuz the pointers are in location respecting the entire system memory
            verts[i] = Read<Vector2>(coll.verts + (i * 8)); // 8 bytes per Vector2 (x, y)
        }
        for (int i = 0; i < coll.line_count; i++) {
            lines[i] = Read<MapLine>(coll.lines + (i * MapLine.SIZE));
        }

        // why is this just giving me a struct full of zeros?
        /*for (int i = 0; i < coll.joint_count; i++) {
            joints[i] = Read<MapJoint>(coll.joints + (i * MapJoint.SIZE), -MeleeGlobals.ROM_SIZE);
        }*/

        var data = new StageData {
            StageId = (ExternalStageId)ReadU16(MeleeGlobals.START_MELEE_RULES + 0xE),
            // Scale = stageScale,
            GroundParams = Read<GrParam>(grParam_ptr),
            BlastZone = Read<BoundingRect>(stinfo + 0x74), //ReadBoundingRect(stinfo + 0x74),
            // 0x0 = camerainfo
            CameraInfo = Read<StageCameraInfo>(stinfo),
            MapLines = lines,
            Vertices = verts,
            Collision = coll,
            // MapJoints = joints
        };

        return data;
    }

    /// <summary>
    /// Loads global melee data.
    /// </summary>
    public static GlobalMeleeData GetGlobalData() {
        var data = new GlobalMeleeData {
            MinorScene = ReadU8(MeleeGlobals.MINOR_SCENE),
            MajorScene = ReadU8(MeleeGlobals.MAJOR_SCENE)
        };
        return data;
    }

    public static SlippiOnlineData GetOnlineData(GlobalMeleeData gmd) {
        var data = new SlippiOnlineData {
            ClientPort = SlippiOnlineData.GetClientPort(gmd),
            ClientControllerPort = ReadU8(ReadPtr(SlippiGlobals.ONLINE_DATA_BLOCK + 0x2)),
            InOnlineMatch = SlippiOnlineData.IsSlippiOnline(gmd),
            Frame = ReadU8(ReadPtr(SlippiGlobals.ONLINE_DATA_BLOCK + 0x3))
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
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeRAM + MeleeGlobals.CAM_START), data, data.Length, out _);
    }
    /// <summary>
    /// Sets the type of camera melee will use.
    /// </summary>
    /// <param name="type">The kind of camera melee will use to set its render matrices to.</param>
    public static void SetCameraType(CameraKind type) {
        // 0x08 = develop camera offset
        WriteU8(MeleeGlobals.CAM_TYPE, (byte)type);
    }

    // non-api

    // big endian since GC architecture is big endian... very important
    static byte[] FloatToBigEndian(float val) {
        byte[] b = BitConverter.GetBytes(val);
        Array.Reverse(b);
        return b;
    }

    // scan's melee's AoB
    static (string?, long) PerformAoBScan(params byte[][] patterns) {
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
                if (memInfo.RegionSize == MeleeGlobals.RAM_SIZE) {
                    // if we're here, we've found the 32MB section of the GC ram.
                    // now check and assign to GALE01 (the game's code)
                    for (int i = 0; i < patterns.Length; i++) {
                        var curPattern = patterns[i];
                        var buffer = new byte[curPattern.Length];
                        if (SysLib.ReadProcessMemory(_dolphin, memInfo.BaseAddress, buffer, curPattern.Length, out _)) {
                            if (PatternMatch(buffer, curPattern)) {
                                // return the the buffer to a string for readability
                                var curPatternStr = System.Text.Encoding.ASCII.GetString(curPattern);
                                return (curPatternStr, memInfo.BaseAddress);
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

        return (null, 0); // :(
    }
    static bool PatternMatch(byte[] data, byte[] pattern) {
        if (data.Length < pattern.Length) return false;
        for (int i = 0; i < pattern.Length; i++) {
            // this ensures we found GALE01's header, since we check the first 8 bytes found in melee's memory (typically!)
            if (data[i] != pattern[i]) return false;
        }
        return true;
    }
    static (string? GameId, long Offset) GameSignatureScan(params string[] gameIds) {
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
    /// <summary>Reads a signed 8-bit integer from a given GameCube offset.</summary>
    public static sbyte ReadS8(long offset) {
        byte rawValue = ReadU8(offset);
        return (sbyte)rawValue;
    }
    /// <summary>Reads an unsigned 8-bit integer from a given GameCube offset.</summary>
    public static byte ReadU8(long offset) {
        byte[] buffer = new byte[1];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 1, out _);
        return buffer[0];
    }
    /// <summary>Reads a signed 16-bit integer from a given GameCube offset.</summary>
    public static short ReadS16(long offset) {
        byte[] buffer = new byte[2];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 2, out _);
        Array.Reverse(buffer);
        return BitConverter.ToInt16(buffer, 0);
    }
    /// <summary>Reads an unsigned 16-bit integer from a given GameCube offset.</summary>
    public static ushort ReadU16(long offset) => (ushort)ReadS16(offset);

    /// <summary>Reads a signed 32-bit integer from a given GameCube offset.</summary>
    public static int ReadS32(long offset) {
        byte[] buffer = new byte[4];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 4, out _);
        Array.Reverse(buffer);
        return BitConverter.ToInt32(buffer, 0);
    }
    /// <summary>Reads an unsigned 32-bit float from a given GameCube offset.</summary>
    public static uint ReadU32(long offset) => (uint)ReadS32(offset);

    /// <summary>Reads an memory address from a given GameCube offset.</summary>
    public static Ptr32 ReadPtr(long offset) => new(ReadU32(offset));

    /// <summary>Reads a 32-bit float from a given GameCube offset.</summary>
    public static float ReadF32(long offset) {
        byte[] buffer = new byte[4];
        // read 4 bytes for a 32 bit single
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 4, out _);
        Array.Reverse(buffer); // Big Endian -> Little Endian
        return BitConverter.ToSingle(buffer, 0);
    }
    #endregion

    #region Non-Primitive Reads
    /// <summary>Reads two (2) 32-bit floats in sequential order from a given GameCube offset to construct a <see cref="Vector2"/>.</summary>
    public static Vector2 ReadVec2(long offset) {
        byte[] buffer = new byte[8];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 8, out _);

        byte[] xB = buffer[0..4]; Array.Reverse(xB);
        byte[] yB = buffer[4..8]; Array.Reverse(yB);

        return new Vector2(
            BitConverter.ToSingle(xB, 0),
            BitConverter.ToSingle(yB, 0)
        );
    }
    /// <summary>Reads three (3) 32-bit floats in sequential order from a given GameCube offset to construct a <see cref="Vector3"/>.</summary>
    public static Vector3 ReadVec3(long offset) {
        // 4 bytes per float
        byte[] buffer = new byte[12];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 12, out _);

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
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 16, out _);
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

    static BoundingRect ReadBoundingRect(long offset) {
        byte[] buffer = new byte[16];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), buffer, 16, out _);

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
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), [(byte)value], 1, out _);
    }
    public static void WriteU8(long offset, byte value) => WriteS8(offset, (sbyte)value);
    public static void WriteS16(long offset, short value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), bytes, 2, out _);
    }
    public static void WriteU16(long offset, ushort value) => WriteS16(offset, (short)value);
    public static void WriteS32(long offset, int value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), bytes, 4, out _);
    }
    public static void WriteU32(long offset, uint value) => WriteS32(offset, (int)value);
    public static void WriteF32(long offset, float value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), bytes, 4, out _);
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
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), data, data.Length, out _);
    }

    public static void WriteVec2(long offset, Vector2 vec) {
        byte[] xB = BitConverter.GetBytes(vec.X); Array.Reverse(xB);
        byte[] yB = BitConverter.GetBytes(vec.Y); Array.Reverse(yB);

        // the payload of bytes to send into melee's memory
        List<byte> payload = [];
        payload.AddRange(xB);
        payload.AddRange(yB);

        byte[] data = [.. payload];
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + offset), data, data.Length, out _);
    }

    #endregion

    /// <summary>
    /// Reads a value of type <typeparamref name="T"/> from the specified memory address.
    /// </summary>
    /// <remarks>This method reads raw bytes from process memory at the computed address (<see cref="MeleeRAM"/> + <paramref name="ptr"/> + <paramref name="offset"/>) and converts them into a value of type <typeparamref name="T"/>. 
    /// <br></br>The endianness of the resulting value is corrected to match the system's endianness. Use this method only with
    /// unmanaged value types.</remarks>
    /// <typeparam name="T">The value type to read from memory. Must be an unmanaged type (<see langword="struct"/>).</typeparam>
    /// <param name="ptr">The base memory address from which to read the value.</param>
    /// <param name="offset">An optional offset to add to <paramref name="ptr"/> when calculating the final memory address.</param>
    /// <returns>The value of type <typeparamref name="T"/> read from the specified memory location, with its endianness adjusted as needed.</returns>
    public static unsafe T Read<T>(long ptr) where T : unmanaged {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];

        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(MeleeROM + ptr), buffer, size, out _);

        // commented for now?
        // this puts the correct bytes in the correct fields, but they are backward (cuz we're in big endian world right now)
        // var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        T result = Unsafe.As<byte, T>(ref buffer[0]);

        // reverses endianness of the field
        EndiannessMarshaler.FixEndianness(ref result);

        return result;
    }
    /// <summary>
    /// Writes the specified value of type <typeparamref name="T"/> to the target process memory at the given offset.
    /// </summary>
    /// <remarks>This method marshals the value to a byte array, adjusts for endianness, and writes it to the
    /// target process memory at the specified offset. The caller must ensure that the offset and value are valid for
    /// the target process. This method is unsafe and should be used with caution, as writing to invalid memory
    /// locations can cause the target process to become unstable.</remarks>
    /// <typeparam name="T">The value type to write. Must be a value type (<see langword="struct"/>).</typeparam>
    /// <param name="ptr">The offset, in bytes, from the base address at which to write the value. Must be within the valid address range
    /// of the target process.</param>
    /// <param name="value">The value to write to the target process memory. The value is marshaled according to the system's endianness.</param>
    public static unsafe void Write<T>(long ptr, T value) where T : unmanaged {
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

        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + ptr), buffer, size, out _);
    }
    // not working yet, but gets the offset
    public static unsafe void WriteSpecific<TStruct, TValue>(long ptr, TStruct structure, TValue value) where TStruct : struct {
        TStruct copy = structure;

        // lil endian to beeg endian
        EndiannessMarshaler.FixEndianness(ref copy);

        // prep buffer
        int size = Marshal.SizeOf<TStruct>();
        byte[] buffer = new byte[size];

        // copies struct data to byte array
        fixed (byte* bPtr = buffer) {
            Unsafe.Copy(bPtr, ref copy);
        }

        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(MeleeROM + ptr), buffer, size, out _);
    }
    #endregion
}