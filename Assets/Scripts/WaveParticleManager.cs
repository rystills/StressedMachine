using UnityEngine;
using UnityEngine.UI;

public class WaveParticleManager : MonoBehaviour
{
    public static WaveParticleManager instance;

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
    [SerializeField] private Image waveOverlayImg;
    private Color greenCol = new(.751f, 1.147f, .683f, 1);
    private Color redCol   = new(1.147f, .751f, .683f, 1);
    public static float desyncAmount;
    [SerializeField] private float desyncIncr;
    [SerializeField] private float desyncDecr;
    [SerializeField] private float syncDuration;
    private float syncedAtTime;
    [SerializeField] private float pitchFactor;
    [SerializeField] private float pitchSyncRatio = .87f;
    [SerializeField] private AudioSource particleSnd;
    [SerializeField] private AudioSource outlineSnd;
    [SerializeField] private AudioSource desyncSnd;
    [SerializeField] private AudioSource justDesyncedSnd;

    private bool heightsSynced => Mathf.Abs(outlineHeightOffset - heightOffset) <= heightSyncDist;
    private bool targetHeightsSynced => Mathf.Abs(outlineHeightOffsetTarget - heightOffset) <= heightSyncDist;

    private void Awake()
    {
        instance = this;

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

        wavePs.SetParticles(waveParticles);
        outlinePs.SetParticles(outlineParticles);
        enabled = false;
    }

    public void Randomize()
    {
        do outlineHeightOffsetTarget = Random.Range(-heightRandomMax, heightRandomMax); while (targetHeightsSynced);
    }

    private void OnEnable()
    {
        particleSnd.Play();
        outlineSnd.Play();
        syncedAtTime = Time.time;
    }

    public void AdjustHeightOffset(float amnt) => heightOffset = Mathf.Clamp(heightOffset + amnt, -heightRandomMax, heightRandomMax);
    public void SetHeightOffsetRatio(float heightRatio) => heightOffset = Mathf.Clamp(heightRandomMax * heightRatio, -1, 1);
    public static void Reset()
    {
        desyncAmount = 0;
        instance.outlineHeightOffset = instance.outlineHeightOffsetTarget = instance.heightOffset;
        instance.prevHeightsSync = true;
        instance.syncedAtTime = Time.time;
        instance.waveParticleMat.SetColor("_EmissionColor", instance.heightsSynced ? instance.greenCol : instance.redCol);
        instance.FlushEffects();
    }

    public void FlushEffects()
    {
        if (GameState.globalFactor != 0)
        {
            desyncAmount = Mathf.Clamp01(desyncAmount + (heightsSynced ? -desyncDecr : desyncIncr) * Time.deltaTime * GameState.globalFactor);
            if (desyncAmount == 1) Player.Die(DeathBy.WaveDesync);
        }

        waveOverlayMat.SetFloat("strength", desyncAmount);
        waveOverlayImg.enabled = desyncAmount > 0;
    }

    private void LateUpdate()
    {
        // rebalance
        if (GameState.rebalancing)
        {
            outlineHeightOffset = outlineHeightOffsetTarget = Mathf.MoveTowards(outlineHeightOffset, heightOffset, 80 * Time.deltaTime);
            desyncAmount = Mathf.MoveTowards(desyncAmount, 0, 80 * Time.deltaTime);
            instance.syncedAtTime = Time.time;
            FlushEffects();
        }

        // desync after syncDuration elapses
        if (syncedAtTime >= 0 && Time.time - syncedAtTime > syncDuration / GameState.waveFactor)
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
            if (!heightsSynced) justDesyncedSnd.PlayBiDir();
            else                justDesyncedSnd.PlayBiDir(true);
        }

        if (GameState.waveFactor != 0) FlushEffects();

        // adjust sounds
        outlineSnd.pitch = outlineHeightOffset * pitchFactor * GameState.powerDownFactor;
        particleSnd.pitch = heightOffset * pitchFactor * pitchSyncRatio * GameState.powerDownFactor;

        desyncSnd.volume = Mathf.Pow(desyncAmount, 6);
    }
}
