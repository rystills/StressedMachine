using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class LEDMachine : MonoBehaviour
{
    public static LEDMachine instance;

    [SerializeField] private List<LEDLight> lights;
    private float syncedAtTime;
    [SerializeField] private float syncDuration;
    private float overlayStrength;
    private int[] rgbNum = new int[3];
    public static int rgbMax = -1;
    [SerializeField] private float desyncIncr;
    [SerializeField] private float desyncDecr;
    [SerializeField] private Material overlayMat;
    [SerializeField] private Image overlayImg;
    [SerializeField] private AudioSource overlaySnd;
    [SerializeField] private AudioSource activeSnd;
    private float[] partColInds;

    private void Awake()
    {
        instance = this;
        partColInds = new float[lights.Count];
    }

    private void OnEnable()
    {
        syncedAtTime = Time.time;
        lights.ForEach(l => l.gameObject.SetActive(true));
        activeSnd.Play();
    }

    public void Reset()
    {
        if (rgbMax != -1)
        {
            // reset all LEDs to the most common color
            foreach (LEDLight l in lights) if (l.colInd != rgbMax) l.SetColInd(rgbMax, false);
            syncedAtTime = Time.time;
            overlayStrength = 0;
        }
    }
    
    public void LockLEDs() => lights.ForEach(l => l.locked = true);

    public static void SendColorsToShader()
    {
        for (int i = 0; i < instance.lights.Count; ++i) instance.partColInds[i] = instance.lights[i].colInd;
        instance.overlayMat.SetFloatArray("_PartColInds", instance.partColInds);
    }

    private void LateUpdate()
    {
        // determine the most common color
        rgbNum[0] = rgbNum[1] = rgbNum[2] = 0;
        for (int i = 0; i < lights.Count; ++i) ++rgbNum[lights[i].colInd];
        rgbMax = rgbNum[0] > rgbNum[1] ? 0 : 1;
        rgbMax = rgbNum[rgbMax] > rgbNum[2] ? rgbMax : 2;

        // desync after syncDuration elapses
        if (syncedAtTime >= 0 && TimeExt.Since(syncedAtTime) > syncDuration / GameState.ledFactor)
        {
            syncedAtTime += syncDuration;

            // select a random light of the most common color
            int swapInd;
            do { swapInd = Random.Range(0, lights.Count); } while (lights[swapInd].colInd != rgbMax);

            // swap the color
            lights[swapInd].SetColInd((lights[swapInd].colInd + Random.Range(1, 3)) % 3);
        }

        // rotate over time
        transform.Rotate(Time.deltaTime * 120 * GameState.powerDownFactor, Time.deltaTime * 80 * GameState.powerDownFactor, 0);

        // update overlay
        overlayStrength = Mathf.Clamp01(overlayStrength + (desyncIncr * (lights.Count - rgbNum[rgbMax]) - desyncDecr) * Time.deltaTime * GameState.globalFactor);
        overlayMat.SetFloat("strength", overlayStrength);
        overlayImg.enabled = overlayStrength > 0;
        
        // adjust sounds
        overlaySnd.volume = Mathf.Pow(overlayStrength, 6);
        activeSnd.pitch = GameState.powerDownFactor * -.25f;
        
        if (overlayStrength == 1) Player.Die(DeathBy.SignalEncodingFailure);
        
    }
}