#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

namespace Framework.Editor
{
    public abstract class GraphNode
    {
        internal GraphNodeEditor Editor;

        public static readonly double DoubleClickTime = 0.25f;
        public static readonly int PositionSnap = 5;

        public static Vector2 ConnectorSize = new Vector2(16, 16);
        public static GUIStyle NormalButton;
        public static GUIStyle Connector;
        public static GUIStyle WindowStyle;

        public GUIContent WindowTitle;

        protected Vector2 position;
        protected Vector2 size;
        protected Rect drawRect;
        private double LastLabelClickTime;
        private string IntermediateName;

        public List<ConnectionInfo> connectedTo = new List<ConnectionInfo>();
        
        [System.Serializable]
        public struct ConnectionInfo : IEquatable<ConnectionInfo>
        {
            [SerializeField]
            public GraphNode Node;
            
            public bool Equals(ConnectionInfo other)
            {
                return Equals(Node, other.Node);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                return obj is ConnectionInfo && Equals((ConnectionInfo) obj);
            }

            public override int GetHashCode()
            {
                return (Node != null ? Node.GetHashCode() : 0);
            }
        }

        public string UniqueName { get; internal set; }
        
        public int Id { get; internal set; }

        public virtual string Name { get; set; }

        public bool Selected { get; private set; }

        public bool IsBeingRenamed { get; private set; }

        public Rect DrawRect => drawRect;

        public Rect BoundsRect { get; private set; }

        public Rect PhysicalRect
        {
            get
            {
                Rect rect = new Rect(DrawRect);
                rect.position += Editor.PannedOffset;

                // Since we operate in drawing coordinates we have to scale down manually
                rect.position *= Editor.ZoomLevel;
                rect.size     *= Editor.ZoomLevel;

                return rect;
            }
        }
        
        public Vector2 Size
        {
            get { return size; }
            set
            {
                size = value;
                RecalculateDrawRect();
            }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                position = value;
                RecalculateDrawRect();
            }
        }
        
        protected virtual void RecalculateDrawRect()
        {
            drawRect.Set
            (
                position.x - size.x * 0.5f,
                position.y - size.y * 0.5f,
                size.x,
                size.y
            );

            BoundsRect = drawRect;
        }
        
        public void SetSelected(bool selected)
        {
            OnSelected(selected);
            Selected = selected;
        }

        protected virtual void OnSelected(bool value)
        { }

        public void DrawGUI()
        {
            if (Event.current.type == EventType.Layout
            || Editor.BoundsRect.Overlaps(BoundsRect))
            {
                drawRect.center += Editor.PannedOffset;

                var savedRect = drawRect;
                {
                    OnGUI();
                    DrawContent();
                }
                drawRect = savedRect;

                GUI.color = Color.white;

                PostGUI();
                SnapToGrid();
            }
        }
        
        protected void SnapToGrid()
        {
            position.Set
            (
                position.x - (position.x % PositionSnap),
                position.y - (position.y % PositionSnap)
            );

            RecalculateDrawRect();
        }

        protected virtual void OnRenamed(string newName)
        {
            
        }

        protected void HandleLabel(Rect rect, string text, GUIStyle style, GUIStyle editStyle = null)
        {
            if (editStyle == null)
                editStyle = EditorStyles.textField;

            if (Event.current.type == EventType.MouseDown)
            {
                if (rect.Contains(Event.current.mousePosition))
                {
                    if (EditorApplication.timeSinceStartup - LastLabelClickTime < DoubleClickTime)
                    {
                        IsBeingRenamed = true;
                        IntermediateName = text;

                        Event.current.Use();
                    }
                    
                    LastLabelClickTime = EditorApplication.timeSinceStartup;
                }
                else if (IsBeingRenamed)
                {
                    IsBeingRenamed = false;
                    OnRenamed(IntermediateName);
                }
            }

            if (IsBeingRenamed)
            {
                GUI.SetNextControlName("_NodeRenameField");
                IntermediateName = GUI.TextField(rect, IntermediateName, editStyle);

                GUI.FocusControl("_NodeRenameField");
            }
            else
            {
                GUI.Label(rect, text, style);
            }
        }

        protected virtual void OnGUI()
        {
            if (Selected)
                GUI.color = SpaceEditorStyles.ActiveColor;
            GUI.Box(drawRect, GUIContent.none, WindowStyle);
                
            GUI.color = Color.white;

            HandleLabel(drawRect, UniqueName, EditorStyles.whiteLargeLabel);
        }

        protected virtual void DrawContent()
        {
            OnDrawContent();
        }

        protected virtual void PostGUI()
        {
            
        }

        protected abstract void OnDrawContent();
        
        public virtual Vector2 GetMaxCoordinates()
        {
            return drawRect.max + ConnectorSize;
        }

        public virtual Vector2 GetMinCoordinates()
        {
            return drawRect.min - ConnectorSize;
        }

        public virtual Color GetParentConnectColor(GraphNode parent)
        {
            return parent != null && parent.Selected ? SpaceEditorStyles.ActiveColor : Color.white;
        }

        public virtual Vector2 GetParentConnectPosition(GraphNode parent)
        {
            return drawRect.center;
        }

        public virtual void GetChildConnectPositions(GraphNode child, IList<Vector2> positions)
        {
            positions.Add(drawRect.center);
        }

        protected virtual bool CanConnectTo(GraphNode child)
        {
            return !connectedTo.Contains(new ConnectionInfo()
            {
                Node = child
            });
        }

        public virtual bool IsAcceptingConnection(GraphNode parent, Vector2 physicalPos)
        {
            return CanMakeConnection(parent, this) && PhysicalRect.Contains(physicalPos);
        }

        public static bool CanMakeConnection(GraphNode parent, GraphNode child)
        {
            if (parent == null || child == null)
                return false;

            return parent.CanConnectTo(child);
        }

        public static void MakeConnection(GraphNode parent, GraphNode child)
        {
            parent.connectedTo.Add(new ConnectionInfo()
            {
                Node = child
            });

            parent.OnConnectToChild(child);
            child.OnConnectToParent(parent);
        }

        protected virtual void OnConnectToChild(GraphNode node)
        {

        }

        protected virtual void OnConnectToParent(GraphNode parent)
        {

        }

        public void RemoveConnection(GraphNode node)
        {
            if (connectedTo.Any(c => c.Node == node))
                connectedTo.RemoveAll(c => c.Node == node);
            
            OnConnectionRemoved(node);
            node.OnParentDisconnected(this);
        }

        public void RemoveConnection(ConnectionInfo connection)
        {
            if (connectedTo.Contains(connection))
                connectedTo.Remove(connection);
            
            OnConnectionRemoved(connection.Node);
            connection.Node.OnParentDisconnected(this);
        }

        protected virtual void OnConnectionRemoved(GraphNode node)
        {
            
        }

        protected virtual void OnParentDisconnected(GraphNode node)
        {
            
        }

        public virtual void OnDelete()
        {

        }

        public void HandleRightClick()
        {
            OnRightClick();
        }

        protected virtual void OnRightClick()
        { }
    }
}
#endif