using UnityEngine;

public class Hourglass : MonoBehaviour
{
    private const int numLevels = 12;
    private const int particleCount = (numLevels * (numLevels + 1) * (2 * numLevels + 1)) / 6;
    private ParticleSystem ps;
    private Vector3[] startPositions = new Vector3[particleCount];
    private Vector3[] endPositions = new Vector3[particleCount];
    private ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
    private float startTime;
    [SerializeField] private float duration = 25;
    [SerializeField] private float spacing = .25f;
    [SerializeField] private float fallDuration = .002f;

    private void Awake()
    {
        // setup particle system
        ps = GetComponent<ParticleSystem>();
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

    private void LateUpdate()
    {
        float timeRatio = (Time.time - startTime) / duration;

        for (int i = 0; i < particleCount; ++i)
        {
            float indRatio = i / (float)particleCount;
            particles[i].position = Vector3.Lerp(startPositions[i], endPositions[particleCount - i - 1],
                Mathf.Pow(Mathf.Clamp01(timeRatio - indRatio + fallDuration), 2) / (2 * fallDuration));
        }
        ps.SetParticles(particles);
    }
}
