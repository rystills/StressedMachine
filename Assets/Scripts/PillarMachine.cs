using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PillarMachine : MonoBehaviour
{
    public static LEDMachine instance;

    [SerializeField] private List<Pillar> pillars;
    [SerializeField] private float rotSpeed;
    private float[] pillarHeights;
    [SerializeField] private Material overlayMat;
    [SerializeField] private Image overlayImg;
    [SerializeField] private float maxHeight = 2;

    private void Awake() => pillarHeights = new float[pillars.Count];
    
    private void OnEnable() => pillars.ForEach(p => p.enabled = true);

    public void Reset() => pillars.ForEach(p => p.transform.localPosition = new(p.transform.localPosition.x, 0, p.transform.localPosition.z));
    
    public void LockPillars() => pillars.ForEach(p => p.locked = true);

    private void LateUpdate()
    {
        // update shader data
        for (int i = 0; i < pillars.Count; ++i) pillarHeights[i] = pillars[i].transform.localPosition.y;
        overlayMat.SetFloatArray("_PillarHeights", pillarHeights);
        
        float overlayStrength = pillarHeights.Max() / maxHeight;
        overlayMat.SetFloat("strength", overlayStrength);
        overlayImg.enabled = overlayStrength > 0;

        if (overlayStrength >= 1) Player.Die(DeathBy.PillarRise);
    }
}
