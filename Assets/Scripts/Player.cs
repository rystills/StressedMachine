using AYellowpaper.SerializedCollections;
using EasyCharacterMovement;
using System;
using System.Collections.Generic;
using System.Linq;
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
    [SerializeField] private float interactRetainRange;
    
    // sounds
    [SerializeField] private AudioSource crouchSnd;
    [SerializeField] private AudioSource zoomOutSnd;
    [SerializeField] private AudioSource jumpSnd;
    [SerializeField] private AudioSource landSnd;
    [SerializeField] private AudioSource landBigSnd;
[SerializedDictionary("Material Name", "Footstep Sound")]
    [SerializeField] private SerializedDictionary<string, AudioClip> footStepSnds;
    [SerializeField] private AudioSource footStepSrc;
    [NonSerialized] private float distFromLastStep;

    // zoom
    private const float minFov = 20;
    private const float maxFov = 75;
    private const float zoomSpeed = 200;
    private float targetFov = maxFov;
    private float curFov = maxFov;

    public static CharacterMovement CharacterMovement => instance.characterMovement;

    public static void Die(DeathBy method) => DeathAnimation.Play();

    public static bool InRangeOf(Collider oCol) => inControl && (transform.position - oCol.ClosestPoint(transform.position)).magnitude <= instance.interactRange;
    public static bool InRetainRangeOf(Collider oCol) => inControl && (transform.position - oCol.ClosestPoint(transform.position)).magnitude <= instance.interactRetainRange;
    public static bool InRangeOf(Vector3 pt) => inControl && (transform.position - pt).magnitude <= instance.interactRange;
    public static bool InRetainRangeOf(Vector3 pt) => inControl && (transform.position - pt).magnitude <= instance.interactRetainRange;

    override protected void Awake()
    {
        base.Awake();
        instance = this;
        transform = GetComponent<Transform>();
        InputExt.RegisterKey("zoom", KeyCode.Z);
    }

    override protected void Update()
    {
        base.Update();
        
        if (_movementMode == MovementMode.None)
            return;

        // disable movement while holding onto the roundabout
        if (!(handleInput = !roundAbout.interacting))
            SetMovementDirection(Vector3.zero);
        
        // toggle zoom
        if (InputExt.keys["zoom"].pressed)
        {
            targetFov = targetFov == maxFov ? minFov : maxFov;
            zoomOutSnd.PlayBiDir(targetFov == minFov, true);
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
            crouchSnd.PlayBiDir(false);
        }
        // uncrouch
        else if (!_crouchButtonPressed && IsCrouching() && CanUnCrouch())
        {
            characterMovement.SetHeight(unCrouchedHeight);
            _isCrouching = false;
            OnUnCrouched();
            crouchSnd.PlayBiDir(true);
        }
    }

    override public void Simulate(float deltaTime)
    {
        base.Simulate(deltaTime);

        if (_movementMode == MovementMode.None)
            return;

        // handle footstep sound
        if (IsGrounded() && Mathf.Sqrt(Mathf.Max(characterMovement.speed, IsCrouching() ? 2 : 10)) * 50 is float targetStepDist
                         && (distFromLastStep += characterMovement.speed) >= targetStepDist)
        {
            distFromLastStep -= targetStepDist;
            footStepSrc.clip = footStepSnds.GetValueOrDefault(characterMovement.groundCollider.material.name.Replace(" (Instance)", ""), footStepSnds.Values.First());
            footStepSrc.Play();
        }
    }

    override protected void OnJumped() => jumpSnd.Play();

    override protected void OnLanded()
    {
        if (characterMovement.landedVelocity.y > -20) landSnd.Play();
        else                                          landBigSnd.Play();
    }

    public static bool inControl => instance._movementMode != MovementMode.None;
    public static void EnableControl() => instance._movementMode = instance.IsGrounded() ? MovementMode.Walking : MovementMode.Falling;
    public static void DisableControl() => instance._movementMode = MovementMode.None;
}
