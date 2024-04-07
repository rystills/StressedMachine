using System.Collections.Generic;
using UnityEngine;

public class AmbientSoundManager : MonoBehaviour
{
    [SerializeField] private float activationChance = .01f;
    [SerializeField] GameObject ambientSoundPrefab;
    [SerializeField] int numSources;
    [SerializeField] private List<AudioClip> clips;
    private List<AudioSource> sources;

    private void Awake()
    {
        sources = new List<AudioSource>(numSources);
        for (int i = 0; i < numSources; ++i) sources.Add(Instantiate(ambientSoundPrefab, transform).GetComponent<AudioSource>());
    }

    private void Update()
    {
        // check for available sources
        foreach (AudioSource source in sources)
        {
            // attempt to play a random sound
            if (!source.isPlaying && Random.Range(0f, 1f) * Time.deltaTime <= activationChance)
            {
                source.transform.position = new(Random.Range(30f, 60f) * (Random.Range(0, 2) * 2 - 1),
                                                Random.Range(-10f, 10f),
                                                Random.Range(30f, 60f) * (Random.Range(0, 2) * 2 - 1));
                source.clip = clips[Random.Range(0, clips.Count)];
                source.Play();
            }
        }
    }
}
