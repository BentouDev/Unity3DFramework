using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Editor.MouseModes
{
    class DragMode : IMouseMode
    {   
        private GraphNodeEditor Editor;

        internal struct NodeData
        {
            internal GraphNode Node;
            internal Vector2 Offset;
        }

        private List<NodeData> SelectedNodes = new List<NodeData>();

        public DragMode(GraphNodeEditor editor)
        {
            Editor = editor;
        }

        public void Start(Vector2 pos)
        {
            foreach (GraphNode node in Editor.SelectedNodes)
            {
                SelectedNodes.Add(new NodeData()
                {
                    Node = node,
                    Offset = node.Position - pos
                });
            }
        }

        public void End(Vector2 pos)
        {
            
        }

        public void Update(Vector2 pos)
        {
            if (Event.current.type != EventType.MouseDrag 
            || (Event.current.type == EventType.Ignore && Event.current.rawType != EventType.MouseDrag))
                return;

            if (!Editor.PhysicalRect.Contains(pos))
                return;

            GUI.color = Color.yellow;
            foreach (var data in SelectedNodes)
            {
                data.Node.Position = pos + data.Offset;
            
                GUI.Box(data.Node.PhysicalRect, GUIContent.none);
            }
            GUI.color = Color.white;

            Event.current.Use();
        }
    }

    class SelectMode : IMouseMode
    {
        public Rect DrawRect;
        public Rect PhysicalRect;
        public Vector2 StartPosition;
        private GraphNodeEditor Editor;

        public SelectMode(GraphNodeEditor editor)
        {
            Editor = editor;
        }

        public void Start(Vector2 pos)
        {
            StartPosition = pos;
        }

        public void End(Vector2 pos)
        {
         
        }

        public void Update(Vector2 pos)
        {
            // Only for dragging
            bool processSelection = (Event.current.type == EventType.MouseDrag
                                 || (Event.current.type == EventType.Ignore &&
                                     Event.current.rawType == EventType.MouseDrag));

            // Only when cursor is in editor
            if (!Editor.PhysicalRect.Contains(pos))
                processSelection = false;
            
            if (processSelection)
            {                
                DrawRect.min = new Vector2(Mathf.Min(StartPosition.x, pos.x), Mathf.Min(StartPosition.y, pos.y));
                DrawRect.max = new Vector2(Mathf.Max(StartPosition.x, pos.x), Mathf.Max(StartPosition.y, pos.y));

                // Physical is scalled as physical of node, so we can overlap them properly
                PhysicalRect.position = DrawRect.position * Editor.ZoomLevel;
                PhysicalRect.size     = DrawRect.size * Editor.ZoomLevel;
                
                var nodes = Editor.AllNodes.Where(node => PhysicalRect.Overlaps(node.PhysicalRect));
                
                var graphNodes = nodes as IList<GraphNode> ?? nodes.ToList();
                Editor.SelectOnly(graphNodes);
            }

            // Draw when appropriate
            if (Event.current.type == EventType.Layout 
            ||  Event.current.type == EventType.Repaint)
            {
                GUI.Box(DrawRect, new GUIContent("Count: " + Editor.SelectedNodes.Count), SpaceEditorStyles.SelectorRectangle);
            }
        }
    }
}