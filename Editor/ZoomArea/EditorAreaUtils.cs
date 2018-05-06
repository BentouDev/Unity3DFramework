using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorAreaUtils
{
    public class GUIColor : IDisposable
    {
        private Color _old;
        public GUIColor(Color color)
        {
            _old = GUI.color;
            GUI.color = color;
        }
        
        public void Dispose()
        {
            GUI.color = _old;
        }
    }
    
    private static Matrix4x4 lastMatrix;

    private static MethodInfo GetTopRect;
    private static PropertyInfo topmostRect;

    public static List<Rect> currentRectStack { get; private set; }
    private static List<List<Rect>> rectStackGroups;

    private static List<Rect> rectStackGroup;

    public static Rect TopRect
    {
        get
        {
            return (Rect)GetTopRect.Invoke(null, null);
        }
    }

    public static Rect TopRectScreenSpace
    {
        get
        {
            return (Rect)topmostRect.GetValue(null, null);
        }
    }

    static EditorAreaUtils()
    {
        Assembly UnityEngine = Assembly.GetAssembly(typeof(UnityEngine.GUI));

        System.Type GUIClipType = UnityEngine.GetType("UnityEngine.GUIClip");

        topmostRect = GUIClipType.GetProperty("topmostRect");
        GetTopRect = GUIClipType.GetMethod("GetTopRect", BindingFlags.Static | BindingFlags.NonPublic);

        // As we can call Begin/Ends inside another, we need to save their states hierarchial in Lists:
        currentRectStack = new List<Rect>();
        rectStackGroup   = new List<Rect>();
        rectStackGroups  = new List<List<Rect>>();

        // GUIMatrices = new List<Matrix4x4>();
        // adjustedGUILayout = new List<bool>();
    }

    /// <summary>
    /// Begins a field without groups. They should be restored using RestoreClips
    /// </summary>
    public static void BeginNoClip()
    {
        rectStackGroup.Clear();
        Rect topMostClip = TopRect;

        while (topMostClip != new Rect(-10000, -10000, 40000, 40000))
        {
            rectStackGroup.Add(topMostClip);
            GUI.EndClip();
            topMostClip = TopRect;
        }

        rectStackGroup.Reverse();
        rectStackGroups.Add(rectStackGroup);
        currentRectStack.AddRange(rectStackGroup);
    }

    /// <summary>
    ///  Restores the clips removed in BeginNoClip or MoveClipsUp
    /// </summary>
    public static void RestoreClips()
    {
        if (rectStackGroups.Count == 0)
        {
            Debug.LogError("GUIClipHierarchy: BeginNoClip/MoveClipsUp - RestoreClips count not balanced!");
            return;
        }

        rectStackGroup = rectStackGroups[rectStackGroups.Count - 1];
        for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
        {
            GUI.BeginClip(rectStackGroup[clipCnt]);
            currentRectStack.RemoveAt(currentRectStack.Count - 1);
        }
        rectStackGroups.RemoveAt(rectStackGroups.Count - 1);
    }

    private static Rect AreaRect;
    private static Rect ClippedAreaRect;
    private static Rect GroupRect;
    private static Vector3 Scale;

    public static void BeginZoomArea(float zoomLevel, Rect viewRect)
    {
//        GUI.Label(new Rect(20, 20, 200, 30), "TL " + ClippedAreaRect.TopLeft());
//        GUI.Label(new Rect(viewRect.xMax - 220, 20, 200, 30), "TR " + ClippedAreaRect.max);
//        GUI.Label(new Rect(20, viewRect.yMax - 50, 200, 30), "BL " + ClippedAreaRect.min);
//        GUI.Label(new Rect(viewRect.xMax - 220, viewRect.yMax - 50, 200, 30), "BR " + new Vector2(ClippedAreaRect.xMax, ClippedAreaRect.yMin));

        BeginNoClip();

        //viewRect.y += 21;
        //viewRect.height -= 21;
        
        AreaRect.Set(viewRect.x, viewRect.y, viewRect.width, viewRect.height);

        //GUI.color = Color.red;
        //GUI.Box(AreaRect, GUIContent.none);
        GUI.color = Color.white;

        var pivot = AreaRect.min;

        AreaRect.x -= pivot.x;
        AreaRect.y -= pivot.y;

        // This expands clip region to match zoomed out space
        ClippedAreaRect.Set(AreaRect.x / zoomLevel, AreaRect.y / zoomLevel, AreaRect.width / zoomLevel, AreaRect.height / zoomLevel);

        ClippedAreaRect.x += pivot.x;
        ClippedAreaRect.y += pivot.y;
        
        GroupRect.Set(0, 0, ClippedAreaRect.width, ClippedAreaRect.height);
        
        GUI.BeginClip(ClippedAreaRect);
        GUI.BeginGroup(GroupRect);

        lastMatrix = GUI.matrix;

        Scale.Set(zoomLevel, zoomLevel, 1.0f);

        Matrix4x4 translation = Matrix4x4.TRS(ClippedAreaRect.min, Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(Scale);
        GUI.matrix = translation * scale * translation.inverse * GUI.matrix;
    }

    public static void EndZoomArea()
    {
        GUI.EndGroup();
        GUI.EndClip();
        GUI.matrix = lastMatrix;
        RestoreClips();
    }
}
