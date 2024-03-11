using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState instance;

    // save data
    private static Vector3 sd_playerPos;
    private static float sd_radiationLevel;
    private static float sd_heatLevel;

    private void Awake() => instance = this;

    public static void Save()
    {
        sd_playerPos = Player.transform.position;
        sd_radiationLevel = RadiationManager.radiationLevel;
        sd_heatLevel = RadiationManager.heatLevel;
    }

    public static void Load()
    {
        Player.transform.position = sd_playerPos;
        RadiationManager.radiationLevel = sd_radiationLevel;
        RadiationManager.heatLevel = sd_heatLevel;
        RadiationManager.FlushEffects();
    }
}
