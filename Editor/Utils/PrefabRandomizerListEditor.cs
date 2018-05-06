using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Framework
{
    [CustomEditor(typeof(PrefabRandomizerList))]
    public class PrefabRandomizerListEditor : UnityEditor.Editor
    {
        private PrefabRandomizerList Target => target as PrefabRandomizerList;
        private ReorderableList List;

        private void OnEnable()
        {
            List = new ReorderableList(Target.Prefabs, typeof(GameObject))
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    rect.y += 2;
                    rect.height = 16;
                    var result = EditorGUI.ObjectField(rect, Target.Prefabs[index], typeof(GameObject), false) as GameObject;
                    if (Target.Prefabs[index] != result)
                    {
                        Undo.RecordObject(Target, "Changed Prefab");
                        Target.Prefabs[index] = result;

                        EditorUtility.SetDirty(Target);
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();
                    }
                },
                onAddCallback = list =>
                {
                    Undo.RecordObject(Target, "Added Prefab");
                    Target.Prefabs.Add(null);
                },
                onRemoveCallback = list =>
                {
                    Undo.RecordObject(Target, "Removed Prefab");
                    Target.Prefabs.RemoveAt(list.index);
                },
                drawHeaderCallback = rect =>
                { },
                onChangedCallback = list =>
                {
                    EditorUtility.SetDirty(Target);
                    AssetDatabase.SaveAssets();
                    AssetDatabase.Refresh();
                }
            };
        }

        public override void OnInspectorGUI()
        {
            var prefabs = serializedObject.FindProperty("Prefabs");
            prefabs.isExpanded = EditorGUILayout.Foldout(prefabs.isExpanded, "Prefabs");
            if (prefabs.isExpanded)
            {
                List.DoLayoutList();
                EditorGUILayout.Space();
            }
        }
    }
}