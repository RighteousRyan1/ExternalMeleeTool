using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ExternalMeleeTool; 

#pragma warning disable IDE0079
#pragma warning disable SYSLIB1054 // no libimport cuz it's stupid

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

/// <summary>
/// Reads/writes another process's memory on Windows, Linux and macOS.
/// <br></br>The "process handle" is whatever the OS needs: a process handle (Windows), the pid (Linux) or the mach task port (macOS).
/// </summary>
internal static unsafe class SysLib {
    // windows
    const uint MEM_COMMIT = 0x1000;
    const uint PAGE_READWRITE = 0x04;
    const uint PAGE_WRITECOPY = 0x08;
    const uint PAGE_EXECUTE_READWRITE = 0x40;
    const uint PAGE_EXECUTE_WRITECOPY = 0x80;

    [DllImport("kernel32.dll", EntryPoint = "ReadProcessMemory")]
    static extern bool WinRead(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead);
    [DllImport("kernel32.dll", EntryPoint = "WriteProcessMemory")]
    static extern bool WinWrite(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten);
    [DllImport("kernel32.dll")]
    static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, uint dwLength);

    // linux
    [StructLayout(LayoutKind.Sequential)]
    struct IoVec {
        public byte* Base;
        public nuint Length;
    }
    [DllImport("libc", EntryPoint = "process_vm_readv", SetLastError = true)]
    static extern nint LinuxRead(int pid, IoVec* local, nuint localCount, IoVec* remote, nuint remoteCount, nuint flags);
    [DllImport("libc", EntryPoint = "process_vm_writev", SetLastError = true)]
    static extern nint LinuxWrite(int pid, IoVec* local, nuint localCount, IoVec* remote, nuint remoteCount, nuint flags);

    // macOS
    const int VM_REGION_BASIC_INFO_64 = 9;
    const int VM_REGION_BASIC_INFO_COUNT_64 = 9;
    const int VM_PROT_READ = 1;
    const int VM_PROT_WRITE = 2;

    [DllImport("libSystem")]
    static extern uint mach_task_self();
    [DllImport("libSystem")]
    static extern int task_for_pid(uint targetTask, int pid, out uint task);
    [DllImport("libSystem")]
    static extern int mach_vm_region(uint task, ref ulong address, out ulong size, int flavor, int* info, ref uint infoCount, out uint objectName);
    [DllImport("libSystem")]
    static extern int mach_vm_read_overwrite(uint task, ulong address, ulong size, byte* data, out ulong outSize);
    [DllImport("libSystem")]
    static extern int mach_vm_write(uint task, ulong address, byte* data, uint size);

    static bool _warnedPermissions;

    /// <summary>
    /// Gets what the other methods use to access <paramref name="process"/>.
    /// </summary>
    /// <returns>The handle, or <see cref="IntPtr.Zero"/> if the OS won't let the process be opened.</returns>
    public static IntPtr OpenProcess(Process process) {
        if (OperatingSystem.IsWindows()) return process.Handle;
        if (OperatingSystem.IsMacOS()) {
            // needs root, or dolphin to be signed with debug entitlements
            if (task_for_pid(mach_task_self(), process.Id, out uint task) == 0) return (IntPtr)task;
            Console.Error.WriteLine("macOS denied access to Dolphin's memory. Run as root (sudo), or sign Dolphin with debugging entitlements.");
            return IntPtr.Zero;
        }
        return process.Id;
    }

    public static bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesRead) {
        lpNumberOfBytesRead = 0;
        if (OperatingSystem.IsWindows()) return WinRead(hProcess, lpBaseAddress, lpBuffer, dwSize, out lpNumberOfBytesRead);

        fixed (byte* buffer = lpBuffer) {
            if (OperatingSystem.IsMacOS()) {
                bool ok = mach_vm_read_overwrite((uint)hProcess, (ulong)lpBaseAddress, (ulong)dwSize, buffer, out ulong read) == 0;
                lpNumberOfBytesRead = (int)read;
                return ok && lpNumberOfBytesRead == dwSize;
            }

            var local = new IoVec { Base = buffer, Length = (nuint)dwSize };
            var remote = new IoVec { Base = (byte*)lpBaseAddress, Length = (nuint)dwSize };
            nint n = LinuxRead((int)hProcess, &local, 1, &remote, 1, 0);
            if (n < 0) WarnLinuxPermissions();
            lpNumberOfBytesRead = (int)Math.Max(n, 0);
            return n == dwSize;
        }
    }

    public static bool WriteProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, byte[] lpBuffer, int dwSize, out int lpNumberOfBytesWritten) {
        lpNumberOfBytesWritten = 0;
        if (OperatingSystem.IsWindows()) return WinWrite(hProcess, lpBaseAddress, lpBuffer, dwSize, out lpNumberOfBytesWritten);

        fixed (byte* buffer = lpBuffer) {
            if (OperatingSystem.IsMacOS()) {
                bool ok = mach_vm_write((uint)hProcess, (ulong)lpBaseAddress, buffer, (uint)dwSize) == 0;
                lpNumberOfBytesWritten = ok ? dwSize : 0;
                return ok;
            }

            var local = new IoVec { Base = buffer, Length = (nuint)dwSize };
            var remote = new IoVec { Base = (byte*)lpBaseAddress, Length = (nuint)dwSize };
            nint n = LinuxWrite((int)hProcess, &local, 1, &remote, 1, 0);
            if (n < 0) WarnLinuxPermissions();
            lpNumberOfBytesWritten = (int)Math.Max(n, 0);
            return n == dwSize;
        }
    }

    /// <summary>Lists every readable and writable region of the process's memory, lowest address first.</summary>
    public static IEnumerable<(long Start, long Size)> GetWritableRegions(IntPtr hProcess) {
        if (OperatingSystem.IsWindows()) return WindowsRegions(hProcess);
        if (OperatingSystem.IsMacOS()) return MacRegions(hProcess);
        return LinuxRegions((int)hProcess);
    }

    static IEnumerable<(long Start, long Size)> WindowsRegions(IntPtr hProcess) {
        long maxAddress = 0x7FFFFFFF0000;
        long currentAddress = 0;
        var memInfo = new MEMORY_BASIC_INFORMATION();

        while (currentAddress < maxAddress &&
               VirtualQueryEx(hProcess, (IntPtr)currentAddress, out memInfo, (uint)Marshal.SizeOf(memInfo)) != 0) {
            // memory has to be committed and have write permissions, or reads/writes will fail
            bool isWritable = (memInfo.Protect & (PAGE_READWRITE | PAGE_WRITECOPY | PAGE_EXECUTE_READWRITE | PAGE_EXECUTE_WRITECOPY)) != 0;
            if (memInfo.State == MEM_COMMIT && isWritable)
                yield return ((long)memInfo.BaseAddress, (long)memInfo.RegionSize);

            long regionSize = (long)memInfo.RegionSize;
            if (regionSize == 0) break;
            currentAddress = (long)memInfo.BaseAddress + regionSize;
        }
    }

    static IEnumerable<(long Start, long Size)> LinuxRegions(int pid) {
        // each line looks like "7f1c00000000-7f1c02000000 rw-s 00000000 00:01 1234 /some/file"
        foreach (var line in File.ReadLines($"/proc/{pid}/maps")) {
            var parts = line.Split(' ', 3);
            if (parts.Length < 2 || parts[1].Length < 2 || parts[1][0] != 'r' || parts[1][1] != 'w') continue;

            var range = parts[0].Split('-');
            long start = Convert.ToInt64(range[0], 16);
            yield return (start, Convert.ToInt64(range[1], 16) - start);
        }
    }

    // not an iterator, since those can't use pointers
    static List<(long Start, long Size)> MacRegions(IntPtr hProcess) {
        var regions = new List<(long Start, long Size)>();
        ulong address = 0;
        int* info = stackalloc int[VM_REGION_BASIC_INFO_COUNT_64];

        while (true) {
            uint count = VM_REGION_BASIC_INFO_COUNT_64;
            if (mach_vm_region((uint)hProcess, ref address, out ulong size, VM_REGION_BASIC_INFO_64, info, ref count, out _) != 0) break;

            // the first field of the info is the current protection
            if ((info[0] & (VM_PROT_READ | VM_PROT_WRITE)) == (VM_PROT_READ | VM_PROT_WRITE))
                regions.Add(((long)address, (long)size));
            address += size;
        }
        return regions;
    }

    static void WarnLinuxPermissions() {
        // ptrace_scope (or missing capability) is stopping functionality
        if (_warnedPermissions || Marshal.GetLastPInvokeError() != 1) return;
        _warnedPermissions = true;
        Console.Error.WriteLine("Linux denied access to Dolphin's memory." +
            "\nRun as root, or \"sudo setcap cap_sys_ptrace=eip <this program>\", or \"sudo sysctl kernel.yama.ptrace_scope=0\"");
    }
}
