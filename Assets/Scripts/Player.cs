using EasyCharacterMovement;
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
    // TODO: extend FirstPersonCharacter and replace MyCharacter
    public static CharacterMovement characterMovement;

    public static void Die(DeathBy method) => DeathAnimation.Play();

    private void Awake()
    {
        instance = this;
        transform = GetComponent<Transform>();
        characterMovement = GetComponent<CharacterMovement>();
        GameState.Save();
    }
}
