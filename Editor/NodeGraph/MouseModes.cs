using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.Editor.MouseModes
{
    class DragMode : IMouseMode
    {
        public void Start(Vector2 pos)
        {
            
        }

        public void End(Vector2 pos)
        {
            
        }

        public void Update(Vector2 pos)
        {
            
        }
    }

    class SelectMode : IMouseMode
    {
        public Rect DrawRect;
        public Vector2 StartPosition;

        public void Start(Vector2 pos)
        {
            StartPosition = pos;
        }

        public void End(Vector2 pos)
        {
         
        }

        public void Update(Vector2 pos)
        {
            DrawRect.min = StartPosition;
            DrawRect.max = pos;
            GUI.Box(DrawRect, GUIContent.none, SpaceEditorStyles.SelectorRectangle);
        }
    }
}