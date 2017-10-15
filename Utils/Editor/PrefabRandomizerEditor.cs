using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Framework
{
    [CustomEditor(typeof(PrefabRandomizer))]
    public class PrefabRandomizerEditor : UnityEditor.Editor
    {
        private bool Folded;

        private ReorderableList List;

        private PrefabRandomizer Target => target as PrefabRandomizer;
        
        private const string ToggleLockMenuName = "Prefabs/Lock randomized prefabs";
        
        private void OnEnable()
        {
            PrefabRandomizer.IsGloballyLocked = EditorPrefs.GetBool(ToggleLockMenuName, false);
            ToggleGlobalLock(PrefabRandomizer.IsGloballyLocked);

            List = new ReorderableList(Target.Prefabs, typeof(GameObject))
            {
                drawElementCallback = (rect, index, active, focused) =>
                {
                    rect.y      += 2;
                    rect.height  = 16;
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
        
        [MenuItem(ToggleLockMenuName)]
        public static void MenuToggleGlobalLock()
        {
            ToggleGlobalLock(!PrefabRandomizer.IsGloballyLocked);
        }

        public static void ToggleGlobalLock(bool enabled)
        {
            Menu.SetChecked(ToggleLockMenuName, enabled);
            EditorPrefs.SetBool(ToggleLockMenuName, enabled);

            PrefabRandomizer.IsGloballyLocked = enabled;
        }

        public override void OnInspectorGUI()
        {
            InspectorUtils.DrawDefaultScriptField(serializedObject);

            EditorGUILayout.Space();

            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Randomize"))
                {
                    Undo.RegisterCompleteObjectUndo(Target, "Randomize Prefab");
                    Target.Randomize(true);
                }

                Target.Locked = GUILayout.Toggle(Target.Locked, Target.Locked ? "Unlock Prefab" : "Lock Prefab");
            }
            GUILayout.EndHorizontal();

            Folded = EditorGUILayout.Foldout(Folded, "Prefabs");
            if (Folded)
            {
                List.DoLayoutList();
                EditorGUILayout.Space();
            }
        }
    }
}