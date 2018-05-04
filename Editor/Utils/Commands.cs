using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    public static class Commands
    {
        [MenuItem("GameObject/Group Selected %g", true)]
        static bool ValidateGroupSelected()
        {
            return Selection.activeTransform != null;
        }

        [MenuItem("GameObject/Group Selected %g")]
        private static void GroupSelected()
        {
            if (!Selection.activeTransform)
                return;

            Vector3 posSum = Vector3.zero;
            foreach (var transform in Selection.transforms)
                posSum += transform.position;

            // Center them
            var go = new GameObject(Selection.activeTransform.name + " Group");
                go.transform.position = posSum / Selection.transforms.Length;

            // Handle GUI
            if (Selection.transforms.All(x => x.GetComponentInParent<Canvas>()))
                go.AddComponent<RectTransform>();

            Undo.RegisterCreatedObjectUndo(go, "Group Selected");

            Transform goParent = Selection.transforms.First().parent;
            if (Selection.transforms.Any(x => x.parent != goParent))
                goParent = null;

            foreach (var transform in Selection.transforms)
                Undo.SetTransformParent(transform, go.transform, "Group Selected");
            
            // If they are come from single parent, nest group there as well
            if (goParent)
                Undo.SetTransformParent(go.transform, goParent.transform, "Set group parent");
            
            Selection.activeGameObject = go;
        }
    }
}