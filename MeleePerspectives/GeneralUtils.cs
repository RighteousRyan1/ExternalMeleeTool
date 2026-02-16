using System.Numerics;

namespace MeleePerspectives; 
public static class GeneralUtils {
    public static string Truncate(this Version v) {
        if (v is null) return "0";

        if (v.Revision > 0) return v.ToString(4);

        if (v.Build > 0) return v.ToString(3);

        if (v.Minor > 0) return v.ToString(2);

        return v.Major.ToString();
    }

    public static Vector3 GetPerpendicular(Vector3 start, Vector3 end, Vector3 up) {
        var dir = Vector3.Normalize(end - start);

        /* // if dot product is near 1 or -1, they are parallel
        if (Math.Abs(Vector3.Dot(dir, up)) > 0.99f) {
            // If parallel to Y, pick X instead
            reference = Vector3.UnitX;
        }*/

        // cross Product gives a vector perpendicular to BOTH
        var perpendicular = Vector3.Cross(dir, up);

        return Vector3.Normalize(perpendicular);
    }
}
