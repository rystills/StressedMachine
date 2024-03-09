using UnityEngine;

public enum DeathBy
{
    Radiation,
    RadiationOverheat,
}

public class Player : MonoBehaviour
{
    public static new Transform transform;
    public static Player instance;

    public static void Die(DeathBy method)
    {
        RadiationManager.heatLevel = RadiationManager.radiationLevel = 0;
    }

    private void Awake()
    {
        instance = this;
        transform = GetComponent<Transform>();
    }
}
