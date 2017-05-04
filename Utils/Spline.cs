using System;
using System.Collections;
using System.Collections.Generic;
using Sabresaurus.SabreCSG;
using UnityEngine;

public class Spline : MonoBehaviour
{
    [Header("Debug")]
    public bool DrawPoints;

    [Header("Points")]
    public bool Loop;

    [EnumFlagsAttribute]
    public SplineTransform AdditionalTransform;

    [HideInInspector]
    public SpllinePoint[] Points;

    [Flags]
    public enum SplineTransform
    {
        Rotation = 1,
        Scale = 2,
    }

    [System.Serializable]
    public struct SpllinePoint
    {
        public SpllinePoint(Vector3 pos)
        {
            position = pos;
            rotation = Quaternion.identity;
            scale    = Vector3.one;
        }

        public SpllinePoint(Vector3 pos, Quaternion rot)
        {
            position = pos;
            rotation = rot;
            scale = Vector3.one;
        }

        public SpllinePoint(Vector3 pos, Vector3 scl)
        {
            position = pos;
            rotation = Quaternion.identity;
            scale = scl;
        }

        public SpllinePoint(Vector3 pos, Quaternion rot, Vector3 scl)
        {
            position = pos;
            rotation = rot;
            scale = scl;
        }

        [SerializeField]
        public Vector3    position;

        [SerializeField]
        public Quaternion rotation;

        [SerializeField]
        public Vector3    scale;
    }

    public void Reset()
    {
        Points = new SpllinePoint[]
        {
            new SpllinePoint(),
            new SpllinePoint(),
            new SpllinePoint(),
        };
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.gray;

        foreach (var point in Points)
        {
            var transformedPoint = transform.TransformPoint(point.position);
            Gizmos.DrawSphere(transformedPoint, point.scale.magnitude * 0.25f);
        }
    }

    void OnDrawGizmos()
    {
        if (!DrawPoints)
            return;

        OnDrawGizmosSelected();
    }
}
