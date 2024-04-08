using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LightController : MonoBehaviour
{
    private bool wantsOn;
    private float activationTime;
    [SerializeField] private List<Light> lights;
    public float targetIntensity;
    public float toggleFactor;
    private Color initialGlassMatColor;
    [SerializeField] private MeshRenderer glassMR;
    private Material glassMat;
    [SerializeField] private AudioSource toggleSnd;
    // TODO: jitter & sound

    public void Activate() => Activate(toggleFactor);
    public void Activate(float newFactor)
    {
        if (!wantsOn) toggleSnd.Play();
        toggleFactor = newFactor;
        wantsOn = enabled = true;
        activationTime = Time.time;
    }

    public void Deactivate() => Deactivate(toggleFactor);
    public void Deactivate(float newFactor)
    {
        if (wantsOn) toggleSnd.PlayBiDir(true);
        toggleFactor = newFactor;
        wantsOn = false;
        activationTime = Time.time;
    }

    private void Awake()
    {
        glassMat = glassMR.material;
        initialGlassMatColor = glassMat.GetColor("_EmissionColor");
        
        // start deactivated
        lights.ForEach(l => l.intensity = 0);
        glassMat.SetColor("_EmissionColor", Color.clear);
        enabled = wantsOn;
    }

    private void Update()
    {
        // lerp lights
        if (wantsOn)
        {
            lights.ForEach(l => l.intensity = Mathf.Lerp(0, targetIntensity, TimeExt.Since(activationTime) * toggleFactor));
            glassMat.SetColor("_EmissionColor", Color.Lerp(Color.clear, initialGlassMatColor, TimeExt.Since(activationTime) * toggleFactor));
        }
        else
        {
            lights.ForEach(l => l.intensity = Mathf.Lerp(targetIntensity, 0, TimeExt.Since(activationTime) * toggleFactor));
            glassMat.SetColor("_EmissionColor", Color.Lerp(initialGlassMatColor, Color.clear, TimeExt.Since(activationTime) * toggleFactor));
        }

        // toggle off
        enabled = wantsOn || lights.FirstOrDefault()?.intensity != 0;
    }
}
