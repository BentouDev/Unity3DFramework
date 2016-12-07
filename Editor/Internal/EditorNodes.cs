#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public class EditorNodes
{
    private Vector2 scrollPos = Vector2.zero;

    private int connectIndex;

    private bool isConnecting;
    private bool isDragging;

    public List<BaseNode> nodes { get; private set; }

    public delegate void NodeEditorEvent();

    public event NodeEditorEvent OnDoubleClick;
    public event NodeEditorEvent OnDelete;

    public EditorNodes()
    {
        nodes = new List<BaseNode>();
    }

    public void ClearNodes()
    {
        nodes.Clear();
    }

    public void RemoveNode(BaseNode node)
    {
        if(nodes.Contains(node))
            nodes.Remove(node);
    }

    public void AddNode(BaseNode node, bool onScrolledPosition = false)
    {
        if(onScrolledPosition)
            node.Position += scrollPos;
        nodes.Add(node);
    }

    public Vector2 GetMaxCoordinates()
    {
        Vector2 max = Vector2.zero;

        foreach (BaseNode node in nodes)
        {
            var maxPos = node.GetMaxCoordinates();
            if (maxPos.x > max.x)
                max.x = maxPos.x;
            if (maxPos.y > max.y)
                max.y = maxPos.y;
        }

        return max;
    }

    public void Draw(EditorWindow editor, Rect position, Rect viewRect)
    {
        scrollPos = GUI.BeginScrollView(position, scrollPos, viewRect);
        {
            DrawConnections();
            DrawWindows(editor);
        }
        GUI.EndScrollView();
    }

    void DrawConnections()
    {
        foreach (BaseNode node in nodes)
        {
            foreach (BaseNode.ConnectionInfo otherNode in node.connectedTo)
            {
                DrawNodeConnection(node.GetConnectPosition(otherNode.IndexFrom), otherNode.Node.GetConnectPosition(otherNode.IndexTo));
            }
        }
    }

    void DrawWindows(EditorWindow editor)
    {
        editor.BeginWindows();
        {
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                var result = node.OnGUI(i);
                if (result != 0)
                {
                    HandleConnection(node, result);
                }
            }
        }
        editor.EndWindows();
    }

    public void HandleDelete()
    {
        Event.current.Use();

        if (OnDelete != null)
            OnDelete();

        foreach (BaseNode baseNode in nodes)
        {
            baseNode.RemoveConnection(BaseNode.toDelete);
        }

        nodes.Remove(BaseNode.toDelete);

        BaseNode.toDelete.OnDelete();
        BaseNode.toDelete = null;
    }

    public void HandleEvents()
    {
        switch (Event.current.type)
        {
            case EventType.mouseUp:
                if (Event.current.button == 2 && isDragging)
                {
                    isDragging = false;
                    Event.current.Use();
                }
                else
                {
                    connectIndex = 0;
                    isConnecting = false;
                    BaseNode.selected = null;
                    Event.current.Use();
                }
                break;
            case EventType.MouseDrag:
                if (isDragging)
                {
                    scrollPos -= Event.current.delta;
                    Event.current.Use();
                }
                break;
            case EventType.mouseDown:
                if (Event.current.button == 2)
                {
                    isDragging = true;
                    Event.current.Use();
                }
                else if (Event.current.clickCount == 1)
                {
                    connectIndex = 0;
                    isConnecting = false;
                    BaseNode.selected = null;
                    Event.current.Use();
                }
                else if (Event.current.clickCount == 2)
                {
                    if (OnDoubleClick != null)
                        OnDoubleClick();
                    Event.current.Use();
                }
                break;
            case EventType.repaint:
                if (isConnecting && BaseNode.selected != null)
                {
                    GUI.color = Color.cyan;
                    DrawNodeConnection(BaseNode.selected.GetConnectPosition(connectIndex) - scrollPos, Event.current.mousePosition);
                    GUI.color = Color.white;
                    Event.current.Use();
                }
                break;
        }

        if (BaseNode.toDelete != null)
        {
            HandleDelete();
        }
    }

    UnityEditor.GenericMenu.MenuFunction CreateRemoveConnectionCallback(BaseNode source, BaseNode.ConnectionInfo toRemove)
    {
        return () => source.RemoveConnection(toRemove);
    }

    void HandleConnectionMenu(BaseNode action, int index)
    {
        GenericMenu menu = new GenericMenu();

        if(index > 0)
        {
            int i = 0;
            foreach (BaseNode.ConnectionInfo connection in action.connectedTo.Where(c => c.IndexFrom == index))
            {
                menu.AddItem(new GUIContent(++i + ") Remove connection to '" + connection.Node.Name + "' ..."), false, CreateRemoveConnectionCallback(action, connection));
            }
        }

        menu.ShowAsContext();
    }

    void HandleConnection(BaseNode actionNode, int result)
    {
        if (result == 0)
            return;

        switch (Event.current.button)
        {
            case 0:
                if (!isConnecting)
                {
                    connectIndex = result;
                    isConnecting = true;
                    BaseNode.selected = actionNode;
                }
                else if ((int)Mathf.Sign(connectIndex) != (int)Mathf.Sign(result))
                {
                    if (connectIndex > 0 && BaseNode.CanMakeConnection(BaseNode.selected, connectIndex, actionNode, result))
                    {
                        BaseNode.MakeConnection(BaseNode.selected, connectIndex, actionNode, result);
                    }
                    else if (connectIndex < 0 && BaseNode.CanMakeConnection(actionNode, result, BaseNode.selected, connectIndex))
                    {
                        BaseNode.MakeConnection(actionNode, result, BaseNode.selected, connectIndex);
                    }

                    connectIndex = 0;
                    isConnecting = false;
                    BaseNode.selected = null;
                }
                break;
            case 1:
                HandleConnectionMenu(actionNode, result);
                break;
            default:
                break;
        }
    }

    public static void DrawNodeConnection(Vector2 from, Vector2 to)
    {
        Color shadowCol = new Color(0.2f, 0.2f, 0.2f, 0.06f) * GUI.color;

        Vector3 startPos = new Vector3(from.x, from.y, 0.0f);
        Vector3 endPos = new Vector3(to.x, to.y, 0.0f);

        Vector3 startTan = startPos + Vector3.right * 50;
        Vector3 endTan = endPos + Vector3.left * 50;

        for (int i = 0; i < 3; i++)
        {
            Handles.DrawBezier(startPos, endPos, startTan, endTan, shadowCol, null, (i + 1) * 5);
        }

        Handles.DrawBezier(startPos, endPos, startTan, endTan, GUI.color, null, 2.0f);
    }
}
#endif