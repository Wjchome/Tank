using System;

public class DeterministicRandomStream
{
    private System.Random random;

    public DeterministicRandomStream(int seed)
    {
        random = new System.Random(seed);
    }

    // 获取下一个int
    public int Next()
    {
        return random.Next();
    }

    // 获取[min, max)的int
    public int Next(int min, int max)
    {
        return random.Next(min, max);
    }

    // 获取[0, 1)的float
    public float NextFloat()
    {
        return (float)random.NextDouble();
    }

    // 获取[min, max)的float
    public float NextFloat(float min, float max)
    {
        return min + (float)random.NextDouble() * (max - min);
    }

    // 随机选取一个枚举
    public T NextEnum<T>(int startIdx = 0) where T : Enum
    {
        var values = (T[])Enum.GetValues(typeof(T));
        return values[random.Next(startIdx, values.Length)];
    }
}