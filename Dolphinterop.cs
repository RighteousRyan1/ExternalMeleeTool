using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Marshaling;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Collision;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
    public const string VANILLA_MD5 = "0e63d4223b01d9aba596259dc155a174";

    const uint MEM_COMMIT = 0x1000;
    const uint PAGE_READWRITE = 0x04;
    const uint PAGE_WRITECOPY = 0x08;
    const uint PAGE_EXECUTE_READWRITE = 0x40;
    const uint PAGE_EXECUTE_WRITECOPY = 0x80;

    static Process? _process;
    public static IntPtr Handle { get; private set; }

    /// <summary>Where melee's ROM starts.</summary>
    public static long GameCube { get; private set; }
    /// <summary>Location of (typically) Melee's RAM in system memory. (MeleeROM + 0x80000000)</summary>
    public static long MeleeRAM { get; private set; } = 0;
    public static string GameId { get; private set; } = string.Empty;
    /// <summary>If GALE01 has been found in system memory.</summary>
    public static bool IsConnected => _process != null && !_process.HasExited && MeleeRAM != 0;

    public static bool GetISOPaths() {
        if (!IsConnected) return false;

        var procPath = _process.MainModule.FileName;

        return true;
    }
    /// <summary>
    /// Attempts to connect to a running instance of Slippi Dolphin and locate Melee's GALE01 module in memory using an AoB scan.
    /// </summary>
    /// <returns><c>true</c> if the scan was successful, otherwise <c>false</c>.</returns>
    public static bool Connect(params string[] gameIds) {
        try {
            _process = Process.GetProcessesByName("Slippi Dolphin").FirstOrDefault();
            
            // if Ishiiruka check fails, try mainline
            _process ??= Process.GetProcessesByName("Slippi_Dolphin").FirstOrDefault();

            // otherwise, use regular dolphin
            _process ??= Process.GetProcessesByName("Dolphin").FirstOrDefault();

            if (_process == null) return false;

            Handle = _process.Handle;
            var result = GameSignatureScan(gameIds);
            MeleeRAM = result.Offset;
            GameCube = MeleeRAM - MeleePointers.ROM_SIZE;
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


        // pointer storage
        var gobj = ReadPtr(fd.IsTransformed ? block + 0xB4 : block + 0xB0);
        fd.GObj = Read<GObj>(gobj);

        ReadFromFighterPtr(ref fd, fd.GObj.user_data);
        
        return fd;
    }

    public static void ReadFromFighterPtr(ref FighterData fd, Ptr32 ptr) {
        fd.FighterPtr = ptr;

        fd.AnimState = (FtAnimState)ReadS32(fd.FighterPtr + 0x10);
        fd.ActionId = ReadS32(fd.FighterPtr + 0x14);

        fd.CharKind = (FtKind)ReadS32(fd.FighterPtr + 0x4);

        fd.Port = ReadU8(fd.FighterPtr + 0xC);

        //Console.WriteLine("test: " + ReadU8(fd.FighterPtr + 0x119F));
        
        // victim_gobj = fp+1A58, 1A5C = ?
        // magically stupid
        /*if (fd.IsTransformed) {
            if (FighterData.SubCharMap.TryGetValue(kind, out FtKind value)) {
                fd.CharKind = value;
            }
        }
        else {
            fd.CharKind = kind;
        }*/

        // hurt capsule stuff
        // fd.Hurtboxes = new FighterHurtCapsule[15];
        for (int i = 0; i < FighterData.FighterHurtCapsuleBuffer15.LENGTH; i++) {
            var readOffset = fd.FighterPtr + 0x11A0 + (FighterHurtCapsule.SIZE * i);
            fd.Hurtboxes[i] = Read<FighterHurtCapsule>(readOffset);
        }

        for (int i = 0; i < FighterData.HitCapsuleBuffer6.LENGTH; i++) {
            var readOffset = fd.FighterPtr + 0x914 + (HitCapsule.SIZE * i);
            fd.Hitboxes[i] = Read<HitCapsule>(readOffset);
        }

        // these positions are part of the same union
        // 0x1c = transformed char pos
        // 0x10 = main player pos
        // var posPtr = fd.IsTransformed ? block + 0x1C : block + 0x10;
        fd.PositionPtr = fd.FighterPtr + 0xB0;
        fd.Position = ReadVec3(fd.PositionPtr); // ReadVec3(posPtr);
        fd.VelocitySelf = ReadVec3(fd.FighterPtr + 0x80);
        fd.Knockback = ReadVec3(fd.FighterPtr + 0x8C);
        fd.ShieldHealth = ReadF32(fd.FighterPtr + 0x1998);
        fd.Scale = ReadVec3(fd.FighterPtr + 0x34);

        // isnt there also grounded velocity...? will check
        fd.VelocityCombined = fd.VelocitySelf + fd.Knockback;

        fd.Input = Read<FighterInput>(fd.FighterPtr + 0x620);
        fd.AnimFrame = ReadF32(fd.FighterPtr + 0x894);
        fd.AnimRate = ReadF32(fd.FighterPtr + 0x89C);
        fd.AnimTree = Read<FigATree>(ReadPtr(fd.FighterPtr + 0x598));

        fd.BonesPtr = ReadPtr(fd.FighterPtr + 0x5E8);
        fd.Attr = /*new StructHint<FtCommonAttr>(fd.FighterPtr + 0x110);*/ Read<FtCommonAttr>(fd.FighterPtr + 0x110);
        /*nint jobj_parent = ReadPtr(head_jobj + 0xC);

        while (jobj_parent != MeleeConstants.ROM_SIZE) {
            var vec = ReadVec3(jobj_parent + 0x38);
            Console.WriteLine(vec); // position
            jobj_parent = ReadPtr(jobj_parent + 0xC);
        }*/
        fd.DObjs = Read<DObjList>(fd.FighterPtr + 0x5EC);

        fd.Grounded = ReadS32(fd.FighterPtr + 0xE0) == 1;
        fd.CollDataPtr = fd.FighterPtr + 0x6F0;
        fd.CollData = Read<CollData>(fd.CollDataPtr);
    }

    // non-api

    // big endian since GC architecture is big endian... very important
    public static byte[] FloatToBigEndian(float val) {
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
               SysLib.VirtualQueryEx(Handle, (IntPtr)currentAddress, out memInfo, (uint)Marshal.SizeOf(memInfo)) != 0) {

            /* checks if:
             * 1) memory is actually WRITABLE memory
             * 2) if memory has read/write permissions to prevent memory access violations
             */
            bool isWritable = (memInfo.Protect & (PAGE_READWRITE | PAGE_WRITECOPY | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY)) != 0;

            if (memInfo.State == MEM_COMMIT && isWritable) {
                // dolphin mem1 is always 32mb, aka 0x2000000
                // aka... the length of RAM, where ROM is 0x80000000 long.
                if (memInfo.RegionSize == MeleePointers.RAM_SIZE) {
                    // if we're here, we've found the 32MB section of the GC ram.
                    // now check and assign to GALE01 (the game's code)
                    for (int i = 0; i < patterns.Length; i++) {
                        var curPattern = patterns[i];
                        var buffer = new byte[curPattern.Length];
                        if (SysLib.ReadProcessMemory(Handle, memInfo.BaseAddress, buffer, curPattern.Length, out _)) {
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
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 1, out _);
        return buffer[0];
    }
    /// <summary>Reads a signed 16-bit integer from a given GameCube offset.</summary>
    public static short ReadS16(long offset) {
        byte[] buffer = new byte[2];
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 2, out _);
        Array.Reverse(buffer);
        return BitConverter.ToInt16(buffer, 0);
    }
    /// <summary>Reads an unsigned 16-bit integer from a given GameCube offset.</summary>
    public static ushort ReadU16(long offset) => (ushort)ReadS16(offset);

    /// <summary>Reads a signed 32-bit integer from a given GameCube offset.</summary>
    public static int ReadS32(long offset) {
        byte[] buffer = new byte[4];
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 4, out _);
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
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 4, out _);
        Array.Reverse(buffer); // Big Endian -> Little Endian
        return BitConverter.ToSingle(buffer, 0);
    }
    /// <summary>
    /// Reads a char pointer into memory, ending with null-termination.
    /// </summary>
    /// <param name="offset">The offset of the char*.</param>
    /// <returns></returns>
    public static string ReadString(long offset) {
        long cur = offset;
        int length = 0;

        // find null terminator
        while (ReadU8(cur) != 0) {
            length++;
            cur++;
        }

        if (length == 0)
            return string.Empty;

        // read bytes
        byte[] buffer = new byte[length];
        for (int i = 0; i < length; i++) {
            buffer[i] = ReadU8(offset + i);
        }
        return Encoding.ASCII.GetString(buffer);
    }
    #endregion

    #region Non-Primitive Reads
    /// <summary>Reads two (2) 32-bit floats in sequential order from a given GameCube offset to construct a <see cref="Vector2"/>.</summary>
    public static Vector2 ReadVec2(long offset) {
        byte[] buffer = new byte[8];
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 8, out _);

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
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 12, out _);

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
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 16, out _);
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
        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + offset), buffer, 16, out _);

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
        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + offset), [(byte)value], 1, out _);
    }
    public static void WriteU8(long offset, byte value) => WriteS8(offset, (sbyte)value);
    public static void WriteS16(long offset, short value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + offset), bytes, 2, out _);
    }
    public static void WriteU16(long offset, ushort value) => WriteS16(offset, (short)value);
    public static void WriteS32(long offset, int value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + offset), bytes, 4, out _);
    }
    public static void WriteU32(long offset, uint value) => WriteS32(offset, (int)value);
    public static void WriteF32(long offset, float value) {
        byte[] bytes = BitConverter.GetBytes(value);
        Array.Reverse(bytes);
        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + offset), bytes, 4, out _);
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
        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + offset), data, data.Length, out _);
    }

    public static void WriteVec2(long offset, Vector2 vec) {
        byte[] xB = BitConverter.GetBytes(vec.X); Array.Reverse(xB);
        byte[] yB = BitConverter.GetBytes(vec.Y); Array.Reverse(yB);

        // the payload of bytes to send into melee's memory
        List<byte> payload = [];
        payload.AddRange(xB);
        payload.AddRange(yB);

        byte[] data = [.. payload];
        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + offset), data, data.Length, out _);
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
    public static unsafe T Read<[DynamicallyAccessedMembers(EndiannessMarshaler.naot_safety)] T>(long ptr) where T : unmanaged {
        int size = Marshal.SizeOf<T>();
        byte[] buffer = new byte[size];

        SysLib.ReadProcessMemory(Handle, (IntPtr)(GameCube + ptr), buffer, size, out _);

        // commented for now?
        // this puts the correct bytes in the correct fields, but they are backward (cuz we're in big endian world right now)
        // var handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        T result = Unsafe.As<byte, T>(ref buffer[0]);

        // reverses endianness of the field
        EndiannessMarshaler.FixEndianness(ref result);

        // T* = &result?
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
    public static unsafe void Write<[DynamicallyAccessedMembers(EndiannessMarshaler.naot_safety)] T>(long ptr, T value) where T : unmanaged {
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

        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + ptr), buffer, size, out _);
    }
    // not working yet, but gets the offset
    public static unsafe void WriteSpecific<TStruct, TValue>(long ptr, TStruct structure, TValue value) where TStruct : unmanaged {
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

        SysLib.WriteProcessMemory(Handle, (IntPtr)(GameCube + ptr), buffer, size, out _);
    }
    #endregion
}