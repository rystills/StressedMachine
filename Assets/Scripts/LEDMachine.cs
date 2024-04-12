using System.Collections.Generic;
using UnityEngine;

public class LEDMachine : MonoBehaviour
{
    public static LEDMachine instance;

    [SerializeField] private List<Light> lights;
    private float syncedAtTime;
    public static float desyncAmount;
    [SerializeField] private float syncDuration;

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
        // desync after syncDuration elapses
        if (syncedAtTime >= 0 && TimeExt.Since(syncedAtTime) > syncDuration / GameState.waveFactor)
        {
            syncedAtTime = -1;
        }

        // rotate over time
        transform.Rotate(Time.deltaTime * 120, Time.deltaTime * 80, 0);
    }
}