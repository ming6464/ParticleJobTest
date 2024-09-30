using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;

public class ParticleFlowMoveJob : MonoBehaviour
{
    public  TfPointCurve[]   pointCurves;
    public  float          _spaceTime = 0.01f;
    private float          _startTime;
    private FlowMoveJob    _flowMoveJob;
    private ParticleSystem _particleSystem;
    private NativeArray<PointCurve> _nativePointCurves;

    private void Start()
    {
        _nativePointCurves = new(pointCurves.Length, Allocator.Persistent);
        _startTime         = Time.time;
        _particleSystem    = GetComponent<ParticleSystem>();
        _flowMoveJob = new()
        {
                startTime = _startTime,
        };
    }

    private void Update()
    {
        LoadNavtiveArray();
        UpdateFlowJob();
    }
    private void UpdateFlowJob()
    {
        _spaceTime               = 1f/_particleSystem.particleCount;
        _flowMoveJob.spaceTime   = _spaceTime;
        _flowMoveJob.pointCurves = _nativePointCurves;
        _flowMoveJob.time        = Time.time;
        OnParticleUpdateJobScheduled();
    }
    private void LoadNavtiveArray()
    {
        for (var i = 0; i < pointCurves.Length; i++)
        {
            var pointCurve = pointCurves[i];
            _nativePointCurves[i] = new()
            {
                    point1 = pointCurve.tfPoint1.position,
                    point2 = pointCurve.tfPoint2.position,
                    point3 = pointCurve.tfPoint3.position
            };
        }
    }
    

    private void OnParticleUpdateJobScheduled()
    {
        _flowMoveJob.ScheduleBatch(_particleSystem, 32);
    }
    private void OnDrawGizmos()
    {
        foreach (var pointCurve in pointCurves)
        {
            DrawCurve3(pointCurve.tfPoint1, pointCurve.tfPoint2, pointCurve.tfPoint3);
        }
    }
    private void DrawCurve3(Transform tfPoint1, Transform tfPoint2, Transform tfPoint3)
    {
        var point1 = tfPoint1.position;
        var point2 = tfPoint2.position;
        var point3 = tfPoint3.position;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(point1, 0.3f);
        Gizmos.color = Color.green;
        Gizmos.DrawSphere(point2, 0.3f);
        Gizmos.color = Color.blue;
        Gizmos.DrawSphere(point3, 0.3f);

        for (var i = 0; i < 100; i++)
        {
            var t1 = i / 100f;
            var p  = Curve_L(point1, point2, point3, t1);
            var t2 = (i + 1) / 100f;
            var q  = Curve_L(point1, point2, point3, t2);
            var c1 = Color.Lerp(Color.red, Color.green, t1);
            var c2 = Color.Lerp(Color.green, Color.blue, t2);
            Gizmos.color = Color.Lerp(c1, c2, (t1 + t2)/2f);
            Gizmos.DrawLine(p, q);
        }
        
        Vector3 Curve_L(Vector3 a, Vector3 b, Vector3 c, float t)
        {
            return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
        }
    }
}

public struct FlowMoveJob : IJobParticleSystemParallelForBatch
{
    [ReadOnly]
    public NativeArray<PointCurve> pointCurves;
    
    [ReadOnly]
    public float                   time;
    
    [ReadOnly]
    public float                   startTime;
    
    [ReadOnly]
    public float                   spaceTime;
    public void Execute(ParticleSystemJobData particles, int startIndex, int count)
    {
        var endCount  = startIndex + count;
        var positions = particles.positions;
        var subtract  = time - startTime;
        
        
        for (var i = 0; i < endCount; i++)
        {
            var subtract2     = subtract - spaceTime * i;
            var floor = Mathf.Floor(subtract2);
            var index = (int)floor;
            positions[i] = MoveCurve(index, subtract2);
        } 
    }
    
    private Vector3 MoveCurve(int index, float subtract2)
    {
        var indexReal  = index;

        if (index >= pointCurves.Length)
        {
            indexReal = 0;
        }
        var pointCurve = pointCurves[indexReal];
        var point1     = pointCurve.point1;
        var point2     = pointCurve.point2;
        var point3     = pointCurve.point3;
        var t          = frac(subtract2);
        return Curve(point1, point2, point3, t);
    }
    
    private Vector3 Curve(Vector3 a, Vector3 b, Vector3 c, float t)
    {
        return Vector3.Lerp(Vector3.Lerp(a, b, t), Vector3.Lerp(b, c, t), t);
    }
    
    private float frac(float x)
    {
        return x - Mathf.Floor(x);
    }
}

[Serializable]
public struct PointCurve
{
    public Vector3 point1;
    public Vector3 point2;
    public Vector3 point3;
}

[Serializable]
public struct TfPointCurve
{
    public Transform tfPoint1;
    public Transform tfPoint2;
    public Transform tfPoint3;
}