using EasyCharacterMovement;
using UnityEngine;

public enum DeathBy
{
    Radiation,
    RadiationOverheat,
}

public class Player : FirstPersonCharacter
{
    public static new Transform transform;
    public static Player instance;
    [SerializeField] private DoorController doorController;

    public static CharacterMovement CharacterMovement => instance.characterMovement;

    public static void Die(DeathBy method) => DeathAnimation.Play();

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        transform = GetComponent<Transform>();
    }

    protected override void Update()
    {
        base.Update();
        
        // temporary door control for testing
        if (Input.GetKeyDown(KeyCode.R)) doorController.ToggleLock();
    }
}
