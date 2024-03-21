using UnityEngine;

public static class GameObjectExt
{
    public static void Reactivate(this GameObject go)
    {
        go.SetActive(false);
        go.SetActive(true);
    }
}
