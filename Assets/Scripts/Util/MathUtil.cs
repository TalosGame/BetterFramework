using UnityEngine;

public sealed class MathUtil 
{
    public static int RandomInt(int range)
    {
        return Random.Range(0, range);
    }

    /// <summary>
    ///  随机一个整数范围
    ///  min 包含
    ///  max 不包含
    /// </summary>
    /// <returns>The int range.</returns>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Max.</param>
    public static int RandomIntRange(int min, int max)
    {
        return Random.Range(min, max);
    }

    /// <summary>
    /// 随机一个浮点数范围
    /// min 包含
    /// max 不包含
    /// </summary>
    /// <returns>The float range.</returns>
    /// <param name="min">Minimum.</param>
    /// <param name="max">Max.</param>
    public static float RandomFloatRange(float min, float max)
    {
        return Random.Range(min, max);
    }

    /// <summary>
    /// 随机一个2D点
    /// </summary>
    /// <returns>The vec2.</returns>
    public static Vector3 RandomVec2()
    {
        float sw = Screen.width / 100f;
        float sh = Screen.height / 100f;

        float x = RandomFloatRange(-sw / 2f, sw / 2f);
        float y = RandomFloatRange(-sh / 2, sh / 2);

        return new Vector2(x, y);
    }

    // 在100%范围内随机一个数
    public static int RandomInt100Present(int []nums)
    {
        int randomRate = RandomInt(100);

        int curRate = 0;
        for (int i = 0; i < nums.Length; i++)
        {
            curRate += nums[i];
            if (randomRate <= curRate)
            {
                return i;
            }
        }

        return -1;
    }

    /// <summary>
    /// 根据半径计算cos
    /// </summary>
    /// <param name="degree">角度</param>
    /// <param name="radius">半径</param>
    /// <returns></returns>
    public static float CountCosValue(float degree, float radius)
    {
        float rad = degree * Mathf.Deg2Rad;
        return Mathf.Cos(rad) * radius;
    }

    /// <summary>
    /// 根据半径计算sin
    /// </summary>
    /// <param name="degree">角度</param>
    /// <param name="radius">半径</param>
    /// <returns></returns>
    public static float CountSinValue(float degree, float radius)
    {
        float rad = degree * Mathf.Deg2Rad;
        return Mathf.Sin(rad) * radius;
    }
}
