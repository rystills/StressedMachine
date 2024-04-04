using UnityEngine;

public class RadiationManager : MonoBehaviour
{
    public static RadiationManager instance;

    [SerializeField] private Material radiationMat;
    [SerializeField] private Material heatMat;
    [SerializeField] private HingeDoor door;
    [SerializeField] private float radiationIncr;
    [SerializeField] private float radiationDecr;
    [SerializeField] private float heatIncr;
    [SerializeField] private float heatDecr;
    [SerializeField] private Light metaballLight;
    [SerializeField] private Color coolColor;
    [SerializeField] private Color hotColor;
    [SerializeField] private float coolStrength;
    [SerializeField] private float hotStrength;
    public static float radiationLevel;
    public static float heatLevel;

    private void Awake() => instance = this;

    public static void FlushEffects()
    {
        instance.radiationMat.SetFloat("strength", radiationLevel);
        instance.heatMat.SetFloat("strength", heatLevel);
        instance.metaballLight.intensity = Mathf.Lerp(instance.coolStrength, instance.hotStrength, heatLevel);
        instance.metaballLight.color = Color.Lerp(instance.coolColor, instance.hotColor, heatLevel);
    }

    private void Update()
    {
        // increase radiation while door is open
        radiationLevel = Mathf.Clamp01(radiationLevel + (door.isOpen * radiationIncr - door.isClosed * radiationDecr) * Time.deltaTime * GameState.globalFactor);
        if (radiationLevel == 1) Player.Die(DeathBy.Radiation);

        // increase heat while door is closed
        heatLevel = Mathf.Clamp01(heatLevel + (door.isClosed * heatIncr - door.isOpen * heatDecr) * Time.deltaTime * GameState.globalFactor);
        if (heatLevel == 1) Player.Die(DeathBy.RadiationOverheat);

        FlushEffects();
    }
}
