using UnityEngine;

public enum DeathBy
{
    Radiation,
    RadiationOverheat,
    WaveDesync,
}

public class GameState : MonoBehaviour
{
    public static GameState instance;
    [SerializeField] private Transform furnaceDoor;
    public static int state = -1;
    private float stateProgress = 0;
    public static int[] deathByCounts = { 0, 0, 0, 0, 0, 0 };
    public static DeathBy lastDeathBy;
    public static float globalFactor => DialogueController.instance.gameObject.activeInHierarchy ? 0 : 1;

    private void Awake() => instance = this;

    public static void Reset()
    {
        RadiationManager.radiationLevel = 0;
        RadiationManager.heatLevel = 0;
        RadiationManager.FlushEffects();
        WaveParticleManager.desyncAmount = 0;
        instance.furnaceDoor.localEulerAngles = new(instance.furnaceDoor.localEulerAngles.x, 0, instance.furnaceDoor.localEulerAngles.z);
        Player.ResetPosition();

        switch(state)
        {
            case 0:
                DialogueController.Show(new() { lastDeathBy == DeathBy.RadiationOverheat ? "Please ensure stable core temperature." : "Please minimize core radiation outflow." });
                break;
        }
    }

    private void Update()
    {
        stateProgress += Time.deltaTime;
    }

    public static void IncrementState() => ++state;
    public static void SetState(int newState) => state = newState;
}
