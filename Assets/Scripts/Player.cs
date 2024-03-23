using EasyCharacterMovement;
using UnityEngine;

public enum DeathBy
{
    Radiation,
    RadiationOverheat,
    WaveDesync,
}

public class Player : FirstPersonCharacter
{
    // static references
    public static new Transform transform;
    public static Player instance;
    
    // local references
    [SerializeField] private DoorController doorController;
    [SerializeField] private RoundAbout roundAbout;
    
    // interaction
    [SerializeField] private float interactRange;
    
    // sounds
    [SerializeField] private AudioSource crouchSnd;
    [SerializeField] private AudioSource zoomOutSnd;

    // zoom
    private const float minFov = 20;
    private const float maxFov = 75;
    private const float zoomSpeed = 200;
    private float targetFov = maxFov;
    private float curFov = maxFov;

    public static CharacterMovement CharacterMovement => instance.characterMovement;

    public static void Die(DeathBy method) => DeathAnimation.Play();

    // TODO: continuous range check during interaction
    public static bool InRangeOf(Collider oCol) => (transform.position - oCol.ClosestPoint(transform.position)).magnitude <= instance.interactRange;
    public static bool InRangeOf(Vector3 pt) => (transform.position - pt).magnitude <= instance.interactRange;

    protected override void Awake()
    {
        base.Awake();
        instance = this;
        transform = GetComponent<Transform>();
        InputExt.RegisterKey("zoom", KeyCode.Z);
    }

    protected override void Update()
    {
        // disable movement while holding onto the roundabout
        if (!(handleInput = !roundAbout.interacting))
            SetMovementDirection(Vector3.zero);
        
        base.Update();
        
        // toggle zoom
        if (InputExt.keys["zoom"].pressed)
        {
            targetFov = targetFov == maxFov ? minFov : maxFov;
            SoundExt.PlayBiDir(zoomOutSnd, targetFov == minFov, true);
        }
        
        // update fov
        curFov = Mathf.MoveTowards(curFov, targetFov, zoomSpeed * Time.deltaTime);
        camera.fieldOfView = curFov;

        // temporary controls for testing
        if (Input.GetKeyDown(KeyCode.R)) doorController.ToggleLock();
    }

    override protected void Crouching()
    {
        // crouch
        if (_crouchButtonPressed && !IsCrouching() && CanCrouch())
        {
            characterMovement.SetHeight(crouchedHeight);
            _isCrouching = true;
            OnCrouched();
            SoundExt.PlayBiDir(crouchSnd, false, true);
        }
        // uncrouch
        else if (!_crouchButtonPressed && IsCrouching() && CanUnCrouch())
        {
            characterMovement.SetHeight(unCrouchedHeight);
            _isCrouching = false;
            OnUnCrouched();
            SoundExt.PlayBiDir(crouchSnd, true, true);
        }
    }
}
