using System.Diagnostics;
using System.Numerics;
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

// will eventually need sequential if i decide to copy over every Fighter struct item
[StructLayout(LayoutKind.Sequential)]
public struct FighterBlock {
    /// <summary>The position of the fighter.</summary>
    public Vector3 Position;
    /// <summary>The character type.</summary>
    public CharacterKind CharKind;

    /// <summary>The kind of slot of this fighter's memory block.</summary>
    public SlotKind SlotKind;
    /// <summary>The team this fighter belongs to.</summary>
    public SlotTeam Team;

    // why did HAL make direction a float? the world will forever be wondering
    // maybe i should change it to a s8 myself
    /// <summary>Either -1.0 for left-facing or 1.0 for right-facing.</summary>
    public float Direction;
    /// <summary>The damage percent of this fighter.</summary>
    public short Percent;

    // and why did HAL allow stocks to be negative semantically???
    /// <summary>How many stocks this fighter has remaining.</summary>
    public sbyte Stocks;

    public readonly string FriendlyString() => $"Fighter: {CharKind} | {Position}";
    public override readonly string ToString() => $"FighterBlock(CKind={CharKind}, Pos={Position}, SKind={SlotKind}, Team={Team}, Dir={Direction}, %={Percent}, Stocks={Stocks})";
}
/// <summary>A structure representing the match's settings.</summary>
public struct MatchSettings {
    /// <summary>If there is an active teams match, <c>true</c>, else, <c>false</c>.</summary>
    public bool IsTeams;
    /// <summary>The ID of the stage being played on.</summary>
    public ExternalStageId StageId;
}
/// <summary>A structure holding data relating to common melee data that isn't bound to gameplay.</summary>
public struct GlobalMeleeData {
    /// <summary>The 'minor' scene data ID. Typically involves sub-menus.</summary>
    public byte MinorScene;
    /// <summary>The 'major' scene data ID. Typically involves different game states.</summary>
    public byte MajorScene;

    public static bool IsSlippiOnline(GlobalMeleeData gmd) {
        // for whatever reason, this indicates online melee
        return gmd.MinorScene == 8 && gmd.MajorScene == 2;
    }
    // fails if IsSlippiOnline is false
    public static int ClientPort(GlobalMeleeData gmd) {
        if (!IsSlippiOnline(gmd)) return -1;

        var odb_ptr = Slippinterop.ReadU32(SlippiConstants.ONLINE_DATA_BLOCK);

        var cli_port = Slippinterop.ReadU8(odb_ptr - 0x80000000);
        // var guh = $"{port_ptr:X} {Slippinterop.GALE01:X}";

        return cli_port;
    }
}
public class Slippinterop {
    const uint MEM_COMMIT = 0x1000;
    const uint PAGE_READWRITE = 0x04;
    const uint PAGE_WRITECOPY = 0x08;
    const uint PAGE_EXECUTE_READWRITE = 0x40;
    const uint PAGE_EXECUTE_WRITECOPY = 0x80;
    
    // size of GC memory, where all code lies for any GC game
    const long EMU_SIZE = 0x2000000;

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
    public static bool Connect() {
        try {
            _process = Process.GetProcessesByName("Slippi Dolphin").FirstOrDefault();
            if (_process == null) return false;

            _dolphin = _process.Handle;
            GALE01 = GALE01Scan();
        } catch {
            // prevent any errors
            return false;
        }

        return GALE01 != 0;
    }

    // read floats from GC memory specifically
    /// <summary>Reads a 32-bit float from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static float ReadF32(long offset) {
        byte[] buffer = new byte[4];
        // read 4 bytes for a 32 bit single
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 4, out _);
        Array.Reverse(buffer); // Big Endian -> Little Endian
        return BitConverter.ToSingle(buffer, 0);
    }
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
    public static uint ReadU32(long offset) {
        return (uint)ReadS32(offset);
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
    public static ushort ReadU16(long offset) {
        return (ushort)ReadS16(offset);
    }
    /// <summary>Reads a signed 8-bit integer from a given GALE01 offset.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    public static sbyte ReadS8(long offset) {
        byte rawValue = ReadU8(offset);
        return (sbyte)rawValue;
    }
    /// <summary>Reads three (3) 32-bit floats in sequential order from a given GALE01 offset to construct a <see cref="Vector3"/>.</summary>
    /// <remarks>GALE01 is automatically added to the offset.</remarks>
    static Vector3 ReadVec3(long offset) {
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
    public static FighterBlock GetMeleeFighterBlock(FighterMemorySlot slot) {
        long block = (long)slot;
        var playerBlock = new FighterBlock {
            Position     = ReadVec3(block + 0x10), // all part of the same union...
            CharKind     = (CharacterKind)ReadS32(block + 0x4),
            SlotKind     = (SlotKind)ReadS32(block + 0x8),
            Team         = (SlotTeam)ReadU8(block + 0x47),
            Direction    = ReadF32(block + 0x40),
            Percent      = ReadS16(block + 0x60),
            Stocks       = ReadS8(block + 0x8E)
        };

        return playerBlock;
    }

    /// <summary>
    /// Loads the current match settings.
    /// </summary>
    public static MatchSettings GetMatchSettings() {
        var settings = new MatchSettings {
            IsTeams = GetIsTeams(),
            StageId = GetStageId()
        };
        return settings;
    }

    /// <summary>
    /// Loads global melee data.
    /// </summary>
    public static GlobalMeleeData GetGlobalMeleeData() {
        var data = new GlobalMeleeData {
            MinorScene = ReadU8(MeleeConstants.MINOR_SCENE),
            MajorScene = ReadU8(MeleeConstants.MAJOR_SCENE)
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
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + MeleeConstants.CAM_TYPE), [(byte)type], 1, out _);
    }

    // non-api

    // big endian since GC architecture is big endian... very important
    static byte[] FloatToBigEndian(float val) {
        byte[] b = BitConverter.GetBytes(val);
        Array.Reverse(b);
        return b;
    }

    // scan's melee's AoB
    static long PerformAoBScan(byte[] pattern) {
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
                if (memInfo.RegionSize == EMU_SIZE) {
                    // if we're here, we've found the 32MB section of the GC ram.
                    // now check and assign to GALE01 (the game's code)
                    byte[] buffer = new byte[pattern.Length];
                    if (SysLib.ReadProcessMemory(_dolphin, memInfo.BaseAddress, buffer, pattern.Length, out _)) {
                        if (PatternMatch(buffer, pattern)) {
                            return memInfo.BaseAddress;
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

    static long GALE01Scan() {
        // "GALE01" + 0x00 + 0x02
        byte[] pattern = [0x47, 0x41, 0x4C, 0x45, 0x30, 0x31, 0x00, 0x02];

        return PerformAoBScan(pattern);
    }

    static bool GetIsTeams() => ReadU8(MeleeConstants.START_MELEE_RULES + 0x8) == 1;
    static ExternalStageId GetStageId() => (ExternalStageId)ReadU16(MeleeConstants.START_MELEE_RULES + 0xE);
}
