using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public static class EditorAreaUtils
{
    private static Matrix4x4 lastMatrix;

    private static MethodInfo GetTopRect;
    private static PropertyInfo topmostRect;

    public static List<Rect> currentRectStack { get; private set; }
    private static List<List<Rect>> rectStackGroups;

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
        rectStackGroups = new List<List<Rect>>();

        // GUIMatrices = new List<Matrix4x4>();
        // adjustedGUILayout = new List<bool>();
    }

    /// <summary>
    /// Begins a field without groups. They should be restored using RestoreClips
    /// </summary>
    public static void BeginNoClip()
    {
        List<Rect> rectStackGroup = new List<Rect>();
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

        List<Rect> rectStackGroup = rectStackGroups[rectStackGroups.Count - 1];
        for (int clipCnt = 0; clipCnt < rectStackGroup.Count; clipCnt++)
        {
            GUI.BeginClip(rectStackGroup[clipCnt]);
            currentRectStack.RemoveAt(currentRectStack.Count - 1);
        }
        rectStackGroups.RemoveAt(rectStackGroups.Count - 1);
    }

    public static void BeginZoomArea(float zoomLevel, Rect viewRect)
    {
        BeginNoClip();

        viewRect.y += 21;
        viewRect.height -= 21;
        
        Rect area = new Rect(viewRect.x, viewRect.y, viewRect.width, viewRect.height);
        
        var pivot = area.min;
        
        area.x -= pivot.x;
        area.y -= pivot.y;

        Rect clippedArea = new Rect(area.x / zoomLevel, area.y / zoomLevel, area.width/ zoomLevel, area.height / zoomLevel);
        
        clippedArea.x += pivot.x;
        clippedArea.y += pivot.y;

        GUI.BeginClip(clippedArea);
        GUI.BeginGroup(new Rect(0, 0, clippedArea.width, clippedArea.height));

        lastMatrix = GUI.matrix;

        Matrix4x4 translation = Matrix4x4.TRS(clippedArea.min, Quaternion.identity, Vector3.one);
        Matrix4x4 scale = Matrix4x4.Scale(new Vector3(zoomLevel, zoomLevel, 1.0f));
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
