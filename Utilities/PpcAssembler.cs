using System.Text.RegularExpressions;

namespace ExternalMeleeTool.Utilities;
public static class PpcAssembler {

    /// <summary>
    /// Compiles a basic PowerPC instruction string and writes it to memory.
    /// </summary>
    public static void WritePpcInstruction(uint address, string instruction) {
        uint machineCode = AssemblePpc(address, instruction);
        Dolphinterop.WriteU32(address, machineCode);
        // Console.WriteLine($"Wrote {instruction} (0x{machineCode:X8}) to 0x{address:X8}");
    }

    /// <summary>
    /// Converts common PowerPC instruction strings to their U32 machine code equivalents.
    /// </summary>
    public static uint AssemblePpc(uint address, string instruction) {
        instruction = instruction.Trim().ToLowerInvariant();

        // static instrs
        if (instruction == "nop") return 0x60000000; // ori r0, r0, 0
        if (instruction == "blr") return 0x4E800020;

        // branch (b)
        var bMatch = Regex.Match(instruction, @"^b\s+(0x[0-9a-f]+)$");
        if (bMatch.Success) {
            uint dest = Convert.ToUInt32(bMatch.Groups[1].Value, 16);

            // offset = destination - address.
            // masked with 0x03FFFFFC to keep aligned with 24-bit branch limit
            // b uses offsets to determine where to go in memory instead of just jumping to a location statically defined
            uint offset = (dest - address) & 0x03FFFFFC;
            return 0x48000000 | offset;
        }

        // branch + link (bl)
        var blMatch = Regex.Match(instruction, @"^bl\s+(0x[0-9a-f]+)$");
        if (blMatch.Success) {
            uint dest = Convert.ToUInt32(blMatch.Groups[1].Value, 16);
            uint offset = (dest - address) & 0x03FFFFFC;
            return 0x48000001 | offset; // '1' at the end sets the LR
        }

        // load immediate (li)
        var liMatch = Regex.Match(instruction, @"^li\s+r(\d+),\s*(0x[0-9a-f]+|-?\d+)$");
        if (liMatch.Success) {
            uint reg = uint.Parse(liMatch.Groups[1].Value);
            string valStr = liMatch.Groups[2].Value;

            // allows both hex and decimals
            short val = valStr.StartsWith("0x") ?
                (short)Convert.ToUInt16(valStr, 16) :
                short.Parse(valStr);

            // Opcode 14 (0x38000000) | Register << 21 | 16-bit immediate
            // li is basically addi rx, r0, value
            return 0x38000000 | (reg << 21) | (uint)(val & 0xFFFF);
        }

        throw new NotSupportedException($"Instruction '{instruction}' is not supported by the mini-assembler.");
    }
}
