#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public abstract class BaseNode
{
    public static BaseNode selected = null;
    public static BaseNode toDelete = null;

    public static Vector2 ConnectorSize = new Vector2(10, 10);
    public static GUIStyle NormalButton;
    public static GUIStyle Connector;

    protected Vector2 position;
    protected Vector2 size;
    protected Rect drawRect;

    public List<ConnectionInfo> connectedTo = new List<ConnectionInfo>();
    
    [System.Serializable]
    public struct ConnectionInfo
    {
        [SerializeField]
        public BaseNode Node;

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
        drawRect = new Rect(
            position.x - size.x*0.5f,
            position.y - size.y*0.5f,
            size.x,
            size.y
        );
    }

    public abstract int OnGUI(int id);

    public virtual Vector2 GetMaxCoordinates()
    {
        return drawRect.max + ConnectorSize;
    }

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

    public static bool CanMakeConnection(BaseNode left, int leftIndex, BaseNode right, int rightIndex)
    {
        return leftIndex != 0 && rightIndex != 0 && left != right && !left.connectedTo.Contains(new ConnectionInfo() {Node = right, IndexTo = rightIndex, IndexFrom = leftIndex});
    }

    public static void MakeConnection(BaseNode left, int leftIndex, BaseNode right, int rightIndex)
    {
        left.connectedTo.Add(new ConnectionInfo() {Node = right, IndexTo = rightIndex, IndexFrom = leftIndex});
        left.OnConnectToRight(right, leftIndex, rightIndex);
        right.OnConnectToLeft(left, rightIndex, leftIndex);
    }

    public virtual void OnConnectToRight(BaseNode node, int from, int to)
    {
        
    }

    public virtual void OnConnectToLeft(BaseNode node, int from, int to)
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

    public virtual void RemoveConnection(BaseNode node)
    {
        if (connectedTo.Any(c => c.Node == node))
            connectedTo.RemoveAll(c => c.Node == node);
    }

    public virtual void RemoveConnection(ConnectionInfo baseNode)
    {
        if(connectedTo.Contains(baseNode))
            connectedTo.Remove(baseNode);
    }

    /*public virtual void OnBeingConnected(BaseNode node, int index)
    {
        
    }*/

    public virtual void OnDelete()
    {
        
    }
}
#endif