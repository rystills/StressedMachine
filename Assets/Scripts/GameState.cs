﻿using UnityEngine;

public enum DeathBy
{
    Radiation,
    RadiationOverheat,
    WaveDesync,
    TimeDecompression,
    SignalEncodingFailure,
    PillarRise,
}

public class GameState : MonoBehaviour
{
    public static GameState instance;
    [SerializeField] private HingeDoor furnaceDoor;
    public static int state = -1;
    private float stateProgress = 0;
    public static int[] deathByCounts = { 0, 0, 0, 0, 0, 0 };
    public static DeathBy lastDeathBy;
    private float targetProgress;
    [SerializeField] private WaveParticleManager waveParticleManager;
    [SerializeField] private Hourglass hourglass;
    [SerializeField] private RoundAbout roundabout;
    [SerializeField] private Lever lever;
    [SerializeField] private LEDMachine ledMachine;
    [SerializeField] private LightController lightControllerMachine;
    [SerializeField] private LightController lightControllerFurnace;
    [SerializeField] private LightController lightControllerWave;
    [SerializeField] private LightController lightControllerHourglass;
    [SerializeField] private LightController lightControllerLED;
    [SerializeField] private LightController lightControllerPillar;
    public static bool rebalancing;
    [SerializeField] private DoorController doorController;
    [SerializeField] private MachineFaceController machineFaceController;
    [SerializeField] private PillarMachine pillarMachine;

    // power down
    private float powerDownAtTime = -1;
    [SerializeField] private float powerDownDuration;
    [SerializeField] private MetaballManager metaballManager;

    public static float stateCompletion => instance.targetProgress == 0 || globalFactor == 0 || instance.powerDownAtTime != -1 ? 1 : instance.stateProgress / instance.targetProgress;

    public static float globalFactor => DialogueController.instance.gameObject.activeInHierarchy ? 0
                                      : instance.powerDownAtTime == -1 ? 1
                                      : powerDownFactor;
    public static float powerDownFactor => instance.powerDownAtTime == -1 ? 1 : 1 - TimeExt.Since(instance.powerDownAtTime) / instance.powerDownDuration;
    public static float furnaceFactor => globalFactor * (state == 0 ? 1.1f : .4f);
    public static float waveFactor => globalFactor * (state == 1 ? 1.3f : .32f);
    public static float hourglassFactor => globalFactor * (state == 2 ? 1.1f : .4f);
    public static float ledFactor => globalFactor * (state == 3 ? 1f : .4f);
    public static float pillarFactor => globalFactor * (state == 4 ? 1f : .4f);

    private void Awake() => instance = this;

    private void ResetWorld()
    {
        RadiationManager.Reset();
        Hourglass.Reset();
        WaveParticleManager.Reset();
        roundabout.Reset();
        furnaceDoor.Reset();
        Player.ResetPosition();
        ledMachine.Reset();
        pillarMachine.Reset();
        stateProgress = 0;
    }

    public static void Reset()
    {
        instance.ResetWorld();
        
        // show death message
        string deathMessage = "";
        switch (lastDeathBy)
        {
            case DeathBy.RadiationOverheat:
                deathMessage = "Please ensure stable core temperature.";
                break;
            case DeathBy.Radiation:
                deathMessage = "Please ensure minimal core radiation outflow.";
                break;
            case DeathBy.WaveDesync:
                deathMessage = "Please ensure continuous wave synchronization.";
                break;
            case DeathBy.TimeDecompression:
                deathMessage = "Please ensure continuous time compression.";
                break;
            case DeathBy.SignalEncodingFailure:
                deathMessage = "Please ensure uninterrupted signal encoding.";
                break;
            case DeathBy.PillarRise:
                deathMessage = "TBD";
                break;
        }
        if (deathMessage != "") DialogueController.Show(new() { deathMessage }, new() { instance.ResetWorld });
    }

    private void Update()
    {
        // update state progress
        if (state > -1 && stateProgress < targetProgress && (stateProgress += Time.deltaTime * globalFactor) >= targetProgress && !DeathAnimation.instance.gameObject.activeSelf)
        {
            DialogueController.Show(state == 0 ? new() { "Core apparatus engaged. Initializing wave synchronization channel . . ." }
                                  : state == 1 ? new() { "Wave alignment synchronized. Initializing time compression chamber . . ." }
                                  : state == 2 ? new() { "Time compression stabilized. Initializing signal encoding mechanism . . ." }
                                  : state == 3 ? new() { "TBD" }
                                  : new() { "Boot sequence complete! Unlocking exit door . . .", "I will see you again up ahead ☀" },
                                    new() { IncrementState, StopRebalancing, ResetWorld });
            rebalancing = true;
            furnaceDoor.enabled = true;
            Player.ReturnControl();
        }

        // power down
        if (powerDownAtTime != -1 && powerDownFactor <= 0)
        {
            enabled = false;
            machineFaceController.enabled = false;
            waveParticleManager.enabled = false;
            hourglass.enabled = false;
            metaballManager.enabled = false;
            instance.ledMachine.enabled = false;
            instance.pillarMachine.enabled = false;
        }
    }

    private void StopRebalancing() => rebalancing = false;

    public static void IncrementState() => SetState(state + 1);
    public static void SetState(int newState)
    {
        instance.stateProgress = 0;
        switch (state = newState)
        {
            case 0:
                // furnace
                instance.targetProgress = 30;
                break;
            case 1:
                // hourglass
                instance.targetProgress = 60;
                instance.waveParticleManager.enabled = true;
                instance.lever.locked = false;
                instance.lightControllerWave.Activate();
                break;
            case 2:
                // roundabout
                instance.targetProgress = 90;
                instance.hourglass.enabled = true;
                instance.roundabout.locked = false;
                instance.lightControllerHourglass.Activate();
                break;
            case 3:
                instance.targetProgress = 120;
                // led machine
                instance.ledMachine.enabled = true;
                instance.lightControllerLED.Activate();
                break;
            case 4:
                // pillar machine
                instance.targetProgress = 150;
                instance.pillarMachine.enabled = true;
                instance.lightControllerPillar.Activate();
                break;
            case 5:
                // end door
                instance.doorController.ToggleLock();
                instance.targetProgress = -1;
                instance.lightControllerMachine.Deactivate();
                instance.lightControllerFurnace.Deactivate();
                instance.lightControllerWave.Deactivate();
                instance.lightControllerHourglass.Deactivate();
                instance.lightControllerLED.Deactivate();
                instance.lightControllerPillar.Deactivate();
                instance.powerDownAtTime = Time.time;
                instance.ledMachine.LockLEDs();
                instance.pillarMachine.LockPillars();
                instance.furnaceDoor.ToggleLock();
                instance.lever.locked = true;
                instance.roundabout.locked = true;
                break;
        }
    }
}
