using System.Collections.Generic;
using System.Collections;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    [CustomPropertyDrawer(typeof(InputScheme.ButtonInfo))]
    public class ButtonInfoDrawer : PropertyDrawer
    {
        enum PickState
        {
            None,
            Waiting,
            Got
        }

        private static KeyCode SavedKeyCode;
        private static PickState State;
        private static int CurrentProperty;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var name = property.FindPropertyRelative("Name");
            var key = property.FindPropertyRelative("Key");
            var oldWidth = position.width;

            var nameLabelWidth = 42;
            var keyLabelWidth = 36;
            var nameWidth = oldWidth * 0.35f;
            var keyButtonWidth = 42;
            var keyWidth = oldWidth - nameLabelWidth - keyLabelWidth - nameWidth - keyButtonWidth;
            
            position.width = nameLabelWidth;
            EditorGUI.PrefixLabel(position, new GUIContent("Name"));
            position.x += nameLabelWidth;

            position.width = nameWidth;
            name.stringValue = EditorGUI.TextField(position, name.stringValue);
            position.x += nameWidth;

            if (State == PickState.Waiting)
            {
                if (CurrentProperty == property.CountRemaining())
                {
                    GUI.SetNextControlName("_KeyPicker");
                    EditorGUI.LabelField(position, "Press any key...");
                    GUI.FocusControl("_KeyPicker");
                    return;
                }

                GUI.enabled = false;
            }

            if (State == PickState.Got
            && CurrentProperty == property.CountRemaining())
            {
                key.intValue = (int) SavedKeyCode;
                State = PickState.None;
                CurrentProperty = -1;
            }

            position.width = keyLabelWidth;
            EditorGUI.PrefixLabel(position, new GUIContent("Key"));
            position.x += keyLabelWidth;

            position.width = keyWidth;
            GUI.Box(position, ((KeyCode)key.intValue).ToString(), EditorStyles.helpBox);
            position.x += keyWidth;
            
            position.width = keyButtonWidth;
            if (EditorGUI.DropdownButton(position, new GUIContent("Pick"), FocusType.Keyboard))
            {
                WaitForInput(property);
            }

            GUI.enabled = true;
        }

        void WaitForInput(SerializedProperty property)
        {
            State = PickState.Waiting;
            CurrentProperty = property.CountRemaining();
        }
        
        [InitializeOnLoadMethod]
        static void EditorInit()
        {
            System.Reflection.FieldInfo info = typeof (EditorApplication).GetField ("globalEventHandler", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic);
     
            EditorApplication.CallbackFunction value = (EditorApplication.CallbackFunction)info.GetValue (null);
     
            value += OnGlobalKeyPress;
     
            info.SetValue (null, value);
        }

        static void OnGlobalKeyPress ()
        {
            if (Event.current == null)
                return;

            if (State == PickState.Waiting)
            {
                if (Event.current.rawType == EventType.KeyDown 
                ||  Event.current.rawType == EventType.Used 
                ||  Event.current.rawType == EventType.ExecuteCommand)
                {
                    if (!GUI.GetNameOfFocusedControl().Equals("_KeyPicker") 
                    && !string.IsNullOrWhiteSpace(GUI.GetNameOfFocusedControl()))
                    {
                        State = PickState.None;
                        CurrentProperty = -1;

                        Debug.Log ($"Failed to get key due to '{GUI.GetNameOfFocusedControl()}' having focus!");

                        Event.current.Use();
                        EditorUtility.SetDirty(Object.FindObjectOfType<InputScheme>());
                    }
                    else
                    {
                        State = PickState.Got;
                        SavedKeyCode = Event.current.keyCode; 

                        Debug.Log ("Got key " + Event.current.keyCode + " for prop " + CurrentProperty);

                        Event.current.Use();
                        
                        var target = Object.FindObjectOfType<InputScheme>();
                        if (target)
                            EditorUtility.SetDirty(target);
                    }
                }
            }
        }
    }
}