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

    private void Awake()
    {
        instance = this;
        GameState.Save();
    }

    public static void Save()
    {
        // TODO: save/load cam orientation as well
        sd_radiationLevel = RadiationManager.radiationLevel;
        sd_heatLevel = RadiationManager.heatLevel;
        sd_furnaceDoorEulerY = instance.furnaceDoor.localEulerAngles.y;
        sd_playerPos = Player.CharacterMovement.GetPosition();
    }

    public static void Load()
    {
        RadiationManager.radiationLevel = sd_radiationLevel;
        RadiationManager.heatLevel = sd_heatLevel;
        RadiationManager.FlushEffects();
        instance.furnaceDoor.localEulerAngles = new(instance.furnaceDoor.localEulerAngles.x,
                                                    sd_furnaceDoorEulerY,
                                                    instance.furnaceDoor.localEulerAngles.z);
        Player.CharacterMovement.SetPosition(sd_playerPos, true);
    }
}
