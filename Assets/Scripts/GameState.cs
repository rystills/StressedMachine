using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState instance;
    [SerializeField] private Transform furnaceDoor;

    // save data
    private static Vector3 sd_playerPos;
    private static float sd_radiationLevel;
    private static float sd_heatLevel;
    private static float sd_furnaceDoorEulerY;

    private void Awake() => instance = this;

    public static void Save()
    {
        // TODO: save/load cam orientation as well
        sd_playerPos = Player.characterMovement.GetPosition();
        sd_radiationLevel = RadiationManager.radiationLevel;
        sd_heatLevel = RadiationManager.heatLevel;
        sd_furnaceDoorEulerY = instance.furnaceDoor.localEulerAngles.y;
    }

    public static void Load()
    {
        RadiationManager.radiationLevel = sd_radiationLevel;
        RadiationManager.heatLevel = sd_heatLevel;
        RadiationManager.FlushEffects();
        instance.furnaceDoor.localEulerAngles = new(instance.furnaceDoor.localEulerAngles.x,
                                                    sd_furnaceDoorEulerY,
                                                    instance.furnaceDoor.localEulerAngles.z);
        Player.characterMovement.SetPosition(sd_playerPos, true);
    }
}
