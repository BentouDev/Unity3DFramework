using UnityEngine;
using UnityEditor;
using UnityEditorInternal;

namespace Framework
{
    [CustomEditor(typeof(PrefabRandomizerList))]
    public class PrefabRandomizerListEditor : UnityEditor.Editor
    {
        private bool Folded;

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
                { }
            };
        }

        public override void OnInspectorGUI()
        {
            Folded = EditorGUILayout.Foldout(Folded, "Prefabs");
            if (Folded)
            {
                List.DoLayoutList();
                EditorGUILayout.Space();
            }
        }
    }
}