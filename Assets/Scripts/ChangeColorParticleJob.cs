using UnityEngine;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine.ParticleSystemJobs;

public class ChangeColorParticleJob : MonoBehaviour
{
    public Color32 color1 = Color.red;
    public Color32 color2 = Color.blue;
    
    private ParticleSystem            _particleSystem;
    private ParticleSystem.Particle[] _particles;
    private ChangeColorJob            _changeColorJob;
    private JobHandle                 _jobHandle;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        if (_particleSystem == null)
        {
            Debug.LogError("ParticleSystem component not found!");
            return;
        }

        _particles = new ParticleSystem.Particle[_particleSystem.main.maxParticles];
    }

    private void Update()
    {
        if (_particleSystem == null) return;

        int numParticlesAlive = _particleSystem.GetParticles(_particles);

        _changeColorJob = new ChangeColorJob
        {
                color1    = color1,
                color2    = color2,
                particles = new NativeArray<ParticleSystem.Particle>(_particles, Allocator.TempJob)
        };

        _jobHandle = _changeColorJob.Schedule(numParticlesAlive, 64);
        _jobHandle.Complete();

        _changeColorJob.particles.CopyTo(_particles);
        _particleSystem.SetParticles(_particles, numParticlesAlive);

        _changeColorJob.particles.Dispose();
    }

    private void OnDisable()
    {
        if (_jobHandle.IsCompleted)
        {
            _jobHandle.Complete();
        }
    }
}

[BurstCompile]
public struct ChangeColorJob : IJobParallelFor
{
    public Color32                              color1;
    public Color32                              color2;
    public NativeArray<ParticleSystem.Particle> particles;

    public void Execute(int index)
    {
        ParticleSystem.Particle particle = particles[index];
        particle.startColor = (index % 2 == 0) ? color1 : color2;
        particles[index]    = particle;
    }
}