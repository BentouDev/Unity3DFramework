#if UNITY_EDITOR
using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

namespace Framework.Editor
{
    public abstract class GraphNode
    {
        internal GraphNodeEditor Editor;

        public static readonly int PositionSnap = 5;

        public static Vector2 ConnectorSize = new Vector2(16, 16);
        public static GUIStyle NormalButton;
        public static GUIStyle Connector;
        public static GUIStyle WindowStyle;

        public GUIContent WindowTitle;

        protected Vector2 position;
        protected Vector2 size;
        protected Rect drawRect;

        public List<ConnectionInfo> connectedTo = new List<ConnectionInfo>();

        [System.Serializable]
        public struct ConnectionInfo
        {
            [SerializeField]
            public GraphNode Node;

            [SerializeField]
            public int IndexTo;

            [SerializeField]
            public int IndexFrom;
        }

        public virtual string Name { get; set; }

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
        }

        public void DrawGUI(int id)
        {
            if (Event.current.type == EventType.Layout
            ||  Editor.DrawRect.Contains(drawRect.max)
            ||  Editor.DrawRect.Contains(drawRect.min))
            {
                drawRect.center -= Editor.ScrollPos;
                
                GUI.Box(drawRect, Name, WindowStyle);

                // drawRect = GUI.Window(id, drawRect, DrawContent, WindowTitle, WindowStyle);
                position = drawRect.center + Editor.ScrollPos;
                
                SnapToGrid();
                OnGUI(id);
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

        protected abstract void OnGUI(int id);

        protected virtual void DrawContent(int id)
        {
            OnDrawContent(id);

            if (Event.current.isMouse && Event.current.button == 0)
                GUI.DragWindow();
        }

        protected abstract void OnDrawContent(int id);

        public virtual Vector2 GetMaxCoordinates()
        {
            return drawRect.max + ConnectorSize;
        }

        public virtual Vector2 GetMinCoordinates()
        {
            return drawRect.min - ConnectorSize;
        }

        public virtual Color GetParentConnectColor(GraphNode childNode)
        {
            return Color.white;
        }

        public virtual Vector2 GetParentConnectPosition(GraphNode parent)
        {
            return drawRect.center;
        }

        public virtual Vector2 GetChildConnectPosition(GraphNode child)
        {
            return drawRect.center;
        }

        [System.Obsolete("Deprecated, use GetParentConnectPosition and GetChildConnectPosition")]
        public virtual Vector2 GetConnectPosition(int connectIndex)
        {
            if (connectIndex == 0)
                throw new System.InvalidOperationException();

            var xHalf = ConnectorSize.x * 0.5f;
            var yHalf = ConnectorSize.y * 0.5f;

            return connectIndex > 0
                ? new Vector2
                (
                    drawRect.xMax + xHalf,
                    position.y + yHalf
                )
                : new Vector2
                (
                    drawRect.xMin - xHalf,
                    position.y + yHalf
                );
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

        public virtual void OnConnectToChild(GraphNode node)
        {

        }

        public virtual void OnConnectToParent(GraphNode parent)
        {

        }

        [System.Obsolete("Deprecated, use CanMakeConnection(BaseNode parent, BaseNode child) instead")]
        public static bool CanMakeConnection(GraphNode left, int leftIndex, GraphNode right, int rightIndex)
        {
            return leftIndex != 0 && rightIndex != 0 && left != right && !left.connectedTo.Contains(new ConnectionInfo() { Node = right, IndexTo = rightIndex, IndexFrom = leftIndex });
        }

        [System.Obsolete("Deprecated, use MakeConnection(BaseNode parent, BaseNode child) instead")]
        public static void MakeConnection(GraphNode left, int leftIndex, GraphNode right, int rightIndex)
        {
            left.connectedTo.Add(new ConnectionInfo() { Node = right, IndexTo = rightIndex, IndexFrom = leftIndex });
            left.OnConnectToRight(right, leftIndex, rightIndex);
            right.OnConnectToLeft(left, rightIndex, leftIndex);
        }

        [System.Obsolete("Deprecated, use OnConnectToChild(BaseNode child)")]
        public virtual void OnConnectToRight(GraphNode node, int from, int to)
        {

        }

        [System.Obsolete("Deprecated, use OnConnectToParent(BaseNode parent) instead")]
        public virtual void OnConnectToLeft(GraphNode node, int from, int to)
        {

        }

        /*public virtual bool CanMakeConnection(BaseNode other, int connectIndex)
        {
            return connectIndex != 0 && this != other && !connectedTo.Contains(new ConnectionInfo() {Node = other, IndexTo = connectIndex});
        }*/

        /*public virtual void MakeConnection(BaseNode baseNode, int connectIndex, int currentIndex)
        {
            connectedTo.Add(new ConnectionInfo() { IndexTo = connectIndex, Node = baseNode });
            baseNode.OnBeingConnected(this, currentIndex);
        }*/

        public virtual void RemoveConnection(GraphNode node)
        {
            if (connectedTo.Any(c => c.Node == node))
                connectedTo.RemoveAll(c => c.Node == node);
        }

        public virtual void RemoveConnection(ConnectionInfo baseNode)
        {
            if (connectedTo.Contains(baseNode))
                connectedTo.Remove(baseNode);
        }

        /*public virtual void OnBeingConnected(BaseNode node, int index)
        {

        }*/

        public virtual void OnDelete()
        {

        }
    }
}
#endif