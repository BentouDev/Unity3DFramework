using Framework.Utils;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public class Splitter
    {
        private Rect LastRect;     
        private float splitterWidth = 3;
        private float splitterPadding = 2;
        private bool dragging;
        
        public void Layout()
        {
            GUILayout.Box ("", 
                GUILayout.Width(splitterWidth), 
                GUILayout.MaxWidth (splitterWidth), 
                GUILayout.MinWidth(splitterWidth),
                GUILayout.ExpandHeight(true));
            LastRect = GUILayoutUtility.GetLastRect ();
            LastRect.x -= splitterPadding;
            LastRect.width += 2 * splitterPadding;
            EditorGUIUtility.AddCursorRect(LastRect, MouseCursor.ResizeHorizontal);
        }

        public Pair</*repaint*/ bool, /*width*/ float> HandleWidth()
        {
            float result = 0;
            bool repaint = false;
            if (Event.current != null) 
            {
                switch (Event.current.rawType) 
                {
                    case EventType.MouseDown:
                        if (LastRect.Contains (Event.current.mousePosition)) 
                        {
                            dragging = true;
                        }
                        break;
                    case EventType.MouseDrag:
                        if (dragging)
                        {
                            result = Event.current.delta.x;
                            repaint = true;
                        }
                        break;
                    case EventType.MouseUp:
                        if (dragging)
                        {
                            dragging = false;
                        }
                        break;
                }
            }

            return PairUtils.MakePair(repaint, result);
        }
    }
}