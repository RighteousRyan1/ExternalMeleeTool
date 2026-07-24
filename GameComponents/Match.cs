using ExternalMeleeTool.Melee.Fighter;
using ExternalMeleeTool.Melee.HSD;
using ExternalMeleeTool.Utilities;

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

    public string LocalName;
    public string OppName;

    public SlippiPlayer[] PlayerData;

    public static bool IsSlippiOnline(SceneData gmd) {
        // for whatever reason, this indicates online melee
        return gmd.MinorScene == 8 && gmd.MajorScene == 2;
    }
    // fails if IsSlippiOnline is false
    public static byte GetClientPort(SceneData gmd) {
        if (!IsSlippiOnline(gmd)) return 255;

        var odb_ptr = Dolphinterop.ReadPtr(SlippiPointers.ONLINE_DATA_BLOCK);

        var cli_port = Dolphinterop.ReadU8(odb_ptr);
        // var guh = $"{port_ptr:X} {Slippinterop.GALE01:X}";

        return cli_port;
    }

    public static SlippiOnlineData GetOnlineData(SceneData gmd) {
        var data = new SlippiOnlineData {
            //InOnlineMatch probably can be properly fixed using the msrb, same with port?
            ClientPort = GetClientPort(gmd),
            ClientControllerPort = Dolphinterop.ReadU8(Dolphinterop.ReadPtr(SlippiPointers.ONLINE_DATA_BLOCK + 0x2)),
            InOnlineMatch = IsSlippiOnline(gmd),
            Frame = Dolphinterop.ReadU8(Dolphinterop.ReadPtr(SlippiPointers.ONLINE_DATA_BLOCK + 0x3))
        };

        data.PlayerData = new SlippiPlayer[4];

        var dtable = SlippiPointers.GetDataTable();
        var msrb = Dolphinterop.Read<MatchStateResponse>(Dolphinterop.ReadPtr(dtable.msrb));

        // i wonder how, i wonder why (it is reading into other buffers LOL)
        int offset = 3;

        data.LocalName = UnsafeUtils.CharptrToStr(msrb.local_name - offset);
        data.OppName = UnsafeUtils.CharptrToStr(msrb.opp_name - offset);

        data.PlayerData[0] = new() {
            Name = UnsafeUtils.CharptrToStr(msrb.p1_name - offset),
            ConnectCode = UnsafeUtils.CharptrToStr(msrb.p1_connect_code - offset),
            UID = UnsafeUtils.CharptrToStr(msrb.p1_uid - offset),
            Rank = msrb.p1_rank
        };
        data.PlayerData[1] = new() {
            Name = UnsafeUtils.CharptrToStr(msrb.p2_name - offset),
            ConnectCode = UnsafeUtils.CharptrToStr(msrb.p2_connect_code - offset),
            UID = UnsafeUtils.CharptrToStr(msrb.p2_uid - offset),
            Rank = msrb.p2_rank
        };
        data.PlayerData[2] = new() {
            Name = UnsafeUtils.CharptrToStr(msrb.p3_name - offset),
            ConnectCode = UnsafeUtils.CharptrToStr(msrb.p3_connect_code - offset),
            UID = UnsafeUtils.CharptrToStr(msrb.p3_uid - offset),
        };
        data.PlayerData[3] = new() {
            Name = UnsafeUtils.CharptrToStr(msrb.p4_name - offset),
            ConnectCode = UnsafeUtils.CharptrToStr(msrb.p4_connect_code - offset),
            UID = UnsafeUtils.CharptrToStr(msrb.p4_uid - offset),
        };
        return data;
    }
}

public struct SlippiPlayer {
    public string Name, ConnectCode, UID;
    public sbyte Rank; // Only in a ranked instance..?
}