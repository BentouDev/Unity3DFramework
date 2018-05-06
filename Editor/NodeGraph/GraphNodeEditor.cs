#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Framework.Editor.MouseModes;
using UnityEditor;

namespace Framework.Editor
{
    public class GraphNodeEditor
    {
        public List<GraphNode> SelectedNodes = new List<GraphNode>();

        public IMouseMode CurrentMouseMode { get; private set; }
        public IMouseMode NextMouseMode { get; set; }

        private float zoom = 1;
        private Vector2 scrollPos = Vector2.zero;
        public Rect DrawRect;

        public float ZoomSpeed = 0.05f;

        public List<GraphNode> AllNodes { get; private set; }

        public class NodeEvent
        {
            public GraphNode Node;
        }

        public class MouseEvent
        {
            public Vector2 MousePos;
        }

        public class NodeMouseEvent
        {
            public GraphNode Node; 
            public Vector2 MousePos;
        }
        
        public class NodeConnectionEvent
        { 
            public GraphNode Source;
            public GraphNode Target; 
        }

        public readonly EventQueue<NodeEvent>  OnDeleteNode   = new EventQueue<NodeEvent>();
        public readonly EventQueue<NodeEvent>  OnSelectNode   = new EventQueue<NodeEvent>();
        public readonly EventQueue<NodeEvent>  OnDeselectNode = new EventQueue<NodeEvent>();
        public readonly EventQueue<MouseEvent> OnRightClick   = new EventQueue<MouseEvent>();
        public readonly EventQueue<MouseEvent> OnDoubleClick  = new EventQueue<MouseEvent>();
        
        public readonly EventQueue<NodeConnectionEvent> OnConnection = new EventQueue<NodeConnectionEvent>();
       
        private bool CtrlPressed;
        private bool SelectionOverride;

        public Vector2 ScrollPos
        {
            get { return scrollPos; }
            set { scrollPos = value; }
        }

        public float ZoomLevel
        {
            get { return zoom; }
            set { zoom = Mathf.Clamp(value, 0.25f, 1); }
        }
        
        public Rect PhysicalRect => new Rect(0, 0, DrawRect.width / ZoomLevel, DrawRect.height / ZoomLevel);
        public Rect BoundsRect => new Rect(-PannedOffset.x, -PannedOffset.y, DrawRect.width / ZoomLevel, DrawRect.height / ZoomLevel);

        public Vector2 PannedOffset => (DrawRect.size * 0.5f) / ZoomLevel - ScrollPos;

        private bool _wantsRepaint;
        public bool WantsRepaint
        {
            get { return _wantsRepaint; }
            set { if (value) _wantsRepaint = true; }
        }

        public enum ConnectionStyle
        {
            Line,
            BezierHorizontal,
            BezierVertical
        }
        
        public ConnectionStyle ConnectionLineStyle { get; private set; }
        
        public GraphNodeEditor()
        {
            AllNodes = new List<GraphNode>();
            OnDeleteNode.Reassign(data =>
            {
                foreach (GraphNode baseNode in AllNodes)
                {
                    baseNode.RemoveConnection(data.Node);
                }

                AllNodes.Remove(data.Node);

                data.Node.OnDelete();
                
                return true;
            });

            OnSelectNode.Reassign(data =>
            {
                data.Node.SetSelected(true);
                SelectedNodes.Add(data.Node);
                return true;
            });

            OnDeselectNode.Reassign(data =>
            {
                data.Node.SetSelected(false);
                SelectedNodes.Remove(data.Node);
                return true;
            });
            
            OnConnection.Reassign(data =>
            {
                if (data.Target != null)
                {
                    GraphNode.MakeConnection(data.Source, data.Target);
                    return true;
                }

                return false;
            });
        }

        public void ClearNodes()
        {
            CtrlPressed = false;

            CurrentMouseMode?.End(Event.current?.mousePosition ?? Vector2.zero);
            CurrentMouseMode = null;
        
            OnDeleteNode.Clear();
            OnSelectNode.Clear();
            OnDeleteNode.Clear();
            
            SelectedNodes.Clear();
            AllNodes.Clear();
        }
        
        public void DeleteNode(GraphNode node)
        {
            if (AllNodes.Contains(node))
                OnDeleteNode.Post().Node = node;
        }

        public void AddNode(GraphNode node, bool onScrolledPosition = false)
        {
            if (onScrolledPosition)
                node.Position += scrollPos;
            
            node.Editor     = this;
            node.Id         = AllNodes.Count;
            node.UniqueName = $"{node.Id}::{node.Name}";
            
            AllNodes.Add(node);
        }

        public Rect GetCoordinateBound()
        {
            var min = GetMinCoordinates();
            return new Rect(min, GetMaxCoordinates() - min);
        }

        public Vector2 GetMinCoordinates()
        {
            Vector2 min = new Vector2(Single.MaxValue, Single.MaxValue);

            foreach (GraphNode node in AllNodes)
            {
                var minPos = node.GetMinCoordinates();
                if (minPos.x < min.x)
                    min.x = minPos.x;
                if (minPos.y < min.y)
                    min.y = minPos.y;
            }

            return min;
        }

        public Vector2 GetMaxCoordinates()
        {
            Vector2 max = Vector2.zero;

            foreach (GraphNode node in AllNodes)
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
            EditorWindow editor, Rect viewRect, 
            ConnectionStyle style = ConnectionStyle.BezierVertical
        )
        {
            DrawRect = viewRect;
            ConnectionLineStyle = style;
            
            EditorAreaUtils.BeginZoomArea(ZoomLevel, viewRect);
            {
                DrawWindows(editor);
                DrawConnections(style);
            }
            EditorAreaUtils.EndZoomArea();
        }
        
        void DrawConnections(ConnectionStyle style)
        {
            switch (style)
            {
                case ConnectionStyle.Line:
                    foreach (GraphNode parent in AllNodes)
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
                    foreach (GraphNode parent in AllNodes)
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
                    foreach (GraphNode parent in AllNodes)
                    {
                        foreach (GraphNode.ConnectionInfo child in parent.connectedTo)
                        {
                            DrawNodeConnectionBezierVertical (
                                parent.GetChildConnectPosition(child.Node) + PannedOffset,
                                child.Node.GetParentConnectPosition(child.Node) + PannedOffset,
                                child.Node.GetParentConnectColor(parent)
                            );
                        }
                    }
                    break;
            }

            /*
            
            bool goodEvent = Event.current.type == EventType.Repaint || Event.current.type == EventType.MouseMove || Event.current.type == EventType.MouseDrag;

            if (goodEvent && IsConnecting && SelectedNode != null)
            {
                switch (style)
                {
                    case ConnectionStyle.Line:
                        DrawNodeConnectionLine(CurrentConnectionStart, Event.current.mousePosition, Color.cyan);
                        break;
                    case ConnectionStyle.BezierHorizontal:
                        DrawNodeConnectionBezierHorizontal(CurrentConnectionStart, Event.current.mousePosition, Color.cyan);
                        break;
                    case ConnectionStyle.BezierVertical:
                        DrawNodeConnectionBezierVertical(CurrentConnectionStart, Event.current.mousePosition, Color.cyan);
                        break;
                }

                if(Event.current.type != EventType.Repaint)
                    Event.current.Use();
                
                GUI.color = Color.white;
            } 
            
             */
        }

        public void StartConnection(GraphNode node, Vector2 position)
        {
            NextMouseMode = new ConnectMode(this, node, position);
            
//            IsConnecting = true;
//            CurrentConnectionStart = position;
            // SelectedNode = node;
        }

        public void DropConnection(GraphNode node = null, Vector2 position = default(Vector2))
        {
            /*if (!IsConnecting)
                return;
            
            if (node == null)
            {
                if (OnConnectionEmptyDrop != null)
                    OnConnectionEmptyDrop(SelectedNode, position);
            }
            else
            {
                if (OnConnectionNodeDrop != null)
                    OnConnectionNodeDrop(SelectedNode, node);
            }

            IsConnecting = false;
            SelectedNode = null;*/
        }

        private void DrawPos(Vector2 pos)
        {
            var zoomCoeff = 1 / ZoomLevel;
            GUI.Box(new Rect(pos.x - 1, 0, 2 * zoomCoeff, DrawRect.height * zoomCoeff), GUIContent.none);
            GUI.Box(new Rect(0, pos.y - 1, DrawRect.width * zoomCoeff, 2 * zoomCoeff), GUIContent.none);
        }

        void DrawWindows(EditorWindow editor)
        {
            foreach (var node in AllNodes.OrderBy(n => n.Position.y))
            {
                node.DrawGUI();
            }
            
            if (NextMouseMode != null)
            {
                CurrentMouseMode?.End(Event.current.mousePosition);
                CurrentMouseMode = NextMouseMode;
                NextMouseMode = null;
                CurrentMouseMode?.Start(Event.current.mousePosition);
            }
            
            if (CurrentMouseMode == null)
                NextMouseMode = new NormalMode(this);
            
            if (CurrentMouseMode != null)
            {
                CurrentMouseMode.Update(Event.current.mousePosition);
            }
    
//            Rect test = new Rect(DrawRect);
//            test.center += PannedOffset;
//            GUI.color = Color.red;
//            GUI.Box(test, GUIContent.none);
//            GUI.color = Color.blue;
//            Rect koza = new Rect(PhysicalRect);
//            GUI.Box(koza, GUIContent.none);
//            GUI.color = Color.white;
        
            //if (Event.current.type == EventType.MouseMove 
            //|| (Event.current.type == EventType.Ignore && Event.current.rawType == EventType.MouseMove))
            {
                //if (DrawRect.Contains(Event.current.mousePosition))
                    
//                DrawPos(Event.current.mousePosition);
//                
//                WantsRepaint = true;
            }
        }

        public void SelectOnly(params GraphNode[] nodes)
        {
            SelectOnly(nodes as IEnumerable<GraphNode>);
        }

        public void SelectOnly(IEnumerable<GraphNode> nodes)
        {
            SelectionOverride = true;

            OnSelectNode.Clear();
            OnDeselectNode.Clear();

            foreach (GraphNode node in SelectedNodes)
            {
                node.SetSelected(false);
            }

            SelectedNodes.Clear();
            SelectedNodes.AddRange(nodes);

            foreach (var node in SelectedNodes)
            {
                node.SetSelected(true);
            }
        }

        public void SelectNodes(params GraphNode[] nodes)
        {
            SelectNodes(nodes as IEnumerable<GraphNode>);
        }

        public void SelectNodes(IEnumerable<GraphNode> nodes)
        {
            foreach (GraphNode node in nodes)
            {
                OnSelectNode.Post().Node = node;
            }
        }

        public void DeselectNodes(params GraphNode[] nodes)
        {
            DeselectNodes(nodes as IEnumerable<GraphNode>);
        }

        public void DeselectNodes(IEnumerable<GraphNode> nodes)
        {
            foreach (GraphNode node in nodes)
            {
                OnDeselectNode.Post().Node = node;
            }
        }

        private void ProcessSelection()
        {
            if (SelectionOverride)
            {
                OnSelectNode.Clear();
                OnDeselectNode.Clear();
                SelectionOverride = false;
                return;
            }

            OnDeselectNode.Process();
            OnSelectNode.Process();
        }

        private bool HandleMouseDown()
        {
            bool left  = Event.current.button == 0;
            bool right = Event.current.button == 1;

            if (!left && !right)
                return false;

            if (PhysicalRect.Contains(Event.current.mousePosition))
            {
                var newNodes = new List<GraphNode>();
                newNodes.AddRange(AllNodes
                    .Where(node => node.PhysicalRect.Contains(Event.current.mousePosition - new Vector2(0,16)))
                    .OrderByDescending(node => node.Id));

                if (!newNodes.Any())
                {
                    DeselectNodes(SelectedNodes);
                    GUI.FocusControl(string.Empty);
                }
                else
                {
                    if (!SelectedNodes.Contains(newNodes.First()))
                    {
                        if (!CtrlPressed)
                            DeselectNodes(SelectedNodes);
 
                        SelectNodes(newNodes.First());
                    }

                    if (left)
                        NextMouseMode = new DragMode(this);
                    if (right)
                        newNodes.First().HandleRightClick();

                    Event.current.Use();
                    return true;
                }

                if (CurrentMouseMode is NormalMode && left)
                {
                    NextMouseMode = new SelectMode(this);

                    Event.current.Use();
                    return true;   
                }
            }

            return false;
        }

        private bool HandleMouseMode()
        {
            var type = Event.current.type;
            if (type == EventType.Ignore && CurrentMouseMode != null)
                type = Event.current.rawType;

            switch (type)
            {
                case EventType.MouseUp:
                    if (Event.current.button == 0 && CurrentMouseMode != null)
                    {
                        CurrentMouseMode.End(Event.current.mousePosition);
                        CurrentMouseMode = null;
                        Event.current.Use();
                        return true;
                    }
                    break;
                case EventType.MouseDown:
                    if (HandleMouseDown())
                        return true;
                    break;
            }

            return false;
        }

        public void HandleEvents(EditorWindow editor)
        {
            if (Event.current.type == EventType.Repaint)
                _wantsRepaint = false;

            if (Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.LeftControl)
                CtrlPressed = true;
            if (Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.LeftControl)
                CtrlPressed = false;

            HandleMouseMode();
            ProcessSelection();
            
            OnRightClick.Process();
            OnConnection.Process();
            OnDeleteNode.Process();
        }

        GenericMenu.MenuFunction CreateRemoveConnectionCallback(GraphNode source, GraphNode.ConnectionInfo toRemove)
        {
            return () => source.RemoveConnection(toRemove);
        }

//        void HandleConnectionMenu(GraphNode action, int index)
//        {
//            GenericMenu menu = new GenericMenu();
//
//            if (index > 0)
//            {
//                int i = 0;
//                foreach (GraphNode.ConnectionInfo connection in action.connectedTo.Where(c => c.IndexFrom == index))
//                {
//                    menu.AddItem(new GUIContent(++i + ") Remove connection to '" + connection.Node.Name + "' ..."), false, CreateRemoveConnectionCallback(action, connection));
//                }
//            }
//
//            menu.ShowAsContext();
//        }

        /*[Obsolete("Deprecated, use StartConnection(BaseNode node)")]
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
                        SelectedNode = actionNode;
                    }
                    else if ((int)Mathf.Sign(connectIndex) != (int)Mathf.Sign(result))
                    {
                        if (connectIndex > 0 && GraphNode.CanMakeConnection(SelectedNode, connectIndex, actionNode, result))
                        {
                            GraphNode.MakeConnection(SelectedNode, connectIndex, actionNode, result);
                        }
                        else if (connectIndex < 0 && GraphNode.CanMakeConnection(actionNode, result, SelectedNode, connectIndex))
                        {
                            GraphNode.MakeConnection(actionNode, result, SelectedNode, connectIndex);
                        }

                        connectIndex = 0;
                        IsConnecting = false;
                        SelectedNode = null;
                    }
                    break;
                case 1:
                    HandleConnectionMenu(actionNode, result);
                    break;
                default:
                    break;
            }
        }*/

        public void DrawNodeConnectionStyled(Vector2 from, Vector2 to, Color color)
        {
            switch (ConnectionLineStyle)
            {
                case ConnectionStyle.Line:
                    DrawNodeConnectionLine(from, to, color);
                    break;
                
                case ConnectionStyle.BezierVertical:
                    DrawNodeConnectionBezierVertical(from, to, color);
                    break;
                    
                case ConnectionStyle.BezierHorizontal:
                    DrawNodeConnectionBezierHorizontal(from, to, color);
                    break;
            }
        }

        public static void DrawNodeConnectionLine(Vector2 from, Vector2 to, Color color)
        {
            Handles.BeginGUI();
            Handles.color = new Color(0.2f, 0.2f, 0.2f, 0.06f) * Color.black;

            for (int i = 0; i < 3; i++)
            {
                Handles.DrawAAPolyLine((i + 1) * 5, @from, to);
            }

            Handles.color = color;
            Handles.DrawAAPolyLine(@from, to);
            Handles.EndGUI();
        }

        public static void DrawNodeConnectionBezierHorizontal(Vector2 from, Vector2 to, Color color)
        {
            Handles.BeginGUI();
            Color shadowCol = new Color(0.2f, 0.2f, 0.2f, 0.06f) * Color.black;

            Vector3 startPos = new Vector3(@from.x, @from.y, 0.0f);
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

            Vector3 startPos = new Vector3(@from.x, @from.y, 0.0f);
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