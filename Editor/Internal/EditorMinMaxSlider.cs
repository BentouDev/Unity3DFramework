#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;

public static class EditorMinMaxSlider
{
    private static readonly int MinMaxHash = "EditorMinMaxSlider".GetHashCode();

    private static readonly string EditorGUIExtName = "UnityEditor.EditorGUIExt, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
    private static readonly Type EditorGUIExtType = Type.GetType(EditorGUIExtName);
    private static readonly MethodInfo MinMaxDrawMethod = EditorGUIExtType.GetMethod("DoMinMaxSlider", BindingFlags.NonPublic | BindingFlags.Static);

    private static readonly string EditorGUILayoutName = "UnityEditor.EditorGUILayout, UnityEditor, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null";
    private static readonly Type EditorGUILayoutType = Type.GetType(EditorGUILayoutName);
    private static readonly FieldInfo LastRect = EditorGUILayoutType.GetField("s_LastRect", BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.SetField);
    private static readonly MethodInfo SliderRectMethod = EditorGUILayoutType.GetMethod("GetSliderRect", BindingFlags.NonPublic | BindingFlags.Static);

    public static void Layout(ref float minValue, ref float maxValue, float minLimit, float maxLimit, GUIStyle background = null, GUIStyle thumb = null, params GUILayoutOption[] options)
    {
        object[] args = new object[] {false, options};
        var someVal = SliderRectMethod.Invoke(obj: null, parameters: args);
        LastRect.SetValue(obj: null, value: (Rect)someVal);
        Draw((Rect)someVal, ref minValue, ref maxValue, minLimit, maxLimit);
    }

    public static void Draw(Rect position, ref float minValue, ref float maxValue, float minLimit, float maxLimit, GUIStyle background = null, GUIStyle thumb = null)
    {
        if (thumb == null)
            thumb = SpaceEditorStyles.TimeRangeThumb;

        if (background == null)
            background = SpaceEditorStyles.TimeRangeBackground;

        float size = maxValue - minValue;
        EditorGUI.BeginChangeCheck();
        
        var id = GUIUtility.GetControlID(MinMaxHash, FocusType.Native);
        
        object[] args = new object[]
        {
            position, id, minValue, size, minLimit, maxLimit, minLimit, maxLimit,
            background, thumb, true
        };
        
        MinMaxDrawMethod.Invoke(obj: null, parameters: args);

        HandleCursor(position, minValue, size, minLimit, maxLimit, background, thumb);

        minValue = (float)args[2];
        size = (float)args[3];
        
        if (!EditorGUI.EndChangeCheck())
            return;
        maxValue = minValue + size;
    }

    private static void HandleCursor(Rect position, float value, float size, float min, float max, GUIStyle background, GUIStyle thumb)
    {
        Event current = Event.current;

        float min1 = Mathf.Min(min, max);
        float num3 = Mathf.Clamp(value, min1, max);
        float num4 = Mathf.Clamp(value + size, min1, max) - num3;
        float num8 = (double)thumb.fixedWidth == 0.0 ? (float)thumb.padding.horizontal : thumb.fixedWidth;
        float num6 = (float)(((double)position.width - (double)background.padding.horizontal - (double)num8) / ((double)max - (double)min1));
        Rect position1 = new Rect((num3 - min1) * num6 + position.x + (float)background.padding.left, position.y + (float)background.padding.top, num4 * num6 + num8, position.height - (float)background.padding.vertical);
        Rect rect1 = new Rect(position1.x, position1.y, (float)thumb.padding.left, position1.height);
        Rect rect2 = new Rect(position1.xMax - (float)thumb.padding.right, position1.y, (float)thumb.padding.right, position1.height);

        EditorGUIUtility.AddCursorRect(rect1, MouseCursor.ResizeHorizontal);
        EditorGUIUtility.AddCursorRect(rect2, MouseCursor.ResizeHorizontal);
    }
}
#endif