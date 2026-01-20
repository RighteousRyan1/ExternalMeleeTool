using ExternalMeleeTool.GameComponents;

namespace ExternalMeleeTool; 

public static class MeleeUtils {
    public static bool GCInputPressed(FighterData fd, HSDPadButton button) => (fd.Input.ButtonsPressed & (uint)button) > 0;
}
