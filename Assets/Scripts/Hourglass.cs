using UnityEngine;

public class Hourglass : MonoBehaviour
{
    private const int numLevels = 13;
    private const int particleCount = (numLevels * (numLevels + 1) * (2 * numLevels + 1)) / 6;
    private ParticleSystem ps;
    private Vector3[] startPositions = new Vector3[particleCount];
    private Vector3[] endPositions = new Vector3[particleCount];
    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
    private float totalTimeElapsed;
    [SerializeField] private float duration = 25;
    [SerializeField] private float spacing = .25f;
    [SerializeField] private Color32 startColor, endColor;
    private bool flipped;
    private Vector3 centerPos = new(-.04f, 0, -.04f);

    private void Awake()
    {
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
    }

    public void Flip() => flipped = !flipped;
    
    private void LateUpdate()
    {
        totalTimeElapsed = Mathf.Clamp(totalTimeElapsed + Time.deltaTime * (flipped ? -1 : 1), 0, duration + .7f);
        float timeRatio = totalTimeElapsed / duration;

        // update particles
        for (int i = 0; i < particleCount; ++i)
        {
            float indRatio = i / (float)particleCount;
            float lerpFac = Mathf.Pow(Mathf.Clamp01(timeRatio - indRatio), 2) * 1000;

            // lerp towards end position
            particles[i].position = Vector3.Lerp(startPositions[i], endPositions[particleCount - i - 1], lerpFac);
            // lerp towards center
            particles[i].position = Vector3.Lerp(particles[i].position, centerPos,
                                                 particles[i].position.y > 0 ? 1 - particles[i].position.y / startPositions[i].y
                                                                             : 1 - particles[i].position.y / endPositions[particleCount - i - 1].y);
            particles[i].startColor = Color.Lerp(startColor, endColor, lerpFac);
        }
        ps.SetParticles(particles);

        // rotate when flipped
        transform.localEulerAngles = new(transform.localEulerAngles.x,
                                         transform.localEulerAngles.y, 
                                         Mathf.MoveTowardsAngle(transform.localEulerAngles.z, flipped ? 180 : 0, Time.deltaTime * 1000));
    }
}
