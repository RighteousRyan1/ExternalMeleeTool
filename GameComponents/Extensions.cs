namespace ExternalMeleeTool.GameComponents;

public static class FighterExtensions {
    public static void SetPosition(this FighterData fighter, Vec3 newPos) {
        Dolphinterop.Write(fighter.PositionPtr, newPos);
    }
    public static void SetVelocity(this FighterData fighter, Vec3 newPos) {
        Dolphinterop.Write(fighter.PositionPtr + 0x80, newPos);
    }
    public static void SetKB(this FighterData fighter, Vec3 newPos) {
        Dolphinterop.Write(fighter.FighterPtr + 0x8C, newPos);
    }
}
