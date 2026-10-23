using ExternalMeleeTool;
using System;

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

    public static Ptr32 RAND_SEED_OFFSET = 0x804D5F94;
    public static uint RandomSeed = 1;
    public static int HSD_Rand() {
        RandomSeed = RandomSeed * 214013 + 2531011;
        return (int)RandomSeed >> 0x10;
    }

    public static int HSD_Randi(int max) => max * HSD_Rand() / (1 << 16);
    public static float HSD_Randf() => (float)HSD_Rand() / (1 << 16);

    // FETCH!!!

    // sets RandomSeed to melee's current random seed offset
    public static void GetMeleeRandomSeed() {
        RandomSeed = Dolphinterop.ReadU32(Dolphinterop.ReadPtr(RAND_SEED_OFFSET));
    }
}
