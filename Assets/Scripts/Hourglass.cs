using UnityEngine;
using UnityEngine.UI;

public class Hourglass : MonoBehaviour
{
    public static Hourglass instance;
    private const int numLevels = 13;
    private const int particleCount = numLevels * (numLevels + 1) * (2 * numLevels + 1) / 6;
    private ParticleSystem ps;
    private Vector3[] startPositions = new Vector3[particleCount];
    private Vector3[] endPositions = new Vector3[particleCount];
    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
    private float totalTimeElapsed;
    [SerializeField] private float duration = 25;
    [SerializeField] private float finishExtraDuration;
    private float fullDuration => duration + finishExtraDuration;
    [SerializeField] private float spacing = .25f;
    [SerializeField] private Color32 endColor;
    private bool flipped;
    private Vector3 centerPos = new(-.04f, 0, -.04f);
    [SerializeField] private float boundsClampFactor = 0.455f;
    private float lastRotTime;
    [SerializeField] private AudioSource snapSnd;
    [SerializeField] private AudioSource activeSnd;
    [SerializeField] private AudioSource overlaySnd;
    private const float maxTimestep = .1f;
    [SerializeField] private float overlayFadeDuration = 7;
    [SerializeField] private Material overlayMat;
    [SerializeField] private Image overlayImg;

    private void Awake()
    {
        instance = this;
        
        // setup particle system
        ps = transform.GetChild(0).GetComponent<ParticleSystem>();
        ps.Emit(particleCount);
        ps.GetParticles(particles);

        // assign particle positions layer by layer
        for (int i = 1, level = 1; level <= numLevels; i += level * level, ++level, i += level * level)
        {
            void SetPartStartPos(int row, int col) 
                => particles[--i].position = startPositions[i] = new(row * spacing - (level * spacing / 2),
                                                                     level * spacing,
                                                                     col * spacing - (level * spacing / 2));

            // spiral out from the center
            for (int top = 0, left = top, bottom = level - 1, right = bottom;
                top <= bottom && left <= right;)
            {
                for (int col = left; col <= right; ++col)
                    SetPartStartPos(top, col);
                ++top;

                for (int row = top; row <= bottom; ++row)
                    SetPartStartPos(row, right);
                --right;

                if (top <= bottom)
                {
                    for (int col = right; col >= left; --col)
                        SetPartStartPos(bottom, col);
                    --bottom;
                }

                if (left <= right)
                {
                    for (int row = bottom; row >= top; --row)
                        SetPartStartPos(row, left);
                    ++left;
                }
            }
            
            // assign end positions in inverted order
            for (int j = 0; j < level * level; ++j)
                endPositions[i + level * level - j - 1] = new(startPositions[i + j].x, -startPositions[i + j].y, startPositions[i + j].z);
        }

        enabled = false;

        // flush the particles
        Reset();
        LateUpdate();
    }

    public void AddRotation(float amnt)
    {
        transform.localEulerAngles = new(transform.localEulerAngles.x,
                                         transform.localEulerAngles.y,
                                         transform.localEulerAngles.z + amnt / 2);
        lastRotTime = Time.time;
    }

    public static void Reset()
    {
        instance.totalTimeElapsed = instance.fullDuration / 2;
        instance.FlushEffects();
    }

    public void FlushEffects()
    {
        float overlayStrength = Mathf.Max(Mathf.Clamp01((overlayFadeDuration - (fullDuration - totalTimeElapsed) / GameState.hourglassFactor) / overlayFadeDuration),
                                          1 - Mathf.Clamp01(totalTimeElapsed / GameState.hourglassFactor / overlayFadeDuration));
        overlaySnd.volume = Mathf.Pow(overlayStrength, 6);
        overlayMat.SetFloat("strength", overlayStrength);
        overlayMat.SetFloat("isPurple", totalTimeElapsed <= fullDuration / 2 ? 0 : 1);
        overlayImg.enabled = overlayStrength > 0;
        if (overlayStrength == 1) Player.Die(DeathBy.TimeDecompression);
    }

    private void LateUpdate()
    {
        flipped = transform.localEulerAngles.z > 90 && transform.localEulerAngles.z < 270;
        float deltaAngle = Mathf.DeltaAngle(transform.localEulerAngles.z, flipped ? 180 : 0);

        // scale elapsed time by rotation angle
        float rotSpeed = (flipped ? -1 : 1) * (1 - Mathf.Abs(deltaAngle) / 90);
        totalTimeElapsed = Mathf.Clamp(totalTimeElapsed + Time.deltaTime * GameState.hourglassFactor * rotSpeed, 0, fullDuration);
        float timeRatio = totalTimeElapsed / duration;

        // update particles
        for (int i = 0; i < particleCount; ++i)
        {
            float indRatio = (i + 1f) / particleCount;

            // lerp gradually down
            float lerpFac = Mathf.Pow(Mathf.Clamp01(timeRatio - indRatio), 2) * 1000;
            lerpFac = Mathf.Lerp(lerpFac, (lerpFac == 0 ? (1 - (indRatio - timeRatio) / indRatio) * .35f
                                                        : lerpFac * .65f + .35f), Mathf.Sqrt(timeRatio));

            // lerp towards end position
            particles[i].position = Vector3.Lerp(startPositions[i], endPositions[particleCount - i - 1], lerpFac);

            // lerp towards center
            particles[i].position = Vector3.Lerp(particles[i].position, centerPos,
                                                 particles[i].position.y > 0 ? .90f - particles[i].position.y / startPositions[i].y
                                                                             : .90f - particles[i].position.y / endPositions[particleCount - i - 1].y);

            // clamp to inner glass bounds (roughly)
            float bounds = Mathf.Max(Mathf.Abs(particles[i].position.y) * boundsClampFactor, .03f);
            particles[i].position = new(Mathf.Clamp(particles[i].position.x, -bounds + centerPos.x, bounds + centerPos.x),
                                        particles[i].position.y,
                                        Mathf.Clamp(particles[i].position.z, -bounds + centerPos.z, bounds + centerPos.z));

            particles[i].startColor = Color.Lerp(ps.main.startColor.color, endColor, lerpFac);
        }
        ps.SetParticles(particles);

        // rotate towards nearest base
        if (Time.time != lastRotTime)
        {
            transform.localEulerAngles = new(transform.localEulerAngles.x,
                                             transform.localEulerAngles.y,
                                             transform.localEulerAngles.z + Mathf.Lerp(0, deltaAngle, Time.time - lastRotTime));
            if (!snapSnd.isPlaying && TimeExt.Since(lastRotTime) <= maxTimestep && deltaAngle != 0)
                snapSnd.Play();
        }

        if (GameState.hourglassFactor != 0) FlushEffects();

        // control active (sand falling) sound
        if (totalTimeElapsed <= 0 || totalTimeElapsed >= fullDuration)
            activeSnd.Stop();
        else
        {
            activeSnd.pitch = rotSpeed * 2;
            if (enabled && !activeSnd.isPlaying) activeSnd.Play();
        }
    }
}
