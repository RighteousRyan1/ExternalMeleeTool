using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool;

#pragma warning disable IDE0079 // seriously. why?
#pragma warning disable CA2020, SYSLIB1054 // avoids marshaling warnings which are not necessary

[StructLayout(LayoutKind.Sequential)]
public struct MEMORY_BASIC_INFORMATION {
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
    public Vector3 Position;
    public CharacterKind CharKind;

    public SlotKind SlotKind;
    public SlotTeam Team;
    
    // why did HAL make direction a float? the world will forever be wondering
    // maybe i should change it to a s8 myself
    public float Direction;
    public short Percent;

    // and why did HAL allow stocks to be negative semantically???
    public sbyte Stocks;

    public readonly string FriendlyString() => $"Fighter: {CharKind} | {Position}";
    public override readonly string ToString() => $"FighterBlock(CKind={CharKind}, Pos={Position}, SKind={SlotKind}, Team={Team}, Dir={Direction}, %={Percent}, Stocks={Stocks})";
}
public struct MatchSettings {
    public bool IsTeams;
    public GroundKind StageId;
}

public struct GlobalMeleeData {
    public byte MinorScene;
    public byte MajorScene;
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
    public static float ReadF32(long offset) {
        byte[] buffer = new byte[4];
        // read 4 bytes for a 32 bit single
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 4, out _);
        Array.Reverse(buffer); // Big Endian -> Little Endian
        return BitConverter.ToSingle(buffer, 0);
    }
    public static int ReadS32(long offset) {
        byte[] buffer = new byte[4];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 4, out _);
        Array.Reverse(buffer);
        return BitConverter.ToInt32(buffer, 0);
    }
    public static uint ReadU32(long offset) {
        return (uint)ReadS32(offset);
    }
    public static byte ReadU8(long offset) {
        byte[] buffer = new byte[1];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 1, out _);
        return buffer[0];
    }
    public static short ReadS16(long offset) {
        byte[] buffer = new byte[2];
        SysLib.ReadProcessMemory(_dolphin, (IntPtr)(GALE01 + offset), buffer, 2, out _);
        Array.Reverse(buffer);
        return BitConverter.ToInt16(buffer, 0);
    }
    public static ushort ReadU16(long offset) {
        return (ushort)ReadS16(offset);
    }
    public static sbyte ReadS8(long offset) {
        byte rawValue = ReadU8(offset);
        return (sbyte)rawValue;
    }
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
    // whenever needed
    static byte[] Unpack(byte value) {
        byte[] bits = new byte[8];
        for (int i = 0; i < 8; i++) {
            bits[i] = (byte)((value >> i) & 1);
        }
        return bits;
    }
    // high-level
    public static FighterBlock GetMeleeFighterBlock(FighterMemorySlot slot) {
        long block = (long)slot;
        var playerBlock = new FighterBlock {
            Position      = ReadVec3(block + 0x10), // 0x10 is with 2 bits of padding after nametag position (transform_position)
            CharKind     = (CharacterKind)ReadS32(block + 0x4),
            SlotKind     = (SlotKind)ReadS32(block + 0x8),
            Team         = (SlotTeam)ReadU8(block + 0x47),
            Direction    = ReadF32(block + 0x40),
            Percent      = ReadS16(block + 0x60),
            Stocks       = ReadS8(block + 0x8E)
        };

        return playerBlock;
    }
    public static MatchSettings GetMatchSettings() {
        var settings = new MatchSettings {
            IsTeams = GetIsTeams(),
            StageId = GetStageId()
        };
        return settings;
    }

    public static GlobalMeleeData GetGlobalMeleeData() {
        var data = new GlobalMeleeData {
            MinorScene = ReadU8(MeleeConstants.MINOR_SCENE),
            MajorScene = ReadU8(MeleeConstants.MAJOR_SCENE)
        };
        return data;
    }

    public static void SetMeleeCamera(Vector3 origin, Vector3 focus, float fov) {
        // the payload of bytes to send into melee's memory
        List<byte> payload = [];

        // important to write the focus first since it's *before* the eye in memory
        payload.AddRange(FloatToBigEndian(focus.X));
        payload.AddRange(FloatToBigEndian(focus.Y));
        payload.AddRange(FloatToBigEndian(focus.Z * -1)); // Invert Z per EMC.py

        // eye/origin, written after
        payload.AddRange(FloatToBigEndian(origin.X));
        payload.AddRange(FloatToBigEndian(origin.Y));
        payload.AddRange(FloatToBigEndian(origin.Z * -1)); // Invert Z per EMC.py

        // camera fov
        payload.AddRange(FloatToBigEndian(fov));

        byte[] data = [.. payload];
        SysLib.WriteProcessMemory(_dolphin, (IntPtr)(GALE01 + MeleeConstants.CAM_START), data, data.Length, out _);
    }

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
    static long PerformSignatureScan(byte[] pattern) {
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

        return PerformSignatureScan(pattern);
    }

    public static bool GetIsTeams() => ReadU8(MeleeConstants.START_MELEE_RULES + 0x8) == 1;
    public static GroundKind GetStageId() => (GroundKind)ReadU16(MeleeConstants.START_MELEE_RULES + 0xE);
}
