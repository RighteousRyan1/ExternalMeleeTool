using ExternalMeleeTool;
using ExternalMeleeTool.GameComponents;

namespace MeleeVoip;

public class Module {
    public static MatchData Match;
    public static SceneData Scene;
    public static SlippiOnlineData OnlineData;
    public static StageData Stage;

    public static Version? version;
    public static HrtfVoip Voip = new();

    static void Main() {
        version = typeof(Module).Assembly.GetName().Version!;
        Console.CursorVisible = false;

        Console.WindowWidth = 125;
        Console.WindowHeight = 42;

        WaitForMelee();

        Console.Clear();

        WriteStatics();

        InitVoip();

        try {
            MainLoop();
        } catch (Exception ex) {
            Console.WriteLine("An error occurred:\n");
            Console.WriteLine(ex.ToString());
            Console.WriteLine("\nPlease restart this window.");
        }
        finally {
            Voip.Shutdown();
        }

        Console.Clear();

        Main();
    }

    static void InitVoip() {
        Console.WriteLine();
        WriteLineC("#-- VoIP Configuration --#", ConsoleColor.Cyan);

        Console.Write("Are you hosting/server? (y/n): ");
        string? isServerInput = Console.ReadLine();
        bool isServer = isServerInput?.Trim().ToLower() == "y";

        int localPort = 9050;
        int targetPort = 9050;
        string targetIp = "127.0.0.1";

        if (isServer) {
            Console.Write("Enter port to listen on (default 9050): ");
            string? pInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(pInput) && int.TryParse(pInput, out int p)) {
                localPort = p;
            }
        }
        else {
            Console.Write("Enter host IP (default 127.0.0.1): ");
            string? ipInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(ipInput)) targetIp = ipInput.Trim();

            Console.Write("Enter target host port (default 9050): ");
            string? tpInput = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(tpInput) && int.TryParse(tpInput, out int tp)) {
                targetPort = tp;
            }
        }

        try {
            Voip.Initialize(localPort, isServer, targetIp, targetPort);
        } catch (Exception ex) {
            WriteLineC($"[VoIP Init Error]: {ex.Message}", ConsoleColor.Red);
        }
    }

    public static void MainLoop() {
        while (Dolphinterop.IsConnected) {
            try {
                Match = MatchData.GetMatchData();
                Scene = SceneData.GetSceneData();
                OnlineData = SlippiOnlineData.GetOnlineData(Scene);

                HandleKeyPressEvents();

                if (Voip.isRunning) {
                    Voip.netManager.PollEvents();

                    if (Scene.IsIngame) {
                        Voip.Update3DCoordinates(Match, OnlineData);
                        Voip.fmodSystem.update().Check();
                    }
                }

                Thread.Sleep(15);
            } catch (Exception e) {
                Console.WriteLine("UH OH!!!: " + e.Message);
                Console.WriteLine(e.StackTrace);
            }
        }
    }

    static void WaitForMelee() {
        int rotIndex = 0;
        while (!Dolphinterop.Connect("GALE01", "GTME01", "GALJ01")) {
            Console.SetCursorPosition(0, 0);
            ShowWait(rotIndex);

            rotIndex++;
            if (rotIndex > 3) rotIndex = 0;

            Thread.Sleep(250);
        }
    }

    static int inputTimeout;
    static void HandleKeyPressEvents() {
        if (inputTimeout > 0) {
            inputTimeout--;
            return;
        }
    }

    static void WriteStatics() {
        Console.WriteLine();
        Console.WriteLine("Support the development of this and other projects!");
        Console.WriteLine("Patreon: https://patreon.com/c/RighteousRyan");
        Console.WriteLine("PayPal:  https://tinyurl.com/righteousryan");
        Console.WriteLine("YouTube: https://youtube.com/@RighteousRyan");
        Console.WriteLine("Twitch:  https://twitch.tv/righteousryan_");

        Console.WriteLine();
        WriteLineC("#-- Disclaimer(s) --#", ConsoleColor.DarkYellow);
        WriteLineC("- If you experience melee crashing, please play on Ishiiruka (\"Faster Melee\").", ConsoleColor.Yellow);
    }

    static void WriteLineC(object? value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.WriteLine(value);
        Console.ResetColor();
    }

    static void WriteC(object? value, ConsoleColor color) {
        Console.ForegroundColor = color;
        Console.Write(value);
        Console.ResetColor();
    }

    static void ShowWait(int rotIndex) {
        char rotChar = rotIndex switch {
            0 => '|',
            1 => '/',
            2 => '-',
            3 => '\\',
            _ => '#'
        };
        Console.WriteLine($"Scanning for Melee ... {rotChar}    ");
        Console.WriteLine();
        Console.WriteLine($"If you have Melee open and this message is still showing, restart your game.");
    }
}