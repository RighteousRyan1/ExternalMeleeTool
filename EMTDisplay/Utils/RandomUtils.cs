using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EMTDisplay.Utils; 
public static class RandomUtils {
    public static float NextFloat(this Random random, float min, float max)
=> (float)(random.NextDouble() * (max - min) + min);
    public static double NextDouble(this Random random, double min, double max)
        => random.NextDouble() * (max - min) + min;
    public static short Next(this Random random, short min, short max)
        => (short)random.Next(min, max);
    public static byte Next(this Random random, byte min, byte max)
        => (byte)random.Next(min, max);
}
