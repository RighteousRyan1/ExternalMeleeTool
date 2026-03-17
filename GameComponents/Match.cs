using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;

namespace ExternalMeleeTool.GameComponents;

/// <summary>A structure representing the match's settings.</summary>
public struct MatchData {
    /// <summary>The fighters in the match.</summary>
    public FighterData[] Fighters;
    /// <summary>The items loaded in the match. Will be null if <c>fetchItemData</c> was <c>false</c> from <see cref="Dolphinterop.GetMatchData(bool)"/>.</summary>
    public List<ItemData> Items;
    /// <summary>If there is an active teams match, <c>true</c>, else, <c>false</c>.</summary>
    public bool IsTeams;
    public bool IsPaused;

    /// <summary>A number representing what frame of the current second the game is on. (0-59)</summary>
    public s16 Frame;

    /// <summary>
    /// Loads the current match data.
    /// </summary>
    public static MatchData GetMatchData(bool fetchItemData = true) {
        var data = new MatchData {
            IsTeams = Dolphinterop.ReadU8(MeleePointers.START_MELEE_RULES + 0x8) == 1,
            // this frame parameter needs a lot of help...
            Frame = Dolphinterop.ReadS16(MeleePointers.MATCH_INFO + 0x2C /*0x46b6cc*/), // ReadS16(MeleeConstants.MATCH_INFO + 0x2C), //
            Fighters = new FighterData[4],
            // and not == 1? who tf made this crap?
            IsPaused = Dolphinterop.ReadU8(MeleePointers.PAUSE_BIT) == 2,
        };
        //Console.WriteLine("sfe: " + data.Frame);
        //Console.WriteLine("f_c: " + ReadS32(0x8046b6c4));
        //Console.WriteLine("t_s: " + ReadS32(0x8046b6c8));

        data.Fighters[0] = Dolphinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexOne);
        data.Fighters[1] = Dolphinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexTwo);
        data.Fighters[2] = Dolphinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexThree);
        data.Fighters[3] = Dolphinterop.GetMeleeFighterBlock(FighterMemorySlot.IndexFour);

        if (fetchItemData) {
            data.Items = [];
            var gobjList = MeleePointers.GetGObjList(PLink.Item);
            foreach (var gobj in gobjList) {
                // var ft = gobj.user_data;
                var item_data = Dolphinterop.Read<ItemData>(gobj.user_data);
                data.Items.Add(item_data);

                //var s = item_data.FieldsToString();
                //int x = Marshal.SizeOf<ItemData.HitboxDesc>();
                //int d = Marshal.SizeOf<ItemData>();
                // var s = item_data.FieldsToString();
                // var kind = (FtKind)ReadS32(ft + 0x4);
            }
        }

        return data;
    }

    /// <summary>
    /// Returns an enumerable to read-only references to each active fighter.
    /// </summary>
    public readonly IEnumerable<FighterData> ActiveFighters {
        get {
            for (int i = 0; i < Fighters.Length; i++) {
                if (Fighters[i].SlotKind == SlotKind.None) continue;

                yield return Fighters[i];
            }
        }
    }

    public readonly MatchData Clone() => (MatchData)MemberwiseClone();
}

public unsafe struct SlippiOnlineData {
    public byte ClientPort;
    public byte ClientControllerPort;
    public bool InOnlineMatch;
    public byte Frame;

    // MSRB = Match State Response Buffer
    // ... not in the same spot every time

    // public fixed char P1Name[31];

    public static bool IsSlippiOnline(SceneData gmd) {
        // for whatever reason, this indicates online melee
        return gmd.MinorScene == 8 && gmd.MajorScene == 2;
    }
    // fails if IsSlippiOnline is false
    public static byte GetClientPort(SceneData gmd) {
        if (!IsSlippiOnline(gmd)) return 255;

        var odb_ptr = Dolphinterop.ReadPtr(SlippiGlobals.ONLINE_DATA_BLOCK);

        var cli_port = Dolphinterop.ReadU8(odb_ptr);
        // var guh = $"{port_ptr:X} {Slippinterop.GALE01:X}";

        return cli_port;
    }

    public static SlippiOnlineData GetOnlineData(SceneData gmd) {
        var data = new SlippiOnlineData {
            ClientPort = GetClientPort(gmd),
            ClientControllerPort = Dolphinterop.ReadU8(Dolphinterop.ReadPtr(SlippiGlobals.ONLINE_DATA_BLOCK + 0x2)),
            InOnlineMatch = IsSlippiOnline(gmd),
            Frame = Dolphinterop.ReadU8(Dolphinterop.ReadPtr(SlippiGlobals.ONLINE_DATA_BLOCK + 0x3))
        };
        return data;
    }
}