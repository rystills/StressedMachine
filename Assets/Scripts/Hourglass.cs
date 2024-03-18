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
    [SerializeField] private float duration = 60;
    [SerializeField] private float spacing = .25f;
    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Emit(particleCount);
        ps.GetParticles(particles);
        int i = 0;
        for (int level = 1; level <= numLevels; ++level)
            for (int row = 0; row < level; ++row)
                for (int col = 0; col < level; ++col, ++i)
                {
                    startPositions[i] = new(row * spacing - (level * spacing / 2),
                                            level * spacing,
                                            col * spacing - (level * spacing / 2));
                    endPositions[i]   = new(row * spacing - (level * spacing / 2),
                                            -level * spacing,
                                            col * spacing - (level * spacing / 2));
                    particles[i].position = startPositions[i];
                }
    }

    private void LateUpdate()
    {
        float timeRatio = (Time.time - startTime) / duration;

        for (int i = 0; i < particleCount; ++i)
        {
            float indRatio = i / (float)particleCount;
            particles[i].position = indRatio >= timeRatio ? startPositions[i] : endPositions[particleCount - i - 1];
        }
        ps.SetParticles(particles);
    }
}
