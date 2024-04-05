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
    [SerializeField] private Transform furnaceDoor;
    public static int state = -1;
    private float stateProgress = 0;
    public static int[] deathByCounts = { 0, 0, 0, 0, 0, 0 };
    public static DeathBy lastDeathBy;
    private float targetProgress;
    [SerializeField] private WaveParticleManager waveParticleManager;
    [SerializeField] private Hourglass hourglass;
    [SerializeField] private RoundAbout roundabout;
    [SerializeField] private Lever lever;
    [SerializeField] private LightController lightControllerWave;
    [SerializeField] private LightController lightControllerHourglass;

    public static float globalFactor => DialogueController.instance.gameObject.activeInHierarchy ? 0 : 1;
    public static float furnaceFactor => globalFactor * (state == 0 ? 1 : .4f);
    public static float waveFactor => globalFactor * (state == 1 ? 1 : .4f);
    public static float hourglassFactor => globalFactor * (state == 2 ? 1 : .4f);

    private void Awake() => instance = this;

    public static void Reset()
    {
        // reset world
        RadiationManager.radiationLevel = 0;
        RadiationManager.heatLevel = 0;
        RadiationManager.FlushEffects();
        WaveParticleManager.desyncAmount = 0;
        instance.furnaceDoor.localEulerAngles = new(instance.furnaceDoor.localEulerAngles.x, 0, instance.furnaceDoor.localEulerAngles.z);
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
        if (state > -1 && stateProgress < targetProgress && (stateProgress += Time.deltaTime) >= targetProgress)
            DialogueController.Show(new() { state == 0 ? "Core sequence completed. Initializing wave synchronization channel . . ."
                                                       : "Particle alignment stabilized. Initializing time compression chamber . . ." },
                                    new() { IncrementState });
    }

    public static void IncrementState() => SetState(state + 1);
    public static void SetState(int newState)
    {
        // TODO: clear effects
        instance.stateProgress = 0;
        switch (state = newState)
        {
            case 0:
                instance.targetProgress = 30;
                break;
            case 1:
                instance.targetProgress = 60;
                instance.waveParticleManager.enabled = true;
                instance.lever.locked = false;
                instance.lightControllerWave.Activate();
                break;
            case 2:
                instance.targetProgress = 90;
                instance.hourglass.enabled = true;
                instance.roundabout.locked = false;
                instance.lightControllerHourglass.Activate();
                break;
        }
    }
}
