namespace ExternalMeleeTool.GameComponents;
/// <summary>A structure holding data relating to common melee data that isn't bound to gameplay.</summary>
public struct SceneData {
    /// <summary>The 'minor' scene data ID. Typically involves sub-menus.</summary>
    public byte MinorScene;
    /// <summary>The 'major' scene data ID. Typically involves different game states.</summary>
    public byte MajorScene;

    public readonly bool IsIngame => MajorScene == 2;
    public readonly bool IsUnclePunch => MinorScene == 43 && Dolphinterop.GameId == "GTME01";
    public readonly bool IsSlippiReplay => MajorScene == 1 && MinorScene == 14;
}
