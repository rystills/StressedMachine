using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState instance;
    [SerializeField] private Transform furnaceDoor;
    public static int state = -1;
    private float stateProgress = 0;

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
                DialogueController.Show(new() { "Please ensure stable core temperature." });
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
