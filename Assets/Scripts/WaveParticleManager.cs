using UnityEngine;

public class WaveParticleManager : MonoBehaviour
{
    [SerializeField] private int particleCount;
    private int outlineParticleCount => particleCount * 4;
    [SerializeField] private float cycleSpeed = 1f;
    [SerializeField] private float length;
    [SerializeField] private float spacing;
    [SerializeField] private float outlineOffset;
    private ParticleSystem wavePs;
    private ParticleSystem.Particle[] waveParticles;
    private ParticleSystem outlinePs;
    private ParticleSystem.Particle[] outlineParticles;
    private float heightOffset, outlineHeightOffset, outlineHeightOffsetTarget;
    [SerializeField] private float outlineHeightOffsetSpeed;
    [SerializeField] private float heightSyncDist;
    [SerializeField] private float heightRandomMax;
    private bool prevHeightsSync;
    [SerializeField] private Material waveParticleMat;
    [SerializeField] private Material waveOverlayMat;
    private Color greenCol = new(.751f, 1.147f, .683f, 1);
    private Color redCol   = new(1.147f, .751f, .683f, 1);
    public static float desyncAmount;
    [SerializeField] private float desyncIncr;
    [SerializeField] private float desyncDecr;
    [SerializeField] private float syncDuration;
    private float syncedAtTime;

    private bool heightsSynced => Mathf.Abs(outlineHeightOffset - heightOffset) <= heightSyncDist;
    private bool targetHeightsSynced => Mathf.Abs(outlineHeightOffsetTarget - heightOffset) <= heightSyncDist;

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
            waveParticles[i].position = Vector3.forward * (i / (float)particleCount * length - length / 2);
    }

    public void Randomize()
    {
        do outlineHeightOffsetTarget = Random.Range(-heightRandomMax, heightRandomMax); while (targetHeightsSynced);
    }

    public void AdjustHeightOffset(float amnt) => heightOffset = Mathf.Clamp(heightOffset + amnt, -heightRandomMax, heightRandomMax);

    private void LateUpdate()
    {
        // desync after syncDuration elapses
        if (syncedAtTime >= 0 && Time.time - syncedAtTime > syncDuration)
        {
            Randomize();
            syncedAtTime = -1;
        }

        // shift outline height offset towards target
        outlineHeightOffset = Mathf.MoveTowards(outlineHeightOffset, outlineHeightOffsetTarget, outlineHeightOffsetSpeed * Time.deltaTime);

        // wave particles follow a sin curve
        for (int i = 0; i < particleCount; ++i)
            waveParticles[i].position = new(waveParticles[i].position.x,
                                            Mathf.Sin((Time.time + i * spacing) * cycleSpeed) * heightOffset,
                                            waveParticles[i].position.z);
        wavePs.SetParticles(waveParticles);

        // outline particles rotate around the wave particles
        float angle;
        Vector3 basePos;
        for (int i = 0; i < outlineParticleCount; ++i)
        {
            angle = Time.time + i * .3f;
            basePos = Vector3.forward * (i / (float)outlineParticleCount * length - length / 2);
            outlineParticles[i].position = new Vector3(basePos.x, Mathf.Sin((Time.time + i/4f * spacing) * cycleSpeed) * outlineHeightOffset, basePos.z)
                                         + new Vector3(outlineOffset * Mathf.Cos(angle), 0, outlineOffset * Mathf.Sin(angle));
        }
        outlinePs.SetParticles(outlineParticles);

        // update wave color
        if (heightsSynced != prevHeightsSync)
        {
            waveParticleMat.SetColor("_EmissionColor", heightsSynced ? greenCol : redCol);
            prevHeightsSync = heightsSynced;

            // begin sync timer
            if (syncedAtTime == -1 && heightsSynced)
                syncedAtTime = Time.time;
        }

        // adjust desync amount (overlay strength)
        desyncAmount = Mathf.Clamp01(desyncAmount + ( heightsSynced ? -desyncDecr : desyncIncr) * Time.deltaTime);
        if (desyncAmount == 1) Player.Die(DeathBy.WaveDesync);

        waveOverlayMat.SetFloat("strength", desyncAmount);
    }
}
