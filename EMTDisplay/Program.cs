using EMTDisplay.CmdConsole;
using EMTDisplay.Utils;
using ExternalMeleeTool;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EMTDisplay;

/* next on the list:
 * - Projectile hitboxes... done
 * - moving map collisions... done?
 * - Get bone mapping to work properly
 * - Implement knockback simulation at some point
 * - Do some hud stuff (hardcore mode possible?)
 * - Figure out PS transformations
 * - General Point getter?
 * POTENTIALLY:
 * - use hurtcapsules "bone" jobj to use for first person melee (?)
 * - or just the capsule data itself?
 */
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

        processor.Register("map_schema", "Changes drawing colors: map_schema <mat, coll, int>", args => {
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

        processor.Register("draw_ecbs", "Enable/Disable ECB drawing", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var set = bool.Parse(args[0]);

            MeleeDrawing.DrawECBs = set;
        });
        processor.Register("draw_ledgegrabs", "Enable/Disable ledgegrab box drawing", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var set = bool.Parse(args[0]);

            MeleeDrawing.DrawLedgeGrabBoxes = set;
        });
        processor.Register("draw_hurtboxes", "Enable/Disable hurtbox drawing", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var set = bool.Parse(args[0]);

            MeleeDrawing.DrawHurtboxes = set;
        });
        processor.Register("draw_hitboxes", "Enable/Disable hitbox drawing", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var set = bool.Parse(args[0]);

            MeleeDrawing.DrawHitboxes = set;
        }); 
        processor.Register("nerd_stats", "Enable/Disable stats for nerds: nerd_stats <ft, it> <true, false>", args => {
            if (args.Length != 2) throw new Exception("Incorrect argument count.");

            var set = bool.Parse(args[1]);

            switch (args[0]) {
                case "ft": MeleeDrawing.DrawStatsForNerdsPlayer = set; break;
                case "it": MeleeDrawing.DrawStatsForNerdsItem = set; break;
            }
            
            MeleeDrawing.DrawStatsForNerdsPlayer = set;
        });
        processor.Register("draw_shields", "Enable/Disable shields drawing", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var set = bool.Parse(args[0]);

            MeleeDrawing.DrawShields = set;
        });
        processor.Register("plr_focus", "Sset to 0-3 to follow a player index. Set to -1 to turn off.", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var set = int.Parse(args[0]);

            EMTDisplay.PlayerFocus = set;
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
