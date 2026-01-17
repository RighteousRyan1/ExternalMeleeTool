namespace MeleeThirdPerson; 
public static class GeneralUtils {
    public static string Truncate(this Version v) {
        if (v is null) return "0";

        if (v.Revision > 0) return v.ToString(4);

        if (v.Build > 0) return v.ToString(3);

        if (v.Minor > 0) return v.ToString(2);

        return v.Major.ToString();
    }
}
