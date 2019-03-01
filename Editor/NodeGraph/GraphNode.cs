#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using Framework.Utils;
using UnityEditor;

namespace Framework.Editor
{
    public enum SlotType
    {
        Input,
        Output
    }
    
    [Serializable]
    public class Slot : IEquatable<Slot>
    {
        public readonly GraphNode Owner;
        public readonly SlotType Type;
        public GraphNode ConnectedNode;

        public Slot(GraphNode owner, SlotType type)
        {
            Owner = owner;
            Type = type;
        }

        public bool Equals(Slot other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return false; // No duplicated instances of slots!
            return Owner.Equals(other.Owner) && Type == other.Type;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Slot) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Owner.GetHashCode() * 397) ^ (int) Type;
            }
        }
    }
    
    [Serializable]
    public class Connection : IEquatable<Connection>
    {
        public Slot From;
        public Slot Target;

        public bool Equals(Connection other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Equals(From, other.From) && Equals(Target, other.Target);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Connection) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((From != null ? From.GetHashCode() : 0) * 397) ^ (Target != null ? Target.GetHashCode() : 0);
            }
        }
    }

    public abstract class GraphNode
    {
        protected static bool IsRecreatingNodes = false;
        public class NodeRecreationContext : IDisposable
        {
            public NodeRecreationContext()
            {
                IsRecreatingNodes = true;
            }
            
            public void Dispose()
            {
                IsRecreatingNodes = false;
            }
        }
        
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

        public List<Connection> connectedTo = new List<Connection>();
        public List<Slot> Outputs = new List<Slot>();
        public List<Slot> Inputs = new List<Slot>();

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
                position.x, //- size.x * 0.5f,
                position.y, // - size.y * 0.5f,
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

                DrawSlots();
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

        public virtual void SetupConnection(IEnumerable<ActionGraphNode> graphNodes)
        {
            
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

        public virtual Vector2 GetSlotPosition(Slot slot)
        {
            if (slot.Type == SlotType.Input)
            {
                return new Vector2(0, Size.y) + new Vector2(0, Inputs.IndexOf(slot) * ConnectorSize.y);
            }
            else
            {
                return Size + new Vector2(0, Outputs.IndexOf(slot) * ConnectorSize.y);
            }
        }
        
        public Rect GetDrawSlotRect(Slot slot)
        {
            var position = GetSlotPosition(slot) + drawRect.position;
            return new Rect
            (
                position.x - ConnectorSize.x * 0.5f, 
                position.y - ConnectorSize.y * 0.5f, 
                ConnectorSize.x, 
                ConnectorSize.y
            );
        }

        public Rect GetPhysicalSlotRect(Slot slot)
        {
            var rect = GetDrawSlotRect(slot);
            rect.position += Editor.PannedOffset;

            // Since we operate in drawing coordinates we have to scale down manually
            rect.position *= Editor.ZoomLevel;
            rect.size     *= Editor.ZoomLevel;

            return rect;
        }

        protected virtual void DrawSlot(Slot slot)
        {
            var ChildDotRect = GetDrawSlotRect(slot);
            GUI.Box
            (
                ChildDotRect,
                GUIContent.none,
                SpaceEditorStyles.DotFlowTarget
            );

            if (slot.ConnectedNode != null)
            {
                if (Selected)
                    GUI.color = SpaceEditorStyles.ActiveColor;

                GUI.Box
                (
                    ChildDotRect,
                    GUIContent.none,
                    SpaceEditorStyles.DotFlowTargetFill
                );
                GUI.color = Color.white;
            }
        }

        private void DrawSlots()
        {
            foreach (var input in Inputs)
            {
                DrawSlot(input);
            }

            foreach (var output in Outputs)
            {
                DrawSlot(output);
            }
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

        [Obsolete]
        public virtual Color GetParentConnectColor(GraphNode parent)
        {
            return parent != null && parent.Selected ? SpaceEditorStyles.ActiveColor : Color.white;
        }

        [Obsolete]
        public virtual Vector2 GetParentConnectPosition(GraphNode parent)
        {
            return drawRect.center;
        }

        [Obsolete]
        public virtual void GetChildConnectPositions(GraphNode child, IList<Vector2> positions)
        {
            positions.Add(drawRect.center);
        }

        protected virtual bool CanConnectTo(Slot targetSlot, Slot childSlot)
        {
            if (targetSlot == null || childSlot == null)
                return false;
            
            if (targetSlot.Owner == childSlot.Owner)
                return false;

            return !connectedTo.Any(c => c.From.Equals(targetSlot) && c.Target == null);
        }

        public virtual Pair<bool, Slot> IsAcceptingConnection(Slot parent, Vector2 physicalPos)
        {
            var slot = GetDefaultInputSlot();
            if (slot != null)
            {
                var rect = GetPhysicalSlotRect(slot);
                return new Pair<bool, Slot>(CanMakeConnection(parent, slot) 
                                            && rect.Contains(physicalPos), slot);                
            }
            else
            {
                return new Pair<bool, Slot>(false, null);
            }
        }
//
//        public static bool CanMakeConnection(GraphNode parent, GraphNode child)
//        {
//            if (parent == null || child == null)
//                return false;
//
//            return parent.CanConnectTo(child);
//        }
//
//        public static void MakeConnection(GraphNode parent, GraphNode child)
//        {
//            parent.connectedTo.Add(new Connection()
//            {
//                Child = child
//            });
//
//            parent.OnConnectToChild(child);
//            child.OnConnectToParent(parent);
//        }
//
//        protected virtual void OnConnectToChild(GraphNode node)
//        {
//
//        }
//
//        protected virtual void OnConnectToParent(GraphNode parent)
//        {
//
//        }
//
//        [Obsolete("Do not use this, provide Connection instead!")]
//        public void RemoveConnection(GraphNode node)
//        {
//            throw new Exception("Attempt to remove connection by node itself, which is no longer supported!");
//            if (connectedTo.Any(c => c.Node == node))
//                connectedTo.RemoveAll(c => c.Node == node);
//            
//            OnConnectionRemoved(node);
//            node.OnParentDisconnected(this);
        //}

        public void RemoveAllConnectionFrom(GraphNode node)
        {
            var toDisconnect = new List<Connection>();
            foreach (var connection in connectedTo)
            {
                if (connection.Target.Owner == node)
                    toDisconnect.Add(connection);
            }

            foreach (var connection in toDisconnect)
            {
                RemoveConnection(connection);
            }
        }

        public void RemoveConnection(Connection connection)
        {
            if (!connectedTo.Contains(connection))
                return;

            OnChildDisconnected(connection);
            connection.Target.Owner.OnParentDisconnected(connection);
            connection.From.ConnectedNode = null;
            connection.Target.ConnectedNode = null;
        }

        protected virtual void OnChildDisconnected(Connection connection)
        {
            connectedTo.Remove(connection);
        }

        protected virtual void OnParentDisconnected(Connection node)
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

        protected internal virtual Slot GetDefaultInputSlot()
        {
            return null;
        }

        /// <summary>
        /// Tries to create connection on default input
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="child"></param>
        public static Connection ConnectNodes(Slot parent, GraphNode child)
        {
            var defaultInput = child.GetDefaultInputSlot();
            if (defaultInput != null)
            {
                return MakeConnection(parent, defaultInput);
            }

            return null;
        }

        public static bool CanMakeConnection(Slot eventSource, Slot eventTarget)
        {
            return eventSource.Owner.CanConnectTo(eventSource, eventTarget);
        }

        public static Connection MakeConnection(Slot eventSource, Slot eventTarget)
        {
            var connection = new Connection()
            {
                From = eventSource, 
                Target = eventTarget
            };

            if (IsRecreatingNodes)
            {
                connection.From.ConnectedNode = connection.Target.Owner;
                connection.Target.ConnectedNode = connection.From.Owner;                
            }

            return connection;
        }

        public static void FinishConnection(Connection connection)
        {
            connection.From.Owner.OnConnectToChild(connection);
            connection.Target.Owner.OnConnectToParent(connection);

            connection.From.ConnectedNode = connection.Target.Owner;
            connection.Target.ConnectedNode = connection.From.Owner;
        }

        protected virtual void OnConnectToParent(Connection eventSource)
        {
            
        }

        protected virtual void OnConnectToChild(Connection eventTarget)
        {
            connectedTo.Add(eventTarget);
        }

        public virtual void OnDestroy()
        {
            
        }

        public void GetConnectionDrawData(Connection connection, out GraphNodeEditor.ConnectionDrawInfo info)
        {
            info.color = Selected ? SpaceEditorStyles.ActiveColor : Color.white;
            info.@from = GetDrawSlotRect(connection.From).center;
            info.@to = connection.Target.Owner.GetDrawSlotRect(connection.Target).center;
        }
    }
}
#endif