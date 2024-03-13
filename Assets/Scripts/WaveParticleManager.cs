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

    private void Awake()
    {
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

    private void LateUpdate()
    {
        // wave particles follow a sin curve
        for (int i = 0; i < particleCount; ++i)
            waveParticles[i].position = new(waveParticles[i].position.x, Mathf.Sin((Time.time + i * spacing) * cycleSpeed) * halfHeight, waveParticles[i].position.z);
        wavePs.SetParticles(waveParticles);

        // outline particles rotate around the wave particles
        float angle;
        Vector3 basePos;
        for (int i = 0; i < outlineParticleCount; ++i)
        {
            angle = Time.time + i * .3f;
            basePos = transform.forward * (i / (float)outlineParticleCount * length - length / 2);
            outlineParticles[i].position = new Vector3(basePos.x, Mathf.Sin((Time.time + (i/4f) * spacing) * cycleSpeed) * halfHeight, basePos.z)
                                         + new Vector3(outlineOffset * Mathf.Cos(angle), 0, outlineOffset * Mathf.Sin(angle));
        }
        outlinePs.SetParticles(outlineParticles);
    }
}
