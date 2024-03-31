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
    
    // interaction
    [SerializeField] private float interactRange;
    [SerializeField] private float interactRetainRange;
    public static bool InRangeOf(Collider oCol) => (inControl && !lentControl) && (transform.position - oCol.ClosestPoint(transform.position)).magnitude <= instance.interactRange;
    public static bool InRangeOf(Vector3 pt) => (inControl && !lentControl) && (transform.position - pt).magnitude <= instance.interactRange;
    public static bool InRetainRangeOf(Collider oCol) => inControl && (transform.position - oCol.ClosestPoint(transform.position)).magnitude <= instance.interactRetainRange;
    public static bool InRetainRangeOf(Vector3 pt) => inControl && (transform.position - pt).magnitude <= instance.interactRetainRange;
    
    // control toggles
    public static bool inControl => instance.activeCutscene == -1 && instance.handleInput;
    public static void EnableControl() => instance.handleInput = true;
    public static void DisableControl() => instance.handleInput = false;

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
    [SerializeField] private List<Light> floodlights;
    [SerializeField] private Material lightMat;
    [SerializeField] private MachineFaceController machineFaceController;
    private float initialFloodlightIntensity;
    private Color initialLightMatColor;
    private Quaternion initialCharacterRot;
    private Quaternion targetCharacterRot;
    private Quaternion initialEyeRot;
    private Quaternion targetEyeRot;

    public static CharacterMovement CharacterMovement => instance.characterMovement;

    public static void Die(DeathBy method) => DeathAnimation.Play();

    private void PlayCutscene(int ind)
    {
        cutsceneElapsedTime = 0;
        activeCutscene = ind;
    }

    override protected void Awake()
    {
        base.Awake();
        instance = this;
        transform = GetComponent<Transform>();
        InputExt.RegisterKey("zoom", KeyCode.Z);
        initialFloodlightIntensity = floodlights.FirstOrDefault()?.intensity ?? 1.25f;
        initialLightMatColor = lightMat.GetColor("_EmissionColor");

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
        
        // update fov
        curFov = Mathf.MoveTowards(curFov, targetFov, zoomSpeed * Time.deltaTime);
        camera.fieldOfView = curFov;

        // temporary controls for testing
        if (Input.GetKeyDown(KeyCode.R)) doorController.ToggleLock();

        if (activeCutscene != -1) TickCutscene(Time.deltaTime);
    }

    private void TickCutscene(float deltaTime)
    {
        switch (activeCutscene)
        {
            case 0:
                // slowly enable floodlights
                cutsceneElapsedTime += deltaTime;
                foreach (Light light in floodlights) light.intensity = Mathf.Lerp(0, initialFloodlightIntensity, cutsceneElapsedTime / 4);
                lightMat.SetColor("_EmissionColor", Color.Lerp(Color.clear, initialLightMatColor, cutsceneElapsedTime / 4));

                // look at machine face
                characterMovement.rotation = Quaternion.Lerp(initialCharacterRot, targetCharacterRot, (cutsceneElapsedTime - 4) * 4);
                eyePivot.localRotation = Quaternion.Lerp(initialEyeRot, targetEyeRot, (cutsceneElapsedTime - 4) * 4);
                if (cutsceneElapsedTime >= 4.25f)
                {
                    activeCutscene = -1;
                    DialogueController.Show(new() { ". .. ... .... ..... ...... ....... ........ .........", "Error detected during boot sequence. Manual core temperature regulation requested." });
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
}
