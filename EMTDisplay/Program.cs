using EMTDisplay.CmdConsole;
using EMTDisplay.Utils;
using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using System;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace EMTDisplay;

/* next on the list:
 * - Implement knockback simulation at some point
 * - Figure out PS transformations
 * - General Point getter?
 */
public static partial class Program {

    [LibraryImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true)]
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
            var allCommands = processor.GetRegisteredCommands().ToList();

            // 1. Single Command Lookup
            if (args.Length > 0) {
                string target = args[0].Trim();
                var cmd = allCommands.FirstOrDefault(c => c.Name.Equals(target, StringComparison.OrdinalIgnoreCase));

                if (cmd.Name != null) {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.Write($"{cmd.Name} ");
                    Console.ResetColor();
                    Console.WriteLine($"- {cmd.Cmd.Desc}");

                    if (!string.IsNullOrEmpty(cmd.Cmd.Usage)) {
                        Console.ForegroundColor = ConsoleColor.DarkGray;
                        Console.WriteLine($"└─ {cmd.Cmd.Usage}");
                        Console.ResetColor();
                    }
                }
                else {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Command '{target}' not found.");
                    Console.ResetColor();
                }
                return;
            }

            // 2. Full Command List
            Console.WriteLine("Angled brackets <> denote required parameters, square brackets [] denote optional ones.");
            Console.WriteLine("\n--- Melee Console Commands ---");

            var sortedCommands = allCommands.OrderBy(c => c.Name);

            foreach (var (Name, Desc) in sortedCommands) {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"{Name} ");
                Console.ResetColor();
                Console.WriteLine($"- {Desc.Desc}");

                if (!string.IsNullOrEmpty(Desc.Usage)) {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.WriteLine($"└─ {Desc.Usage}");
                    Console.ResetColor();
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine();
            Console.WriteLine("Arguments that require spaces in-between are to be placed in-between quotes (\"like this\").");
            Console.WriteLine("Vec2 is formatted as such: (x, y), <x, y>");
            Console.WriteLine("Vec3 is formatted as such: (x, y, z), <x, y, z>");
            Console.ResetColor();
            Console.WriteLine("-------------------------------\n");
            Console.ResetColor();
        }, "help [command_name]");

        processor.Register("ft_setpos", "Sets a fighter's position", args => {
            if (args.Length < 2) throw new Exception("Not enough arguments.");
            int id = int.Parse(args[0]);
            if (Parsing.TryParse(args[1], out Vector3 pos)) {
                var fd = EMTDisplay.Match.Fighters[id];
                //fd.Position = pos;
                // fd.Position.Value = pos;
                // Dolphinterop.WriteVec3(fd.FighterPtr + 0xB0, Vector3.Zero);
                fd.SetPosition(pos);
                Dolphinterop.WriteVec3(fd.FighterPtr + 0xBC, pos);
                Dolphinterop.WriteVec3(fd.FighterPtr + 0xC8, Vector3.Zero);
                Dolphinterop.WriteS32(fd.FighterPtr + 0xE0, 0);
                Console.WriteLine($"Moved {fd.CharKind} to {pos}");
            }
        }, "ft_setpos <id> <vec3>");
        processor.Register("ft_focus", "Set to 0-3 to follow a player index. Set to -1 to turn off", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var set = int.Parse(args[0]);

            EMTDisplay.PlayerFocus = set;
        }, "ft_focus <portid>");

        processor.Register("map_schema", "Changes drawing colors of the stage's collision lines", args => {
            if (args.Length != 1) throw new Exception("Incorrect argument count.");
            var drawType = args[0];

            EMTDisplay.drawSchema = drawType switch {
                "mat" => 0,
                "coll" => 1,
                "int" => 2,
                _ => throw new Exception("Invalid draw type.")
            };
            Console.WriteLine("Drawing schema set to " + EMTDisplay.drawSchema + ".");
        }, "map_schema <mat, coll, int>");

        processor.Register("draw_ecbs", "Toggle ECB drawing", args => {
            MeleeDrawing.DrawECBs = !MeleeDrawing.DrawECBs;
        });
        processor.Register("draw_lgbs", "Toggle ledgegrab box drawing", args => {
            MeleeDrawing.DrawLedgeGrabBoxes = !MeleeDrawing.DrawLedgeGrabBoxes;
        });
        processor.Register("draw_hurt", "Toggle hurtbox drawing", args => {
            MeleeDrawing.DrawHurtboxes = !MeleeDrawing.DrawHurtboxes;
        });
        processor.Register("draw_hit", "Toggle hitbox drawing", args => {
            MeleeDrawing.DrawHitboxes = !MeleeDrawing.DrawHitboxes;
        }); 
        processor.Register("nerd_stats", "Toggle stats for nerds", args => {
            if (args.Length != 2) throw new Exception("Incorrect argument count.");

            var set = bool.Parse(args[1]);

            switch (args[0]) {
                case "ft": MeleeDrawing.DrawStatsForNerdsPlayer = set; break;
                case "it": MeleeDrawing.DrawStatsForNerdsItem = set; break;
            }
        }, "nerd_stats <ft, it> <true|false>");
        processor.Register("draw_shields", "Toggle shields drawing", args => {
            MeleeDrawing.DrawShields = !MeleeDrawing.DrawShields;
        });
        processor.Register("cam_smoothzoom", "Set the camera's target zoom over a specified duration of time", args => {
            if (args.Length < 2 || args.Length > 4) throw new Exception("Incorrect argument count.");
            var set = float.Parse(args[0]);
            var time = double.Parse(args[1]);

            if (args.Length == 3) {
                if (EnumUtils.TryGetEnumValue(args[2], out EasingFunction easing)) {
                    EMTDisplay.SetZoomTarget(set, TimeSpan.FromSeconds(time), easing);
                }
                else {
                    var fallback = EasingFunction.InOutQuad;

                    Console.WriteLine($"Invalid easing function. Falling back to {fallback}.");

                    EMTDisplay.SetZoomTarget(set, TimeSpan.FromSeconds(time), fallback);
                }
                return;
            }
            EMTDisplay.SetZoomTarget(set, TimeSpan.FromSeconds(time));
        }, "cam_zoom <zoom> <time_seconds> [easing]");
        processor.Register("cam_smoothpos", "Set the camera's target position (2D) over a specified duration of time", args => {
            if (args.Length < 2 || args.Length > 4) throw new Exception("Incorrect argument count.");
            
            if (Parsing.TryParse(args[0], out Vector2 set)) {
                var time = double.Parse(args[1]);

                if (args.Length == 3) {
                    if (EnumUtils.TryGetEnumValue(args[2], out EasingFunction easing)) {
                        EMTDisplay.SetTranslationTarget(set, TimeSpan.FromSeconds(time), easing);
                    }
                    else {
                        var fallback = EasingFunction.InOutQuad;

                        Console.WriteLine($"Invalid easing function. Falling back to {fallback}.");

                        EMTDisplay.SetTranslationTarget(set, TimeSpan.FromSeconds(time), fallback);
                    }
                    return;
                }

                EMTDisplay.SetTranslationTarget(set, TimeSpan.FromSeconds(time));
            }

            // EMTDisplay.SetTranslationTarget();
        }, "cam_setpos <vec2> <time_seconds> [easing]");

        processor.Register("easing_types", "See the kinds of easing types that can be used for commands with <easing> arguments", args => {
            Console.WriteLine("Here are the easings that can be used:");
            EnumUtils.PrintAll<EasingFunction>("\n");
        });
    }
}
