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

    /// <summary>
    /// Loads global melee data.
    /// </summary>
    public static SceneData GetSceneData() {
        var data = new SceneData {
            MinorScene = Dolphinterop.ReadU8(MeleePointers.MINOR_SCENE),
            MajorScene = Dolphinterop.ReadU8(MeleePointers.MAJOR_SCENE)
        };
        // var hud = Read<IfAll_804A0FD8_t>(MeleePointers.MATCH_HUD);
        // var bone = Read<JObj>(hud.bone);
        //var hidden = ReadU8(MeleePointers.MATCH_HUD_HIDDEN);
        //WriteU8(MeleePointers.MATCH_HUD_HIDDEN, 1);
        // hud.stock_icon_pos1.X += 10;
        // var s = hud.FieldsToString();
        // var rand = new Random();
        //hud.xC.X = rand.Next(-500, 500);
        //hud.x60_1.X = rand.Next(-500, 500);
        //hud.x60_2.X = rand.Next(-500, 500);
        //hud.x60_3.X = rand.Next(-500, 500);

        //hud.x84_1.Y = rand.Next(-500, 500);
        //hud.x84_2.Y = rand.Next(-500, 500);

        //hud.stock_icon_pos1.Y = rand.Next(-50, 50);
        //hud.stock_icon_pos2.Y = rand.Next(-50, 50);
        //hud.stock_icon_pos3.Y = rand.Next(-50, 50);
        //hud.stock_icon_pos4.Y = rand.Next(-50, 50);
        //hud.stock_icon_pos5.Y = rand.Next(-50, 50);
        //hud.stock_icon_pos6.Y = rand.Next(-50, 50);
        // hud.xC.X = rand.Next(-50, 50); hud.xC.X = rand.Next(-50, 50);

        // Write(MeleePointers.MATCH_HUD, hud);
        return data;
    }
}
