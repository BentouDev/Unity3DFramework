using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Framework
{
    [CustomEditor(typeof(CharacterMesh))]
    public class CharacterMeshEditor : UnityEditor.Editor
    {
        private ReorderableList SlotList;
    
        private CharacterMesh Target => target as CharacterMesh;

        void InitializeList()
        {
            if (SlotList != null)
                return;
        
            SlotList = new ReorderableList(Target.Slots, typeof(CharacterMesh.SlotInfo));
        
            SlotList.displayAdd    = false;
            SlotList.displayRemove = false;
            SlotList.draggable     = false;
        
            SlotList.onCanRemoveCallback = list => false;
            SlotList.onCanAddCallback    = list => false;

            SlotList.drawHeaderCallback = rect => GUI.Label(rect, "Slots", EditorStyles.label);
            SlotList.drawElementCallback = (rect, index, active, focused) =>
            {
                Rect originalRect = rect;
            
                rect.y      += 2;
                rect.width  *= 0.25f;
                EditorGUI.PrefixLabel(rect, new GUIContent(Target.Slots[index].Name));
            
                rect.height -= 4;
                rect.y      -= 1;
                rect.x      += rect.width;
                rect.width   = originalRect.width * 0.75f;
                var slot = Target.Slots[index];
                slot.Mesh = EditorGUI.ObjectField(rect, slot.Mesh, typeof(GameObject), false) as GameObject;
                Target.Slots[index] = slot;
            };
        }

        public override void OnInspectorGUI()
        {
            InitializeList();
            DrawDefaultInspector();
        
            EditorGUI.BeginChangeCheck();
            SlotList.DoLayoutList();
            if (EditorGUI.EndChangeCheck())
            {
                serializedObject.ApplyModifiedProperties();
            }
        }
    }    
}
