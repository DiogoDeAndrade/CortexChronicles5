using UnityEngine;

public static class Extensions 
{
    public static Vector3 Multiply(this Vector3 left, Vector2 right)
    {
        return new Vector3(left.x * right.x, left.y * right.y, left.z);
    }

    public static float Random(this Vector2 v)
    {
        return UnityEngine.Random.Range(v.x, v.y);
    }

    public static bool IsContained(this Emotion emotion, Emotion[] emotions)
    {
        foreach (var e in emotions)
        {
            if (e == emotion)
            {
                return true;
            }
        }

        return false;
    }
}
