using EMTDisplay.CmdConsole;
using ExternalMeleeTool;
using ExternalMeleeTool.Melee.Collision;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace EMTDisplay;

public static partial class Program {

    [LibraryImport("Kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool AllocConsole();

    public static void Main() {
        AllocConsole();
        var processor = new CommandProcessor();

        RegisterCommands(processor);

        Console.WindowWidth = 65;
        Console.WindowHeight = 20;
        Console.Title = "EMTDisplay Command Window";

        Task.Run(() => {
            while (true) {
                Console.Write("> ");
                var input = Console.ReadLine();
                processor.Process(input);
            }
        });

        using var game = new EMTDisplay();
        game.Run();
    }
    public static void RegisterCommands(CommandProcessor processor) {
        processor.Register("help", "Lists all available commands and their usage.", args => {
            Console.WriteLine("\n--- Melee Console Commands ---");

            // sorts commands alphabetically
            var sortedCommands = processor.GetRegisteredCommands().OrderBy(c => c.Name);

            foreach (var (Name, Description) in sortedCommands) {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{Name,-15}");
                Console.ResetColor();
                Console.WriteLine($"- {Description}");
            }
            Console.WriteLine("-------------------------------\n");
        });

        processor.Register("ftpos", "Sets fighter position: ftpos <id> <vector>", args => {
            if (args.Length < 2) throw new Exception("Not enough arguments.");
            int id = int.Parse(args[0]);
            if (TryParse(args[1], out Vector3 pos)) {
                var fighter = EMTDisplay.Match.Fighters[id];
                //fighter.Position = pos;
                // fighter.Position.Value = pos;
                Dolphinterop.WriteVec3(fighter.FighterPtr + 0xB0, Vector3.Zero);
                Console.WriteLine($"Moved {fighter.CharKind} to {pos}");
            }
        });

        processor.Register("drawschema", "Changes drawing colors: drawinfo <mat, coll, int>", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var drawType = args[0];

            EMTDisplay.drawSchema = drawType switch {
                "mat" => 0,
                "coll" => 1,
                "int" => 2,
                _ => throw new Exception("Invalid draw type.")
            };
            Console.WriteLine("Drawing schema set to " + EMTDisplay.drawSchema + ".");
        });
    }

    public static bool TryParse(string text, out Vector3 result) {
        result = default;

        if (string.IsNullOrWhiteSpace(text))
            return false;

        text = text.Trim();

        // common wrappers
        if ((text.StartsWith('(') && text.EndsWith(')')) ||
            (text.StartsWith('<') && text.EndsWith('>'))) {
            text = text[1..^1];
        }

        // common splitters
        var parts = text.Split([',', ' '], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 3)
            return false;

        if (!float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x))
            return false;
        if (!float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            return false;
        if (!float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out float z))
            return false;

        result = new Vector3(x, y, z);
        return true;
    }
}
