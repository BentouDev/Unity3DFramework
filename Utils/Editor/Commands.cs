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

            var go = new GameObject(Selection.activeTransform.name + " Group");
                go.transform.position = posSum / Selection.transforms.Length;

            Undo.RegisterCreatedObjectUndo(go, "Group Selected");
            
            foreach (var transform in Selection.transforms)
                Undo.SetTransformParent(transform, go.transform, "Group Selected");
            
            Selection.activeGameObject = go;
        }
    }
}