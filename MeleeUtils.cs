using ExternalMeleeTool.GameComponents;
using System.Numerics;

namespace ExternalMeleeTool; 

public static class MeleeUtils {
    public static bool GCInputPressed(FighterData fd, HSDPadButton button) => fd.Input.Pressed.HasFlag(button); // (fd.Input.Pressed & (uint)button) > 0;
    public static bool GCInputHeld(FighterData fd, HSDPadButton button) => fd.Input.Held.HasFlag(button);// (fd.Input.Held & (uint)button) > 0;
}

static class Util {
    public static float ToRotation(this Vector2 vector) => MathF.Atan2(vector.Y, vector.X);
}
