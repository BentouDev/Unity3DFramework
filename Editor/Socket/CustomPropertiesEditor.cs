using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[CustomEditor(typeof(CustomProperties))]
public class CustomPropertiesyEditor : Editor
{
    private ParamListDrawer Drawer = new ParamListDrawer();

    private void Init()
    {
        CustomProperties prop = serializedObject.targetObject as CustomProperties;
        Drawer.Init(prop.Properties);
        Drawer.DrawerList.onChangedCallback -= OnChangedCallback;
        Drawer.DrawerList.onChangedCallback += OnChangedCallback;
    }

    public override void ReloadPreviewInstances()
    {
        Init();
    }

    private void OnChangedCallback(ReorderableList list)
    {
        EditorUtility.SetDirty(serializedObject.targetObject);
        serializedObject.ApplyModifiedProperties();
    }

    public override void OnInspectorGUI()
    {
        if (Drawer.DrawerList == null)
            Init();
        
        DrawDefaultInspector();
        Drawer.DrawerList.DoLayoutList();
    }
}
