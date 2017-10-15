using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace Framework
{
    [CustomEditor(typeof(DialogInstance))]
    public class DialogInstanceEditor : UnityEditor.Editor
    {
        DialogInstance Dialog => target as DialogInstance;

        public override void OnInspectorGUI()
        {
            InspectorUtils.DrawDefaultScriptField(serializedObject);

            serializedObject.Update();
            {
                HandleField(ref Dialog.Dialog, "Dialog Script");

                InspectorUtils.DrawFoldableProperty(serializedObject.FindProperty("OnDialogStart"));
                InspectorUtils.DrawFoldableProperty(serializedObject.FindProperty("OnDialogEnd"));

                var actors = serializedObject.FindProperty("Actors");
                if (actors.arraySize != 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Actors", EditorStyles.boldLabel);
                    for (int i = 0; i < actors.arraySize; i++)
                    {
                        EditorGUILayout.PropertyField(actors.GetArrayElementAtIndex(i));
                    }
                }

                var functions = serializedObject.FindProperty("Functions");
                if (functions.arraySize != 0)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Functions", EditorStyles.boldLabel);
                    for (int i = 0; i < functions.arraySize; i++)
                    {
                        var prop = functions.GetArrayElementAtIndex(i);
                        InspectorUtils.DrawFoldableProperty(prop, prop.FindPropertyRelative("Name").stringValue);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("Reload"))
            {
                Dialog.ReloadDialog();
            }
        }

        private void HandleField<T>(ref T oldObj, string name) where T : UnityEngine.Object
        {
            var newObj = EditorGUILayout.ObjectField(new GUIContent(name), oldObj, typeof(Dialog), false) as T;
            if (!Equals(oldObj, newObj))
            {
                Undo.RecordObject(Dialog, "Changed " + name);
                oldObj = newObj;
                Undo.FlushUndoRecordObjects();
            }
        }

        private void HandleArrayElement<T>(ref List<T> arrayObj, string name, int index) where T : UnityEngine.Object
        {
            var newObj = EditorGUILayout.ObjectField(new GUIContent(name), arrayObj[index], typeof(Dialog), false) as T;
            if (newObj != arrayObj[index])
            {
                Undo.RecordObject(Dialog, "Changed " + name);
                arrayObj[index] = newObj;
                Undo.FlushUndoRecordObjects();
            }
        }
    }
}
