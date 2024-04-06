using UnityEngine;
using UnityEngine.UI;

public class RadiationManager : MonoBehaviour
{
    public static RadiationManager instance;

    [SerializeField] private Material radiationMat;
    [SerializeField] private Image radiationOverlayImg;
    [SerializeField] private Material heatMat;
    [SerializeField] private Image heatOverlayImg;
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
    [SerializeField] private AudioSource radiationSnd;
    [SerializeField] private AudioSource heatSnd;
    public static float radiationLevel;
    public static float heatLevel;

    private void Awake() => instance = this;

    public static void FlushEffects()
    {
        instance.radiationMat.SetFloat("strength", radiationLevel);
        instance.radiationOverlayImg.enabled = radiationLevel > 0;
        instance.heatMat.SetFloat("strength", heatLevel);
        instance.heatOverlayImg.enabled = heatLevel > 0;
        instance.metaballLight.intensity = Mathf.Lerp(instance.coolStrength, instance.hotStrength, heatLevel);
        instance.metaballLight.color = Color.Lerp(instance.coolColor, instance.hotColor, heatLevel);
    }

    private void Update()
    {
        // increase radiation while door is open
        radiationLevel = Mathf.Clamp01(radiationLevel + (door.isOpen * radiationIncr - door.isClosed * radiationDecr) * Time.deltaTime * GameState.globalFactor);
        if (radiationLevel == 1) Player.Die(DeathBy.Radiation);
        radiationSnd.volume = Mathf.Pow(radiationLevel, 6);

        // increase heat while door is closed
        heatLevel = Mathf.Clamp01(heatLevel + (door.isClosed * heatIncr * GameState.furnaceFactor - door.isOpen * heatDecr) * Time.deltaTime);
        if (heatLevel == 1) Player.Die(DeathBy.RadiationOverheat);
        heatSnd.volume = Mathf.Pow(heatLevel, 6);

        FlushEffects();
    }

    public static void Reset()
    {
        radiationLevel = 0;
        heatLevel = 0;
        FlushEffects();
    }
}
