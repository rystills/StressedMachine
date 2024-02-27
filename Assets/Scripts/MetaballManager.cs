using System.Collections.Generic;
using UnityEngine;

public class MetaballManager : MonoBehaviour
{
    [SerializeField] int particleCount;
    [SerializeField] float accel;
    [SerializeField] float maxVel;
    private ParticleSystem mbPs;
    List<Vector3> targetPositions;
    List<Vector3> velocities;
    ParticleSystem.Particle[] particles;
    private Vector3 RandomVectorInCube(float halfExtents) => new Vector3(Random.Range(-halfExtents, halfExtents),
                                                                         Random.Range(-halfExtents, halfExtents),
                                                                         Random.Range(-halfExtents, halfExtents));
    private Vector3 RandomVectorInSphere(float magnitude) => Random.insideUnitSphere * magnitude;
    private Vector3 RandomVectorOnSphere(float magnitude) => Random.insideUnitSphere.normalized * magnitude;

    private void Awake()
    {
        mbPs = GetComponent<ParticleSystem>();
        particles = new ParticleSystem.Particle[particleCount];
        targetPositions = new List<Vector3>(particleCount);
        velocities = new List<Vector3>(particleCount);
        for (int i = 0; i < particleCount; ++i)
        {
            targetPositions.Add(RandomVectorOnSphere(2f));
            velocities.Add(RandomVectorOnSphere(2f));
        }
        mbPs.Emit(particleCount);
        mbPs.GetParticles(particles);
        for (int i = 1; i < particleCount; ++i) particles[i].position = RandomVectorOnSphere(2f);
        particles[0].startSize *= 3;
    }

    private void Update()
    {
        for (int i = 1; ++i < particleCount; ++i)
        {
            velocities[i] = Vector3.ClampMagnitude(velocities[i] + (targetPositions[i] - particles[i].position).normalized 
                                                                 * (accel * Time.deltaTime
                                                                 * (targetPositions[i] - particles[i].position).magnitude * 3f), maxVel);
            particles[i].position += (velocities[i] + Vector3.Lerp(particles[i].position, Vector3.zero, Mathf.Sqrt(particles[i].position.magnitude * .1f))) * Time.deltaTime;

            if ((targetPositions[i] - particles[i].position).magnitude < 20)
            {
                targetPositions[i] = RandomVectorOnSphere(2f);
            }
        }
        mbPs.SetParticles(particles);
    }
}
