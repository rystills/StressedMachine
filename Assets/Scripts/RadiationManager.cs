using UnityEngine;

public class RadiationManager : MonoBehaviour
{
    [SerializeField] private Material mat;
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

    private void Update()
    {
        // increase radiation while door is open
        float radDelta = (door.openPercentage * radiationIncr - (int)(1 - door.openPercentage + .01f) * radiationDecr) * Time.deltaTime;
        radiationLevel = Mathf.Clamp01(radiationLevel + radDelta);
        if (radiationLevel == 1) Player.Die(DeathBy.Radiation);

        // increase heat while door is closed
        float heatDelta = (door.openPercentage * -heatDecr + (int)(1 - door.openPercentage + .01f) * heatIncr) * Time.deltaTime;
        heatLevel = Mathf.Clamp01(heatLevel + heatDelta);
        if (heatLevel == 1) Player.Die(DeathBy.RadiationOverheat);
        
        // apply effects
        mat.SetFloat("strength", radiationLevel);
        
        metaballLight.intensity = Mathf.Lerp(coolStrength, hotStrength, heatLevel);
        metaballLight.color = Color.Lerp(coolColor, hotColor, heatLevel);
    }
}
