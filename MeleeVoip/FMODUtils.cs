using System.Numerics;
using FMOD;

namespace MeleeVoip; 
public static class FMODUtils {
    public static VECTOR ToFMOD(this Vector3 v) => new() { x = v.X, y = v.Y, z = v.Z };
    public static void Check(this RESULT result) {
        if (result != RESULT.OK) {
            throw new Exception($"[FMOD Error]: {result} - {Error.String(result)}");
        }
    }
}
