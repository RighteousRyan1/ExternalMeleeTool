using System.Numerics;
using ExternalMeleeTool;

namespace MeleeThirdPerson;
public class MeleeCamManip {
    static int updates;
    static int fps = 0;

    static ThirdPersonCamera TPCamera;

    public static DateTime latestTime;
    static DateTime oldLatestTime;

    public static FighterBlock[] Fighters = new FighterBlock[4];
    public static MatchSettings Match;
    public static GlobalMeleeData MeleeData;
    static int clientPort;

    public static bool ForceToClientPort = true;
    static void Main() {
        TPCamera ??= new ThirdPersonCamera();
        Console.CursorVisible = false;

        WaitForMelee();

        Console.Clear();
        // Console.WriteLine($"Connected! GALE01 found at 0x{Slippinterop.GALE01:X}");

        while (Slippinterop.IsConnected) {
            latestTime = DateTime.Now;
            Match = Slippinterop.GetMatchSettings();
            MeleeData = Slippinterop.GetGlobalMeleeData();
            clientPort = GlobalMeleeData.ClientPort(MeleeData);

            var isOnline = GlobalMeleeData.IsSlippiOnline(MeleeData);

            if (ForceToClientPort)
                if (isOnline)
                    if (clientPort != -1)
                        TPCamera.FocusPort = clientPort;

            // our camera manips won't work unless develop cam is enabled
            Slippinterop.SetCameraType(CameraKind.Develop); // 0x08 = develop cam

            // player data
            PlayerPositionsAssign();
            HandleKeyPressEvents();

            // idk if i rly need a third person camera class
            // cam.Update(p1Pos, p2Pos, 0.016f);

            // these are mainly temp positions

            TPCamera.Update(/*(latestTime - oldLatestTime).Milliseconds*/);
            TPCamera.Camera.SetCam();

            var numFighters = Fighters.Count(x => x.Position != Vector3.Zero);

            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"FPS: {fps}                                    ");
            Console.WriteLine();
            Console.WriteLine("Keybinds:");
            Console.WriteLine($"Focus Next Port:   {NextFighter} (Current={TPCamera.FocusPort}, {Fighters[TPCamera.FocusPort].CharKind})                ");
            Console.WriteLine($"Change Focus Type: {ChangeFocusType} (Current={TPCamera.FocusType})         ");
            Console.WriteLine($"Force Online Port: {ToggleForcePort} (Current={ForceToClientPort})          ");

            var focusedFighter = Fighters[TPCamera.FocusPort];
            Console.WriteLine();
            Console.WriteLine($"Follow Data:         FocusType={TPCamera.FocusType}, FocusPort={TPCamera.FocusPort} ({focusedFighter.CharKind}, IsSub={focusedFighter.IsTransformed})             ");
            Console.WriteLine($"Slippi Data:         IsOnline={isOnline}, ClientPort={clientPort} Force={ForceToClientPort}     ");
            Console.WriteLine($"Match Data:          StageId={Match.StageId}, IsTeams={Match.IsTeams}        ");
            Console.WriteLine($"Global Data:         MajorScene={MeleeData.MajorScene}, MinorScene={MeleeData.MinorScene}        ");
            Console.WriteLine();
            Console.WriteLine($"Camera Position:     {TPCamera.Camera.Eye}                           ");
            Console.WriteLine($"Camera Focus:        {TPCamera.Camera.Focus}                         ");
            Console.WriteLine($"Camera FOV:          {TPCamera.Camera.Fov}                           ");
            Console.WriteLine();
            Console.WriteLine($"# Players Active: {numFighters}         ");

            for (int i = 0; i < Fighters.Length; i++) {
                var ft = Fighters[i];

                if (ft.SlotKind == SlotKind.None) continue;
                Console.WriteLine($"Player {i + 1}: {ft.FriendlyString()}                    ");
            }
            Console.WriteLine("                                                  ");
            Console.WriteLine("                                                  ");
            Console.WriteLine("                                                  ");
            
            updates++;

            if (latestTime.Second != oldLatestTime.Second) {
                fps = updates;
                updates = 0;
            }

            oldLatestTime = latestTime;

            // Thread.Sleep(4); // idk anything i do forces ~63 fps
        }

        Console.Clear();

        Main();
    }
    static void WaitForMelee() {
        // don't do anything with an invalid GALE01
        int rotIndex = 0;
        while (!Slippinterop.Connect()) {
            Console.SetCursorPosition(0, 0);
            ShowWait(rotIndex);

            rotIndex++;
            if (rotIndex > 3) rotIndex = 0;

            Thread.Sleep(250);
        }
    }
    static ConsoleKey NextFighter = ConsoleKey.OemMinus;
    static ConsoleKey ChangeFocusType = ConsoleKey.OemPlus;
    static ConsoleKey ToggleForcePort = ConsoleKey.Backspace;
    static void HandleKeyPressEvents() {

        // cycle to next active fighter
        if (KeyUtils.WasKeyPressed(NextFighter)) {
            int start = TPCamera.FocusPort;

            do {
                TPCamera.FocusPort++;
                if (TPCamera.FocusPort >= Fighters.Length)
                    TPCamera.FocusPort = 0;

                if (TPCamera.FocusPort == start)
                    break;

            } while (Fighters[TPCamera.FocusPort].SlotKind == SlotKind.None);
        }
        else if (KeyUtils.WasKeyPressed(ChangeFocusType)) {
            TPCamera.FocusType++;

            if (TPCamera.FocusType > ThirdPersonFocusType.ClosestEnemy)
                TPCamera.FocusType = 0;
        }
        else if (KeyUtils.WasKeyPressed(ToggleForcePort))
           ForceToClientPort = !ForceToClientPort;
    }
    static void PlayerPositionsAssign() {
        Fighters[0] = Slippinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexOne);
        Fighters[1] = Slippinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexTwo);
        Fighters[2] = Slippinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexThree);
        Fighters[3] = Slippinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexFour);
    }
    // blah...
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
        Console.WriteLine($"This program does not work with regular Dolphin. It must be Slippi Dolphin.");
    }
}
