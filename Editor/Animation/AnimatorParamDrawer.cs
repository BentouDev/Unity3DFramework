using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Framework.Editor.Animation
{
    [CustomPropertyDrawer(typeof(AnimatorParam))]
    public class AnimatorParamDrawer : PropertyDrawer
    {
        public AnimatorParam Param => attribute as AnimatorParam;
        
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (Param == null)
                return;

            bool specificAnimator = !string.IsNullOrEmpty(Param.AnimatorName);
            Animator anim = null;
            var comp = property.serializedObject.targetObject as Component;
            if (comp)
            {
                if (specificAnimator)
                {
                    anim = comp.gameObject.GetComponentsInChildren<Animator>()
                        .FirstOrDefault(c => c.gameObject.name == Param.AnimatorName);                
                }
                else
                {
                    anim = comp.gameObject.GetComponentInChildren<Animator>();
                }
            }

            if (anim == null)
            {
                if (specificAnimator)
                    EditorGUI.HelpBox(position, $"No animator named '{Param.AnimatorName}' found!", MessageType.Error);
                else
                    EditorGUI.HelpBox(position, $"No animator found!", MessageType.Error);
                return;
            }

            IEnumerable<AnimatorControllerParameter> otherParams = anim.parameters.Where(p => p.type == Param.ParamType);
            string[] ParamNames = otherParams.Select(p => p.name).ToArray();

            EditorGUI.BeginChangeCheck();
            int index = EditorGUI.Popup(position, label.text, Array.IndexOf<string>(ParamNames, property.stringValue), ParamNames);

            property.stringValue = index >= 0 && index < ParamNames.Length ? ParamNames[index] : string.Empty;
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
    }
}