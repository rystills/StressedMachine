using UnityEngine;

public static class Vector3Ext
{
    public static Vector3 Clamp(this Vector3 v, float min = -1, float max = 1)
    => new(Mathf.Clamp(v.x, min, max),
           Mathf.Clamp(v.y, min, max),
           Mathf.Clamp(v.z, min, max));

    public static Vector3 Round(this Vector3 v)
        => new(Mathf.Round(v.x),
               Mathf.Round(v.y),
               Mathf.Round(v.z));
}
