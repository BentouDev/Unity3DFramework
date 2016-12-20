#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Framework.EditorUtils
{
    public class GraphNodeEditor
    {
        private Vector2 scrollPos = Vector2.zero;

        private int connectIndex;

        private Vector2 CurrentConnectionStart;

        public bool IsConnecting { get; private set; }
        public bool IsDragging { get; private set; }

        public List<GraphNode> nodes { get; private set; }

        public delegate void GraphEditorEvent();
        public delegate void GraphEditorMouseEvent(Vector2 mousePosition);
        public delegate void GraphEditorNodeEvent(GraphNode node, Vector2 mousePosition);
        public delegate void GraphEditorNodeConnectionEvent(GraphNode first, GraphNode second);

        public event GraphEditorNodeEvent OnConnectionEmptyDrop;
        public event GraphEditorNodeConnectionEvent OnConnectionNodeDrop;

        public event GraphEditorMouseEvent OnDoubleClick;
        public event GraphEditorMouseEvent OnRightClick;
        public event GraphEditorMouseEvent OnLeftClick;
        public event GraphEditorEvent OnDelete;

        public enum ConnectionStyle
        {
            Line,
            BezierHorizontal,
            BezierVertical
        }

        public GraphNodeEditor()
        {
            nodes = new List<GraphNode>();
        }

        public void ClearNodes()
        {
            nodes.Clear();
        }

        public void RemoveNode(GraphNode node)
        {
            if (nodes.Contains(node))
                nodes.Remove(node);
        }

        public void AddNode(GraphNode node, bool onScrolledPosition = false)
        {
            if (onScrolledPosition)
                node.Position += scrollPos;
            nodes.Add(node);
        }

        public Vector2 GetMaxCoordinates()
        {
            Vector2 max = Vector2.zero;

            foreach (GraphNode node in nodes)
            {
                var maxPos = node.GetMaxCoordinates();
                if (maxPos.x > max.x)
                    max.x = maxPos.x;
                if (maxPos.y > max.y)
                    max.y = maxPos.y;
            }

            return max;
        }

        public void Draw (
            EditorWindow editor, Rect position, Rect viewRect, 
            ConnectionStyle style = ConnectionStyle.BezierVertical
        )
        {
            scrollPos = GUI.BeginScrollView(position, scrollPos, viewRect);
            {
                DrawWindows(editor);
                DrawConnections(style);
            }
            GUI.EndScrollView();
        }
        
        void DrawConnections(ConnectionStyle style)
        {
            switch (style)
            {
                case ConnectionStyle.Line:
                    foreach (GraphNode parent in nodes)
                    {
                        foreach (GraphNode.ConnectionInfo child in parent.connectedTo)
                        {
                            DrawNodeConnectionLine (
                                parent.GetChildConnectPosition(child.Node), 
                                child.Node.GetParentConnectPosition(parent),
                                child.Node.GetParentConnectColor(parent)
                            );
                        }
                    }
                    break;

                case ConnectionStyle.BezierHorizontal:
                    foreach (GraphNode parent in nodes)
                    {
                        foreach (GraphNode.ConnectionInfo child in parent.connectedTo)
                        {
                            DrawNodeConnectionBezierHorizontal (
                                parent.GetChildConnectPosition(child.Node), 
                                child.Node.GetParentConnectPosition(child.Node),
                                child.Node.GetParentConnectColor(parent)
                            );
                        }
                    }    
                    break;
                case ConnectionStyle.BezierVertical:
                    foreach (GraphNode parent in nodes)
                    {
                        foreach (GraphNode.ConnectionInfo child in parent.connectedTo)
                        {
                            DrawNodeConnectionBezierVertical (
                                parent.GetChildConnectPosition(child.Node),
                                child.Node.GetParentConnectPosition(child.Node),
                                child.Node.GetParentConnectColor(parent)
                            );
                        }
                    }
                    break;
            }

            bool goodEvent = Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag;

            if (goodEvent && IsConnecting && GraphNode.selected != null)
            {
                switch (style)
                {
                    case ConnectionStyle.Line:
                        DrawNodeConnectionLine(CurrentConnectionStart - scrollPos, Event.current.mousePosition, Color.cyan);
                        break;
                    case ConnectionStyle.BezierHorizontal:
                        DrawNodeConnectionBezierHorizontal(CurrentConnectionStart - scrollPos, Event.current.mousePosition, Color.cyan);
                        break;
                    case ConnectionStyle.BezierVertical:
                        DrawNodeConnectionBezierVertical(CurrentConnectionStart - scrollPos, Event.current.mousePosition, Color.cyan);
                        break;
                }

                if(Event.current.type != EventType.Repaint)
                    Event.current.Use();
                
                GUI.color = Color.white;
            }
        }

        public void StartConnection(GraphNode node)
        {
            StartConnection(node, Event.current.mousePosition);
        }

        public void StartConnection(GraphNode node, Vector2 position)
        {
            IsConnecting = true;
            CurrentConnectionStart = position;
            GraphNode.selected = node;
        }

        public void DropConnection(GraphNode node = null, Vector2 position = default(Vector2))
        {
            if (!IsConnecting)
                return;
            
            if (node == null)
            {
                if (OnConnectionEmptyDrop != null)
                    OnConnectionEmptyDrop(GraphNode.selected, position);
            }
            else
            {
                if (OnConnectionNodeDrop != null)
                    OnConnectionNodeDrop(GraphNode.selected, node);
            }

            IsConnecting = false;
            GraphNode.selected = null;
        }

        void DrawWindows(EditorWindow editor)
        {
            editor.BeginWindows();
            {
                for (int i = 0; i < nodes.Count; i++)
                {
                    nodes[i].OnGUI(i);
                }
            }
            editor.EndWindows();
        }

        public void HandleDelete()
        {
            Event.current.Use();

            if (OnDelete != null)
                OnDelete();

            foreach (GraphNode baseNode in nodes)
            {
                baseNode.RemoveConnection(GraphNode.toDelete);
            }

            nodes.Remove(GraphNode.toDelete);

            GraphNode.toDelete.OnDelete();
            GraphNode.toDelete = null;
        }

        public void HandleEvents()
        {
            switch (Event.current.type)
            {
                case EventType.mouseUp:
                    if (Event.current.button == 0)
                    {
                        if (IsConnecting)
                        {
                            DropConnection(null, Event.current.mousePosition);
                        }
                        else
                        {
                            if (OnLeftClick != null)
                                OnLeftClick(Event.current.mousePosition);
                        }

                        Event.current.Use();
                    }
                    else if (Event.current.button == 1)
                    {
                        if (OnRightClick != null)
                            OnRightClick(Event.current.mousePosition);

                        Event.current.Use();
                    }
                    else if (Event.current.button == 2 && IsDragging)
                    {
                        IsDragging = false;
                        Event.current.Use();
                    }
                    /*else
                    {
                        connectIndex = 0;
                        IsConnecting = false;
                        GraphNode.selected = null;
                        Event.current.Use();
                    }*/
                    break;
                case EventType.MouseDrag:
                    if (IsDragging)
                    {
                        scrollPos -= Event.current.delta;
                        Event.current.Use();
                    }
                    break;
                case EventType.mouseDown:
                    if (Event.current.button == 2)
                    {
                        IsDragging = true;
                        Event.current.Use();
                    }
                    /*else if (Event.current.clickCount == 1)
                    {
                        connectIndex = 0;
                        IsConnecting = false;
                        GraphNode.selected = null;
                        Event.current.Use();
                    }*/
                    else if (Event.current.clickCount == 2)
                    {
                        if (OnDoubleClick != null)
                            OnDoubleClick(Event.current.mousePosition);

                        Event.current.Use();
                    }
                    break;
            }

            if (GraphNode.toDelete != null)
            {
                HandleDelete();
            }
        }

        UnityEditor.GenericMenu.MenuFunction CreateRemoveConnectionCallback(GraphNode source, GraphNode.ConnectionInfo toRemove)
        {
            return () => source.RemoveConnection(toRemove);
        }

        void HandleConnectionMenu(GraphNode action, int index)
        {
            GenericMenu menu = new GenericMenu();

            if (index > 0)
            {
                int i = 0;
                foreach (GraphNode.ConnectionInfo connection in action.connectedTo.Where(c => c.IndexFrom == index))
                {
                    menu.AddItem(new GUIContent(++i + ") Remove connection to '" + connection.Node.Name + "' ..."), false, CreateRemoveConnectionCallback(action, connection));
                }
            }

            menu.ShowAsContext();
        }

        [System.Obsolete("Deprecated, use StartConnection(BaseNode node)")]
        void HandleConnection(GraphNode actionNode, int result)
        {
            if (result == 0)
                return;

            switch (Event.current.button)
            {
                case 0:
                    if (!IsConnecting)
                    {
                        connectIndex = result;
                        IsConnecting = true;
                        GraphNode.selected = actionNode;
                    }
                    else if ((int)Mathf.Sign(connectIndex) != (int)Mathf.Sign(result))
                    {
                        if (connectIndex > 0 && GraphNode.CanMakeConnection(GraphNode.selected, connectIndex, actionNode, result))
                        {
                            GraphNode.MakeConnection(GraphNode.selected, connectIndex, actionNode, result);
                        }
                        else if (connectIndex < 0 && GraphNode.CanMakeConnection(actionNode, result, GraphNode.selected, connectIndex))
                        {
                            GraphNode.MakeConnection(actionNode, result, GraphNode.selected, connectIndex);
                        }

                        connectIndex = 0;
                        IsConnecting = false;
                        GraphNode.selected = null;
                    }
                    break;
                case 1:
                    HandleConnectionMenu(actionNode, result);
                    break;
                default:
                    break;
            }
        }

        public static void DrawNodeConnectionLine(Vector2 from, Vector2 to, Color color)
        {
            Handles.BeginGUI();
            Handles.color = new Color(0.2f, 0.2f, 0.2f, 0.06f) * Color.black;

            for (int i = 0; i < 3; i++)
            {
                Handles.DrawAAPolyLine((i + 1) * 5, from, to);
            }

            Handles.color = color;
            Handles.DrawAAPolyLine(from, to);
            Handles.EndGUI();
        }

        public static void DrawNodeConnectionBezierHorizontal(Vector2 from, Vector2 to, Color color)
        {
            Handles.BeginGUI();
            Color shadowCol = new Color(0.2f, 0.2f, 0.2f, 0.06f) * Color.black;

            Vector3 startPos = new Vector3(from.x, from.y, 0.0f);
            Vector3 endPos = new Vector3(to.x, to.y, 0.0f);

            Vector3 startTan = startPos + Vector3.right * 50;
            Vector3 endTan = endPos + Vector3.left * 50;

            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 2.0f);
            Handles.EndGUI();
        }

        public static void DrawNodeConnectionBezierVertical(Vector2 from, Vector2 to, Color color)
        {
            Handles.BeginGUI();
            Color shadowCol = new Color(0.2f, 0.2f, 0.2f, 0.06f) * Color.black;

            Vector3 startPos = new Vector3(from.x, from.y, 0.0f);
            Vector3 endPos = new Vector3(to.x, to.y, 0.0f);

            Vector3 startTan = startPos + Vector3.up * 50;
            Vector3 endTan = endPos + Vector3.down * 50;

            for (int i = 0; i < 3; i++)
            {
                Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 7);
            }

            Handles.DrawBezier(startPos, endPos, startTan, endTan, color, null, 3.0f);
            Handles.EndGUI();
        }
    }
}

#endif