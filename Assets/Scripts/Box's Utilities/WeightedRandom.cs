using System;
using UnityEngine;

public static class WeightedRandom
{
    // ---------- Int: [min, max] inclusive ----------

    public static int Int(int min, int max, float bias = 2f, float luck = 0f)
    {
        return IntFromRoll(UnityEngine.Random.value, min, max, bias, luck);
    }

    public static int Int(System.Random rng, int min, int max, float bias = 2f, float luck = 0f)
    {
        return IntFromRoll((float)rng.NextDouble(), min, max, bias, luck);
    }

    // ---------- Float: [min, max] ----------

    public static float Float(float min, float max, float bias = 2f, float luck = 0f)
    {
        return FloatFromRoll(UnityEngine.Random.value, min, max, bias, luck);
    }

    public static float Float(System.Random rng, float min, float max, float bias = 2f, float luck = 0f)
    {
        return FloatFromRoll((float)rng.NextDouble(), min, max, bias, luck);
    }

    // ---------- Internals ----------

    // Returns a 0-1 value skewed toward 0.
    // bias: 1 = uniform, >1 favors 0, <1 favors 1.
    // luck: 0 = no effect, positive pushes toward 1, negative toward 0.
    private static float Skew(float roll01, float bias, float luck)
    {
        // Luck <= -1 would divide by zero or flip the sign, so clamp it.
        float safeLuck = Mathf.Max(luck, -0.99f);
        float exponent = Mathf.Max(bias, 0.0001f) / (1f + safeLuck);
        return Mathf.Pow(Mathf.Clamp01(roll01), exponent);
    }

    private static int IntFromRoll(float roll01, int min, int max, float bias, float luck)
    {
        if (min > max) (min, max) = (max, min);

        // Scale across (range + 1) and floor so min and max get equal weight.
        int offset = Mathf.FloorToInt(Skew(roll01, bias, luck) * (max - min + 1));
        return Mathf.Clamp(min + offset, min, max);
    }

    private static float FloatFromRoll(float roll01, float min, float max, float bias, float luck)
    {
        if (min > max) (min, max) = (max, min);

        return Mathf.Lerp(min, max, Skew(roll01, bias, luck));
    }
}