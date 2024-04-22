using UnityEngine;

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
    public static float furnaceFactor   => globalFactor * (state == 0 ? 1.5f
                                                         : state == 1 ? .7f
                                                         : state == 2 ? .6f
                                                         : state == 3 ? .55f
                                                                      : .45f);
    public static float waveFactor      => globalFactor * (state == 1 ? 1.3f
                                                         : state == 2 ? .7f
                                                         : state == 3 ? .46f
                                                                      : .3f);
    public static float hourglassFactor => globalFactor * (state == 2 ? 1.3f
                                                         : state == 3 ? .6f
                                                                      : .4f);
    public static float ledFactor       => globalFactor * (state == 3 ? 1.2f
                                                                      : .9f);
    public static float pillarFactor    => globalFactor * .6f;

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
                deathMessage = "Please ensure that excess core heat is periodically flushed via the hardware shield.";
                break;
            case DeathBy.Radiation:
                deathMessage = "Please ensure that excess core radiation is filtered via the hardware shield.";
                break;
            case DeathBy.WaveDesync:
                deathMessage = "Please ensure that continuous wave synchronization is maintained via the alignment knob.";
                break;
            case DeathBy.TimeDecompression:
                deathMessage = "Please ensure that continuous time compression is maintained via the orientation wheel.";
                break;
            case DeathBy.SignalEncodingFailure:
                deathMessage = "Please ensure that homogeneous encoding is maintained via the light beacons.";
                break;
            case DeathBy.PillarRise:
                deathMessage = "Please ensure that bus alignment is maintained via kinetic force.";
                break;
        }
        if (deathMessage != "") DialogueController.Show(new() { deathMessage }, new() { instance.ResetWorld });
    }

    private void Update()
    {
        // update state progress
        if (state > -1 && stateProgress < targetProgress && (stateProgress += Time.deltaTime * globalFactor) >= targetProgress && !DeathAnimation.instance.gameObject.activeSelf)
        {
            DialogueController.Show(state == 0 ? new() { "The core has been recalibrated successfully!",
                                                         "Next, the wave synchronization channel will activate. Please use the alignment knob to achieve continuous wave synchronization . . ." }
                                  : state == 1 ? new() { "The wave channel has been recalibrated successfully!",
                                                         "Next, the time compression chamber will activate. Please use the orientation wheel to achieve continuous time compression . . ." }
                                  : state == 2 ? new() { "The time compression chamber has been recalibrated successfully!",
                                                         "Next, the signal encoding mechanism will activate. Please cycle the light beacons to achieve homogeneous encoding . . ." }
                                  : state == 3 ? new() { "The signal encoding mechanism has been recalibrated successfully!",
                                                         "Finally, the data transfer bus will activate. Please use kinetic force to prevent bus misalignment . . ." }
                                               : new() { "The data transfer bus has been recalibrated successfully!",
                                                         "Congratulations on completing system recalibration! The door will unlock momentarily . . .",
                                                         "I will see you again up ahead ☀" },
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
                instance.targetProgress = 42;
                instance.waveParticleManager.enabled = true;
                instance.lever.locked = false;
                instance.lightControllerWave.Activate();
                break;
            case 2:
                // roundabout
                instance.targetProgress = 60;
                instance.hourglass.enabled = true;
                instance.roundabout.locked = false;
                instance.lightControllerHourglass.Activate();
                break;
            case 3:
                // led machine
                instance.targetProgress = 100;
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
