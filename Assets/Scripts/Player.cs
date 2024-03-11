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

    public static void Die(DeathBy method) => DeathAnimation.Play();

    private void Awake()
    {
        instance = this;
        transform = GetComponent<Transform>();
        GameState.Save();
    }
}
