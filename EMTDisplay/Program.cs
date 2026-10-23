using EMTDisplay.CamExperiments;
using EMTDisplay.CmdConsole;
using EMTDisplay.Utils;
using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;
using ExternalMeleeTool.Melee;
using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.Mechanics;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text.Json;
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

    internal const string DRAWPARAMS_JSON_FNAME = "draw_params.json";
    internal static readonly JsonSerializerOptions opts = new() {
        IncludeFields = true,
        WriteIndented = true,
    };

    public static void Main() {
        AllocConsole();

        var processor = new CommandProcessor();

        RegisterCommands(processor);
        RegisterGoofyCommands(processor);

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

            MeleeDrawing.Params.DrawSchema = drawType switch {
                "mat" => 0,
                "coll" => 1,
                "int" => 2,
                _ => throw new Exception("Invalid draw type.")
            };
            Console.WriteLine("Drawing schema set to " + MeleeDrawing.Params.DrawSchema + ".");
        }, "map_schema <mat, coll, int>");

        processor.Register("draw_ecbs", "Toggle ECB drawing", args => {
            MeleeDrawing.Params.DrawECBs = !MeleeDrawing.Params.DrawECBs;
        });
        processor.Register("draw_lgbs", "Toggle ledgegrab box drawing", args => {
            MeleeDrawing.Params.DrawLedgeGrabBoxes = !MeleeDrawing.Params.DrawLedgeGrabBoxes;
        });
        processor.Register("draw_hurt", "Toggle hurtbox drawing", args => {
            MeleeDrawing.Params.DrawHurtboxes = !MeleeDrawing.Params.DrawHurtboxes;
        });
        processor.Register("draw_hit", "Toggle hitbox drawing", args => {
            MeleeDrawing.Params.DrawHitboxes = !MeleeDrawing.Params.DrawHitboxes;
        }); 
        processor.Register("nerd_stats", "Toggle stats for nerds", args => {
            if (args.Length != 2) throw new Exception("Incorrect argument count.");

            var set = bool.Parse(args[1]);

            switch (args[0]) {
                case "ft": MeleeDrawing.Params.DrawStatsForNerdsPlayer = set; break;
                case "it": MeleeDrawing.Params.DrawStatsForNerdsItem = set; break;
            }
        }, "nerd_stats <ft, it> <true|false>");
        processor.Register("draw_shields", "Toggle shields drawing", args => {
            MeleeDrawing.Params.DrawShields = !MeleeDrawing.Params.DrawShields;
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

        processor.Register("view", "Switch between the 2D and 3D views (Tab also toggles it in the window)", args => {
            if (args.Length == 0) {
                SceneView.Toggle();
            }
            else {
                switch (args[0].ToLowerInvariant()) {
                    case "2d": SceneView.SetMode(ViewMode.View2D); break;
                    case "3d": SceneView.SetMode(ViewMode.View3D); break;
                    case "toggle": SceneView.Toggle(); break;
                    default: throw new Exception("Invalid view. Use 2d, 3d or toggle.");
                }
            }
            Console.WriteLine("View set to " + (SceneView.Is3D ? "3D" : "2D") + ".");
        }, "view [2d|3d|toggle]");

        processor.Register("view_flipz", "Flip the Z axis of the 3D view if depth looks backwards", args => {
            SceneView.ZSign = -SceneView.ZSign;
            Console.WriteLine("Z sign is now " + SceneView.ZSign + ".");
        });

        processor.Register("cam_gameflipz", "Flip the Z mirroring used when writing the 3D camera to the game (F key)", args => {
            EMTDisplay.GameCamZSign = -EMTDisplay.GameCamZSign;
            Console.WriteLine("Game camera Z sign is now " + EMTDisplay.GameCamZSign + ".");
        });

        processor.Register("cam_orbit", "Set the 3D camera's orbit angles in degrees (no args resets them)", args => {
            if (args.Length == 0) {
                SceneView.ResetOrbit();
                return;
            }
            if (args.Length != 2) throw new Exception("Incorrect argument count.");

            SceneView.Yaw = Microsoft.Xna.Framework.MathHelper.ToRadians(float.Parse(args[0], CultureInfo.InvariantCulture));
            SceneView.Pitch = Microsoft.Xna.Framework.MathHelper.Clamp(Microsoft.Xna.Framework.MathHelper.ToRadians(float.Parse(args[1], CultureInfo.InvariantCulture)), -1.5f, 1.5f);
        }, "cam_orbit [yaw_deg pitch_deg]");

        processor.Register("draw_extrude", "3D view: toggle drawing stage collision/zones as depth walls, optionally set the depth", args => {
            if (args.Length > 0) {
                MeleeDrawing.Params.ExtrudeDepth = float.Parse(args[0], CultureInfo.InvariantCulture);
                MeleeDrawing.Params.Extrude3D = true;
            }
            else {
                MeleeDrawing.Params.Extrude3D = !MeleeDrawing.Params.Extrude3D;
            }
            Console.WriteLine($"Extrusion: {MeleeDrawing.Params.Extrude3D} (depth {MeleeDrawing.Params.ExtrudeDepth}).");
        }, "draw_extrude [depth]");

        processor.Register("draw_inactive", "Toggle drawing collision lines the engine has switched off, faintly. Off by default.", args => {
            MeleeDrawing.Params.ShowInactiveLines = !MeleeDrawing.Params.ShowInactiveLines;
            Console.WriteLine("Inactive collision lines shown: " + MeleeDrawing.Params.ShowInactiveLines);
        });

        processor.Register("stage_joints", "Print each collision joint's flags and how many of its lines are active", args => {
            var st = EMTDisplay.StDat;
            if (st.CollGroups == null) throw new Exception("No stage collision loaded.");
            foreach (var line in st.DescribeJoints()) Console.WriteLine(line);
        });

        processor.Register("prediction_debug", "Print the values the trajectory simulation starts from, for every fighter.", args => {
            foreach (var fd in EMTDisplay.Match.Fighters) {
                if (fd.SlotKind == SlotKind.None) continue;
                Console.WriteLine($"{fd.CharKind}: pos={fd.Position} self={fd.VelocitySelf} kb={fd.Knockback} hitlag={fd.HitlagFrames} hitstun={fd.HitstunFrames} grounded={fd.Grounded}");
                Console.WriteLine($"   grav={fd.Attr.grav} term={fd.Attr.terminal_vel} drift={fd.Attr.air_drift_stick_mul}/{fd.Attr.aerial_drift_base}/{fd.Attr.air_drift_max} fric={fd.Attr.aerial_friction} kbDecay={FighterData.PlCo.x204_knockbackFrameDecay}");
                var r = TrajectorySim.Simulate(fd, DIDirection.Current, 1f, 20);
                Console.WriteLine("   first pts: " + string.Join(" ", r.Points.Take(8).Select(pt => $"({pt.X:F1},{pt.Y:F1})")));
            }
        });

        processor.Register("draw_prediction", "Toggle knockback trajectories (white = current inputs, green = DI in, red = DI out). Optional frame count.", args => {
            if (args.Length > 0 && int.TryParse(args[0], out int n)) {
                MeleeDrawing.Params.PredictionFrames = Math.Clamp(n, 1, 600);
                MeleeDrawing.Params.DrawPrediction = true;
            }
            else MeleeDrawing.Params.DrawPrediction = !MeleeDrawing.Params.DrawPrediction;
            Console.WriteLine("Prediction: " + MeleeDrawing.Params.DrawPrediction + " (" + MeleeDrawing.Params.PredictionFrames + " frames)");
        });

        processor.Register("draw_centerlines", "Toggle the solid stage collision lines at Z=0 (3D walls/extrusion are separate)", args => {
            MeleeDrawing.Params.DrawCenterLines = !MeleeDrawing.Params.DrawCenterLines;
            Console.WriteLine("Center collision lines: " + MeleeDrawing.Params.DrawCenterLines);
        });

        processor.Register("fill_map", "3D view: toggle filled, translucent collision walls (spans the extrude depth). Optional opacity 0-1.", args => {
            if (args.Length > 0) {
                MeleeDrawing.Params.FillAlphaMap = Math.Clamp(float.Parse(args[0], CultureInfo.InvariantCulture), 0f, 1f);
                MeleeDrawing.Params.FillMapLines = true;
            }
            else {
                MeleeDrawing.Params.FillMapLines = !MeleeDrawing.Params.FillMapLines;
            }
            Console.WriteLine($"Filled collision: {MeleeDrawing.Params.FillMapLines} (opacity {MeleeDrawing.Params.FillAlphaMap:F2}).");
        }, "fill_map [opacity]");

        processor.Register("fill_boxes", "Toggle filled hit/hurtbox capsules. No argument toggles both.", args => {
            if (args.Length == 0) {
                bool set = !(MeleeDrawing.Params.FillHitboxes || MeleeDrawing.Params.FillHurtboxes);
                MeleeDrawing.Params.FillHitboxes = set;
                MeleeDrawing.Params.FillHurtboxes = set;
            }
            else {
                switch (args[0].ToLowerInvariant()) {
                    case "hit": MeleeDrawing.Params.FillHitboxes = !MeleeDrawing.Params.FillHitboxes; break;
                    case "hurt": MeleeDrawing.Params.FillHurtboxes = !MeleeDrawing.Params.FillHurtboxes; break;
                    default: throw new Exception("Invalid type. Use hit or hurt.");
                }
            }
            Console.WriteLine($"Filled hitboxes: {MeleeDrawing.Params.FillHitboxes}, filled hurtboxes: {MeleeDrawing.Params.FillHurtboxes}.");
        }, "fill_boxes [hit|hurt]");

        processor.Register("fill_outline", "Toggle drawing outlines on top of filled capsules", args => {
            MeleeDrawing.Params.OutlineFilledBoxes = !MeleeDrawing.Params.OutlineFilledBoxes;
            Console.WriteLine("Outline on filled capsules: " + MeleeDrawing.Params.OutlineFilledBoxes);
        });

        processor.Register("fill_alpha", "Set the opacity (0-1) of filled shapes", args => {
            if (args.Length != 2) throw new Exception("Incorrect argument count.");
            float a = Math.Clamp(float.Parse(args[1], CultureInfo.InvariantCulture), 0f, 1f);
            switch (args[0].ToLowerInvariant()) {
                case "map": MeleeDrawing.Params.FillAlphaMap = a; break;
                case "boxes": MeleeDrawing.Params.FillAlphaBoxes = a; break;
                default: throw new Exception("Invalid target. Use map or boxes.");
            }
        }, "fill_alpha <map|boxes> <0-1>");

        processor.Register("easing_types", "See the kinds of easing types that can be used for commands with <easing> arguments", args => {
            Console.WriteLine("Here are the easings that can be used:");
            EnumUtils.PrintAll<EasingFunction>("\n");
        });

        processor.Register("copy_melee_cam", "Copies the values from melee's camera into the viewer's camera", args => {
            EMTDisplay.CopyMeleeCamToViewer();
            Console.WriteLine("Copied Melee's camera into the 3D viewer camera.");
        });

        processor.Register("cam_match", "3D view: follow Melee's own camera live (on/off, no argument toggles). C also toggles it.", args => {
            if (args.Length == 0) SceneView.MatchGameCamera = !SceneView.MatchGameCamera;
            else {
                switch (args[0].ToLowerInvariant()) {
                    case "on": case "true": case "1": SceneView.MatchGameCamera = true; break;
                    case "off": case "false": case "0": SceneView.MatchGameCamera = false; break;
                    default: throw new Exception("Invalid argument. Use on or off.");
                }
            }
            Console.WriteLine("Match game camera: " + SceneView.MatchGameCamera + (SceneView.Is3D ? "." : " (takes effect in the 3D view)."));
        }, "cam_match [on|off]");
    }
    public static void RegisterGoofyCommands(CommandProcessor processor) {
        processor.Register("cam_cinematic", "Toggles a fun looking cinematic camera.", args => {
            CinematicCamera.IsEnabled = !CinematicCamera.IsEnabled;
            if (CinematicCamera.IsEnabled) Camera.SetCameraType(CameraType.DebugFree);
            else Camera.SetCameraType(CameraType.Standard);
        });
    }
}
