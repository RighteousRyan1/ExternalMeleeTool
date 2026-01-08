namespace ExternalMeleeTool; 

public static class MeleeUtils {
    public static bool GCInputPressed(FighterData fd, HSDPadButton button) => (fd.ButtonsOnInput & (uint)button) > 0;
}
