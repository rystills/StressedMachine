using System.Collections.Generic;
using UnityEngine;

public class LEDMachine : MonoBehaviour
{
    public static LEDMachine instance;

    [SerializeField] private List<LEDLight> lights;
    private float syncedAtTime;
    public static float desyncAmount;
    [SerializeField] private float syncDuration;
    private int[] rgbNum = new int[3];
    private int rgbMax;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        syncedAtTime = Time.time;
        lights.ForEach(l => l.gameObject.SetActive(true));
    }

    public static void Reset()
    {
        desyncAmount = 0;
        instance.syncedAtTime = Time.time;
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
        transform.Rotate(Time.deltaTime * 120, Time.deltaTime * 80, 0);
    }
}