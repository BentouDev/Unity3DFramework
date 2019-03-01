using Framework.Editor.Base;
using Framework.Utils;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor.Parameter
{
    /*[CustomPropertyDrawer(typeof(ParamList))]
    public class ParamListEditor : UnityEditor.PropertyDrawer
    {
        protected ParamList Target;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Target = property.GetAs<ParamList>();
            if (Target != null)
            {
                Pair<bool, ReorderableList> list = ReorderableDrawer.GetList(property);
                if (list.First)
                {
                    // Just created, setup first
                    list.Second.drawElementCallback += GenericParameterDrawer;
                    list.Second.onAddDropdownCallback += GenericParameterAdd;                    
                }

                if (list.Second != null)
                {
                    list.Second.DoList(position, label);                    
                }
                else
                {
                    EditorGUI.HelpBox(position, "Unable to render as ParamList!", MessageType.Error);
                }
            }
            else
            {
                EditorGUI.HelpBox(position, "Unable to render as ParamList!", MessageType.Error);
            }
        }
        
        private void GenericParameterAdd(Rect buttonrect, ReorderableList list)
        {
            
        }

        private void GenericParameterDrawer(Rect rect, SerializedProperty element, GUIContent label, bool selected, bool focused)
        {
            var param = element.GetAs<GenericParameter>();
            if (param != null)
            {
                GenericParamUtils.DrawParameter(rect, param);
            }
            else
            {
                GUI.Label(rect, "Unable to render as GenericParameter!");
            }
        }
    }*/
}