using AYellowpaper.SerializedCollections;
using EasyCharacterMovement;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Player : FirstPersonCharacter
{
    // static references
    public static new Transform transform;
    public static Player instance;
    
    // local references
    [SerializeField] private GameObject crosshair;
    [SerializeField] private HingeDoor hingeDoor;

    // interaction
    [SerializeField] private float interactRange;
    [SerializeField] private float interactRetainRange;
    public static bool InRangeOf(Collider oCol) => (inControl && !lentControl) && (transform.position - oCol.ClosestPoint(transform.position)).magnitude <= instance.interactRange;
    public static bool InRangeOf(Vector3 pt) => (inControl && !lentControl) && (transform.position - pt).magnitude <= instance.interactRange;
    public static bool InRetainRangeOf(Collider oCol) => inControl && (transform.position - oCol.ClosestPoint(transform.position)).magnitude <= instance.interactRetainRange;
    public static bool InRetainRangeOf(Vector3 pt) => inControl && (transform.position - pt).magnitude <= instance.interactRetainRange;
    
    // control toggles
    public static bool inControl => instance.activeCutscene == -1 && instance.handleInput;
    public static void EnableControl() => instance.crosshair.SetActive(instance.handleInput = true);
    public static void DisableControl() => instance.crosshair.SetActive(instance.handleInput = false);

    public static bool lentControl;
    public static void LendControl() => lentControl = true;
    public static void ReturnControl() => lentControl = false;
    
    // sounds
    [SerializeField] private AudioSource crouchSnd;
    [SerializeField] private AudioSource zoomOutSnd;
    [SerializeField] private AudioSource jumpSnd;
    [SerializeField] private AudioSource landSnd;
    [SerializeField] private AudioSource landBigSnd;

    // footsteps
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

    // cutscenes
    private int activeCutscene = -1;
    private float cutsceneElapsedTime;
    [SerializeField] private LightController lightControllerMachine;
    [SerializeField] private LightController lightControllerFurnace;
    [SerializeField] private MachineFaceController machineFaceController;
    private Quaternion initialCharacterRot;
    private Quaternion targetCharacterRot;
    private Quaternion initialEyeRot;
    private Quaternion targetEyeRot;
    private Vector3 startPos;
    [SerializeField] private GameObject endBackground;
    [SerializeField] private GameObject endTitle;
    [SerializeField] private GameObject endSignature;

    public static CharacterMovement CharacterMovement => instance.characterMovement;

    public static void Die(DeathBy method) => DeathAnimation.Play(method);

    private void PlayCutscene(int ind)
    {
        crosshair.SetActive(false);
        cutsceneElapsedTime = 0;
        activeCutscene = ind;
        switch (ind)
        {
            case 0:
                lightControllerMachine.Activate();
                break;
        }
    }

    override protected void Awake()
    {
        base.Awake();
        instance = this;
        startPos = characterMovement.GetPosition();
        transform = GetComponent<Transform>();
        InputExt.RegisterKey("zoom", KeyCode.Z);

        // cutscenes
        initialCharacterRot = characterMovement.rotation;
        targetCharacterRot = Quaternion.Euler(0, 30, 0) * initialCharacterRot;
        initialEyeRot = eyePivot.localRotation;
        targetEyeRot = Quaternion.Euler(-75, 0, 0) * initialEyeRot;

        PlayCutscene(0);
    }

    override protected void Update()
    {
        base.Update();

        if (!inControl || lentControl) SetMovementDirection(Vector3.zero);
        
        // rebalance
        if (GameState.rebalancing)
        {
            instance.characterMovement.SetPosition(Vector3.MoveTowards(instance.characterMovement.GetPosition(), instance.startPos, 100 * Time.deltaTime));
            instance.characterMovement.SetRotation(Quaternion.RotateTowards(instance.characterMovement.GetRotation(), instance.targetCharacterRot, 1000 * Time.deltaTime));
            instance.eyePivot.localRotation = Quaternion.RotateTowards(instance.eyePivot.localRotation, instance.targetEyeRot, 400 * Time.deltaTime);
            instance.camera.fieldOfView = instance.curFov = instance.targetFov = Mathf.MoveTowards(instance.curFov, maxFov, 400 * Time.deltaTime);
        }
        else
        {
            // update fov
            curFov = Mathf.MoveTowards(curFov, targetFov, zoomSpeed * Time.deltaTime);
            camera.fieldOfView = curFov;

            if (activeCutscene != -1) TickCutscene(Time.deltaTime);
        }

        // end cutscene
        if (transform.position.y < -120) endSignature.SetActive(true);
        else if (transform.position.y < -50) endTitle.SetActive(true);
        else if (transform.position.y < -10)
        {
            if (inControl)
            {
                crosshair.SetActive(false);
                endBackground.SetActive(true);
                DisableControl();
            }
            eyePivot.localRotation = Quaternion.RotateTowards(eyePivot.localRotation, initialEyeRot, 100 * Time.deltaTime);
        }
    }

    private void TickCutscene(float deltaTime)
    {
        switch (activeCutscene)
        {
            case 0:
                // slowly enable floodlights
                cutsceneElapsedTime += deltaTime;

                // look at machine face
                characterMovement.rotation = Quaternion.Lerp(initialCharacterRot, targetCharacterRot, (cutsceneElapsedTime - 2) * 4);
                eyePivot.localRotation = Quaternion.Lerp(initialEyeRot, targetEyeRot, (cutsceneElapsedTime - 2) * 4);
                if (cutsceneElapsedTime >= 2.25f)
                {
                    activeCutscene = -1;
                    DialogueController.Show(new() { "Initializing . . . . . . .", "Multiple failures detected during boot sequence. Manual system regulation authorized.", "Initializing core apparatus . . ." },
                                            new() { lightControllerFurnace.Activate, hingeDoor.ToggleLock, GameState.IncrementState } );
                }
                break;
        }
    }

    override protected void HandleInput()
    {
        // camera control
        if (!inControl) return;
        HandleCameraInput();

        // toggle zoom
        if (InputExt.keys["zoom"].pressed)
        {
            targetFov = targetFov == maxFov ? minFov : maxFov;
            zoomOutSnd.PlayBiDir(targetFov == minFov, true);
        }

        // character control
        if (lentControl) return;
        HandleCharacterInput();
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

        // handle footstep sound
        if (IsGrounded() && Mathf.Sqrt(Mathf.Max(characterMovement.speed, IsCrouching() ? 2 : 10)) * 50 is float targetStepDist
                         && (distFromLastStep += characterMovement.speed) >= targetStepDist)
        {
            distFromLastStep -= targetStepDist;
            footStepSrc.PlayClip(footStepSnds.GetValueOrDefault(characterMovement.groundCollider.material.name.Replace(" (Instance)", ""), footStepSnds.Values.First()));
        }
    }

    override protected void OnJumped() => jumpSnd.Play();

    override protected void OnLanded()
    {
        if (Time.timeSinceLevelLoad > 0)
        {
            if (characterMovement.landedVelocity.y > -20) landSnd.Play();
            else                                          landBigSnd.Play();
        }
    }

    public static void ResetPosition()
    {
        // reset inputs
        instance._crouchButtonPressed = false;
        instance._sprintButtonPressed = false;
        instance._jumpButtonPressed = false;

        // reset transform
        instance.characterMovement.SetPosition(instance.startPos);
        instance.characterMovement.rotation = instance.targetCharacterRot;
        instance.eyePivot.localRotation = instance.targetEyeRot;

        // reset zoom
        instance.camera.fieldOfView = instance.curFov = instance.targetFov = maxFov;
    }
}
