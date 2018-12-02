using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using UnityEngine;

namespace Framework.Editor.MouseModes
{
    class NormalMode : IMouseMode
    {
        GraphNodeEditor Editor;
        
        public NormalMode(GraphNodeEditor editor)
        {
            Editor = editor;
        }
        
        public void Start(Vector2 pos)
        {
            
        }

        public void End(Vector2 pos)
        {
            
        }

        public void Update(Vector2 pos)
        {
            switch (Event.current.type)
            {
                case EventType.ScrollWheel:
                    Editor.ZoomLevel -= Event.current.delta.y * Editor.ZoomSpeed;
                    Event.current.Use();
                    break;
                case EventType.MouseUp:
                    if (Event.current.button == 0)
                    {
                        //Editor.OnLeftClick?.Invoke(Event.current.mousePosition);
                        //Editor.DeselectNodes(Editor.SelectedNodes);
                        Event.current.Use();
                    }
                    else if (Event.current.button == 1)
                    {
                        //Editor.OnRightClick();
                        Editor.OnRightClick.Post().MousePos = Event.current.mousePosition;
                        Event.current.Use();
                    }
                    break;
                case EventType.MouseDown:
                    if (Event.current.button == 2)
                    {
                        Editor.NextMouseMode = new PannMode(Editor);
                        Event.current.Use();
                    }
                    else if (Event.current.clickCount == 2)
                    {
                        //Editor.OnDoubleClick?.Invoke(Event.current.mousePosition);
                        Event.current.Use();
                    }
                    break;
            }
        }
    }

    class PannMode : IMouseMode
    {
        GraphNodeEditor Editor;

        public PannMode(GraphNodeEditor editor)
        {
            Editor = editor;
        }
        
        public void Start(Vector2 pos)
        {
            
        }

        public void End(Vector2 pos)
        {
            
        }

        public void Update(Vector2 pos)
        {
            if (Event.current.button == 2
            &&  Event.current.type == EventType.MouseUp
            || (Event.current.type == EventType.Ignore && Event.current.rawType == EventType.MouseUp))
            {
                Editor.NextMouseMode = new NormalMode(Editor);
                return;
            }
            
            if (Event.current.type != EventType.MouseDrag 
            || (Event.current.type == EventType.Ignore && Event.current.rawType != EventType.MouseDrag))
                return;
            
            Editor.ScrollPos -= Event.current.delta / Editor.ZoomLevel;
            Editor.WantsRepaint = true;
            Event.current.Use();
        }
    }
    
    class DragMode : IMouseMode
    {   
        GraphNodeEditor Editor;

        struct NodeData
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
        Rect            DrawRect;
        Rect            PhysicalRect;
        Vector2         StartPosition;
        GraphNodeEditor Editor;

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
                                 || (Event.current.type == EventType.Ignore 
                                 &&  Event.current.rawType == EventType.MouseDrag));

            // Only when cursor is in editor
            //if (!Editor.PhysicalRect.Contains(pos))
            //    processSelection = false;
            
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
                Editor.WantsRepaint = true;
            }

            // Draw when appropriate
            if (Event.current.type == EventType.Layout 
            ||  Event.current.type == EventType.Repaint)
            {
                // new GUIContent("Count: " + Editor.SelectedNodes.Count)
                GUI.Box(DrawRect, GUIContent.none, SpaceEditorStyles.SelectorRectangle);
            }
        }
    }

    class ConnectMode : IMouseMode
    {
        GraphNodeEditor Editor;
        GraphNode       ConnectionSource;
        Vector2         StartPos;

        public ConnectMode(GraphNodeEditor editor, GraphNode node, Vector2 position)
        {
            Editor = editor;
            StartPos = position;
            ConnectionSource = node;
        }

        public void Start(Vector2 pos)
        {
            
        }

        public void End(Vector2 pos)
        {
            var physicalPos   = (pos * Editor.ZoomLevel) - new Vector2(0,16);
            var data          = Editor.OnConnection.Post();
                data.MousePos = pos;
                data.Source   = ConnectionSource;
                data.Target   = Editor.AllNodes
                    .Where(n => n.IsAcceptingConnection(ConnectionSource, physicalPos))
                    .OrderByDescending(n => n.Position.y)
                    .FirstOrDefault();
        }

        public void Update(Vector2 pos)
        {
            GraphNodeEditor.ConnectionDrawData data = new GraphNodeEditor.ConnectionDrawData
            {
                fromOrigin = ConnectionSource.Position,
                @from = StartPos,
                toOrigin = pos,
                to = pos,
                color = Color.cyan
            };

            Editor.DrawNodeConnectionStyled(data);
            Editor.WantsRepaint = true;
        }
    }
}