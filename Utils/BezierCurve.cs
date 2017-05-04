using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BezierCurve : MonoBehaviour
{
	public	bool			DrawGizmo;
	public	float			Stride = 10;

	[HideInInspector]
	public	List<Transform>	TransformPoints;

//	////////////////////////////////////////////////////////////////////////////
	#region BezierPoint

	[System.Serializable]
	public struct BezierPoint
	{
		[SerializeField]
		public	Vector3	Point;

		[SerializeField]
		public	Vector3	Normal;

		[SerializeField]
		public	Vector3 Forward;

		public BezierPoint(Transform trans)
		{
			Point		= trans.position;
			Normal		= trans.up;
			Forward		= trans.forward;
		}

		public static BezierPoint CalcBezier(BezierPoint a, BezierPoint b, float t)
		{
			//	tmp[k] = tmp[k] + t * ( tmp[k+1] - tmp[k] );
			return new BezierPoint
			{
				Normal	= a.Normal + t * (b.Normal - a.Normal),
				Point	= a.Point + t * (b.Point - a.Point),
				Forward	= (a.Point - b.Point).normalized,
			};
		}

		public static BezierPoint operator+ (BezierPoint a, BezierPoint b)
		{
			return new BezierPoint
			{
				Forward	= a.Forward + b.Forward,
				Normal	= a.Normal + b.Normal,
				Point	= a.Point + b.Point
			};
		}

		public static BezierPoint operator- (BezierPoint a, BezierPoint b)
		{
			return new BezierPoint
			{
				Forward	= a.Forward - b.Forward,
				Normal	= a.Normal - b.Normal,
				Point	= a.Point - b.Point
			};
		}

		public static BezierPoint operator* (BezierPoint a, float s)
		{
			return new BezierPoint
			{
				Forward	= a.Forward * s,
				Normal	= a.Normal * s,
				Point	= a.Point * s
			};
		}

		public static BezierPoint operator* (float s, BezierPoint a)
		{
			return new BezierPoint
			{
				Forward	= a.Forward * s,
				Normal	= a.Normal * s,
				Point	= a.Point * s
			};
		}
	}

	#endregion
//	////////////////////////////////////////////////////////////////////////////

//	////////////////////////////////////////////////////////////////////////////
	#region Gizmo

#if UNITY_EDITOR

	void OnDrawGizmos()
	{
		if (!DrawGizmo || (!Selection.Contains(this)
		&& !GetComponentsInChildren<Transform>().Any(t => Selection.Contains(t.gameObject))
		&& !TransformPoints						.Any(t => Selection.Contains(t.gameObject))))
			return;

		if (TransformPoints == null || TransformPoints.Count < 2)
			return;

		var oldColor	= Gizmos.color;
		var array		= GetBezierPoints();
		var previous	= array.First();

		var distance	= CalcDistance();
		int		count	= Mathf.FloorToInt(distance/Stride);

		for(int i = 0; i < count; i++)
		{
			var point	= CalcBezierPoint(array, i / (float)count);

			DrawBezierGizmo(previous, point);

			previous = point;
		}
		DrawBezierGizmo(previous, array.Last());
		DrawBezierGizmo(array.Last(), array.Last());

		Gizmos.color	= Color.yellow;
		previous		= array.First();
		foreach (var point in array)
		{
			Gizmos.DrawLine(previous.Point, point.Point);
			previous = point;
		}

		Gizmos.color = oldColor;
	}

	internal void DrawBezierGizmo(BezierPoint previous, BezierPoint point)
	{
		Gizmos.color = Color.green;
		Gizmos.DrawLine	(previous.Point, point.Point);

		Gizmos.color = Color.cyan;
		Gizmos.DrawRay	(previous.Point, previous.Normal);
	}

#endif

	#endregion Gizmo
//	////////////////////////////////////////////////////////////////////////////

//	////////////////////////////////////////////////////////////////////////////
	#region BezierCalc

	internal float CalcDistance()
	{
		var distance = 0.0f;
		var previous = TransformPoints[0];
		for (int i = 1; i < TransformPoints.Count; i++)
		{
			var current  = TransformPoints[i];
			distance	+= Vector3.Distance(previous.position, current.position);
			previous	 = current;
		}
		return distance;
	}

	internal List<BezierPoint> CalcBezierCurve(List<BezierPoint> points = null)
	{
		if (points == null)
			points = GetBezierPoints();

		var distance	= CalcDistance();
		int		count	= Mathf.FloorToInt(distance/Stride);

		var result = new List<BezierPoint>(points.Count);
		for (int i = 0; i < count; i++)
			result.Add(CalcBezierPoint(points, i / (float)count));
	//	result.Add(points.Last());

		return result;
	}

	internal List<BezierPoint> GetBezierPoints()
	{
		var result = new List<BezierPoint>	(TransformPoints.Count);
			result.AddRange	(ToBezierPoints (TransformPoints));
		return result;
	}

	internal BezierPoint CalcBezierPoint(List<BezierPoint> points, float t)
	{
		var tmp = new List<BezierPoint>(points);

		int i = tmp.Count - 1;
		while (i > 0)
		{
			for (int k = 0; k < i; k++)
				tmp[k] = BezierPoint.CalcBezier(tmp[k], tmp[k+1], t);
			i--;
		}

		BezierPoint result = tmp[0];
		return result;
	}

	internal List<BezierPoint> ToBezierPoints(List<Transform> transforms)
	{
		if (transforms.Any(trans => !trans))
			return new List<BezierPoint>();

		var		result = new List<BezierPoint>(transforms.Count);
				result.AddRange(transforms.Select(trans => new BezierPoint {Forward = trans.forward, Normal = trans.up, Point = trans.position}));
		return	result;
	}

	#endregion
//	////////////////////////////////////////////////////////////////////////////
}
