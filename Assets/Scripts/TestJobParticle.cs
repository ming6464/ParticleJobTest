using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using Random = Unity.Mathematics.Random;

public class TestJobParticle : MonoBehaviour
{
    public Vector3   localVelocity;
    public Transform point1;
    public Transform point2;
    [Header("--")]
    public float    speed = 1f;
    
    private ParticleSystem _particleSystem;
    private SmokeJob      _smokeJob;

    private void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _smokeJob = new()
        {
            localVelocity = localVelocity
        };
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point1.position, 0.5f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(point2.position, 0.5f);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(point1.position, point2.position);
    }

    private void Update()
    {
        if(point1 && point2)
            localVelocity = (point2.position - point1.position).normalized * speed;
        _smokeJob.localVelocity = localVelocity;
    }

    private void OnParticleUpdateJobScheduled()
    {
       _smokeJob.ScheduleBatch(_particleSystem, 32);
    }
}

[BurstCompile]
public struct SmokeJob : IJobParticleSystemParallelForBatch
{
    public Vector3 localVelocity;
    

    public void Execute(ParticleSystemJobData particles, int startIndex, int count)
    {
        var velocities = particles.velocities;
        var endIndex   = startIndex + count;
        var p          = particles.startColors;
        
        
        for (var i = startIndex; i < endIndex; i++)
        {
            var random = Random.CreateFromIndex((uint)(i * 1.5f));
            velocities[i] = localVelocity * random.NextFloat3(new (i * 1.5f,i * -1.5f,i * -0.5f));
            p[0]          = Color32.Lerp(p[0], new(255, 0, 0, 255), 0.1f * count);
        }
    }
}



public struct Point
{
    public int index1;
    public int index2;
    public int index3;
}