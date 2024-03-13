using UnityEngine;

public class WaveParticleManager : MonoBehaviour
{
    [SerializeField] private int particleCount;
    private int outlineParticleCount => particleCount * 4;
    [SerializeField] private float cycleSpeed = 1f;
    [SerializeField] private float length;
    [SerializeField] private float halfHeight;
    [SerializeField] private float spacing;
    [SerializeField] private float outlineOffset;
    private ParticleSystem wavePs;
    private ParticleSystem.Particle[] waveParticles;
    private ParticleSystem outlinePs;
    private ParticleSystem.Particle[] outlineParticles;
    private Vector3[] offsets;

    private void Awake()
    {
        offsets = new Vector3[4] { transform.up * outlineOffset, -transform.up * outlineOffset, -transform.right * outlineOffset, transform.right * outlineOffset };
        wavePs = GetComponent<ParticleSystem>();
        outlinePs = transform.GetChild(0).GetComponent<ParticleSystem>();
        waveParticles = new ParticleSystem.Particle[particleCount];
        outlineParticles = new ParticleSystem.Particle[outlineParticleCount];
        wavePs.Emit(particleCount);
        outlinePs.Emit(outlineParticleCount);
        wavePs.GetParticles(waveParticles);
        outlinePs.GetParticles(outlineParticles);
        for (int i = 0; i < particleCount; ++i)
            waveParticles[i].position = transform.forward * (i / (float)particleCount * length - length / 2);
    }

    private void Update()
    {
        for (int i = 0; i < particleCount; ++i)
            waveParticles[i].position = new(waveParticles[i].position.x, Mathf.Sin(Time.time + i * spacing * cycleSpeed) * halfHeight, waveParticles[i].position.z);
        for (int i = 0; i < outlineParticleCount; ++i)
            outlineParticles[i].position = waveParticles[i / 4].position + offsets[i % 4];

        wavePs.SetParticles(waveParticles);
        outlinePs.SetParticles(outlineParticles);
    }
}
