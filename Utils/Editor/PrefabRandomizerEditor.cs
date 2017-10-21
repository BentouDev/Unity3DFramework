using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEditorInternal;

namespace Framework
{
    [CustomEditor(typeof(PrefabRandomizer))]
    [CanEditMultipleObjects]
    public class PrefabRandomizerEditor : UnityEditor.Editor
    {
        private PrefabRandomizer Target => target as PrefabRandomizer;
        
        private const string ToggleLockMenuName = "Prefabs/Lock randomized prefabs";
        private const string RandomizeAllMenuName = "Prefabs/Randomize prefabs";
        private const string RandomizeReqursivelyenuName = "GameObject/Prefabs/Randomize Reqursively";
        
        private void OnEnable()
        {
            PrefabRandomizer.IsGloballyLocked = EditorPrefs.GetBool(ToggleLockMenuName, false);
            ToggleGlobalLock(PrefabRandomizer.IsGloballyLocked);
        }

        [MenuItem(RandomizeAllMenuName)]
        public static void MenuRandomizeAll()
        {
            foreach (var prefab in FindObjectsOfType<PrefabRandomizer>())
            {
                prefab.Randomize();
            }
        }
        
        [MenuItem(RandomizeReqursivelyenuName, false, 0)]
        public static void RandomizeRecursively()
        {
            foreach (PrefabRandomizer randomizer in Selection.gameObjects[0].GetComponentsInChildren<PrefabRandomizer>())
            {
                randomizer.Randomize();
            }
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

            EditorGUI.BeginChangeCheck();
            
            var list = EditorGUILayout.ObjectField(Target.Prefabs, typeof(PrefabRandomizerList), true, GUILayout.ExpandWidth(true)) as PrefabRandomizerList;
            if (EditorGUI.EndChangeCheck())
            {
                foreach (PrefabRandomizer rand in targets.Select(r => r as PrefabRandomizer))
                {
                    if (rand) rand.Prefabs = list;
                }
            }
            
            GUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Randomize"))
                {
                    if (targets.Length == 0 || targets.Length == 1)
                    {
                        Undo.RegisterCompleteObjectUndo(Target, "Randomize Prefab");
                        Target.Randomize(true);
                    }
                    else
                    {
                        foreach (PrefabRandomizer obj in targets.Select(a => a as PrefabRandomizer))
                        {
                            obj?.Randomize(true);
                        }
                    }
                }

                Target.Locked = GUILayout.Toggle(Target.Locked, Target.Locked ? "Unlock Prefab" : "Lock Prefab");
            }
            GUILayout.EndHorizontal();

            EditorGUILayout.Space();
        }
    }
}