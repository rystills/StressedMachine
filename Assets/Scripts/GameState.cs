using UnityEngine;

public enum DeathBy
{
    Radiation,
    RadiationOverheat,
    WaveDesync,
    TimeDecompression,
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
    public static bool rebalancing;
    [SerializeField] private DoorController doorController;
    [SerializeField] private MachineFaceController machineFaceController;

    // power down
    private float powerDownAtTime = -1;
    [SerializeField] private float powerDownDuration;
    [SerializeField] private MetaballManager metaballManager;

    public static float globalFactor => DialogueController.instance.gameObject.activeInHierarchy ? 0
                                      : instance.powerDownAtTime == -1 ? 1
                                      : powerDownFactor;
    public static float powerDownFactor => instance.powerDownAtTime == -1 ? 1 : 1 - TimeExt.Since(instance.powerDownAtTime) / instance.powerDownDuration;
    public static float furnaceFactor => globalFactor * (state == 0 ? 1 : .4f);
    public static float waveFactor => globalFactor * (state == 1 ? 1 : .4f);
    public static float hourglassFactor => globalFactor * (state == 2 ? 1 : .4f);

    private void Awake() => instance = this;

    public static void Reset()
    {
        // reset world
        RadiationManager.Reset();
        Hourglass.Reset();
        WaveParticleManager.Reset();
        instance.roundabout.Reset();
        instance.furnaceDoor.Reset();
        Player.ResetPosition();
        
        // reset state
        instance.stateProgress = 0;
        
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
        }
        if (deathMessage != "") DialogueController.Show(new() { deathMessage });
    }

    private void Update()
    {
        // update state progress
        if (state > -1 && stateProgress < targetProgress && (stateProgress += Time.deltaTime) >= targetProgress && !DeathAnimation.instance.gameObject.activeSelf)
        {
            DialogueController.Show(state == 0 ? new() { "Core apparatus engaged. Initializing wave synchronization channel . . ." }
                                  : state == 1 ? new() { "Wave alignment synchronized. Initializing time compression chamber . . ." }
                                  : state == 2 ? new() { "Time compression stabilized. Initializing signal encoding mechanism . . ." }
                                  : state == 3 ? new() { "TBD" }
                                  : new() { "Boot sequence complete! Unlocking exit door . . .", "I will see you again up ahead ☀" },
                                    new() { IncrementState, StopRebalancing });
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
                // light array
                instance.targetProgress = 120;
                instance.ledMachine.enabled = true;
                instance.lightControllerLED.Activate();
                break;
            case 4:
                // ???
                instance.targetProgress = 150;
                break;
            case 5:
                // end door
                instance.doorController.ToggleLock();
                instance.targetProgress = -1;
                instance.lightControllerMachine.Deactivate();
                instance.lightControllerFurnace.Deactivate();
                instance.lightControllerWave.Deactivate();
                instance.lightControllerHourglass.Deactivate();
                instance.powerDownAtTime = Time.time;
                instance.furnaceDoor.ToggleLock();
                instance.lever.locked = true;
                instance.roundabout.locked = true;
                break;
        }
    }
}
