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
}

public struct SlippiOnlineData {
    public byte ClientPort;
    public byte ClientControllerPort;
    public bool InOnlineMatch;
    public byte Frame;

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
}