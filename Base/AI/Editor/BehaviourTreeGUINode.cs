using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Framework.AI
{
    public class BehaviourTreeGUINode : BaseNode
    {
        public BehaviourTreeNode TreeNode { get; private set; }

        public delegate void NewChildNodeCallback(BehaviourTreeNode node);

        public static NewChildNodeCallback OnNewChildNode;

        public override Vector2 GetChildConnectPosition(BaseNode child)
        {
            return new Vector2 (
                drawRect.center.x,
                drawRect.yMax + ConnectorSize.y * 0.5f
            );
        }

        public override Vector2 GetParentConnectPosition(BaseNode parent)
        {
            return new Vector2 (
                drawRect.center.x,
                drawRect.yMin - ConnectorSize.y * 0.5f
            );
        }

        public BehaviourTreeGUINode(BehaviourTreeNode node)
        {
            TreeNode = node;
            Size = new Vector2(150, 50);

            Position = TreeNode.EditorPosition;
            Name = TreeNode.Name;
        }

        public override int OnGUI(int id)
        {
            DrawConnectDots();

            drawRect = GUI.Window(id, drawRect, WindowRoutine, Name);
            position = drawRect.center;

            TreeNode.EditorPosition = Position;
            Position = new Vector2 ( 
                Position.x - (Position.x % 5),
                Position.y - (Position.y % 5)
            );
            
            return 0;
        }

        void WindowRoutine(int id)
        {
            HandleSelection();
            HandleDeletion();

            GUILayout.Label(TreeNode.Description);
            GUI.DragWindow();
        }

        void HandleSelection()
        {
            if (Event.current.isMouse
            && Event.current.type == EventType.MouseDown
            && Event.current.button == 0
            && Selection.activeObject != TreeNode)
            {
                Selection.activeObject = TreeNode;
            }
        }

        void HandleDeletion()
        {
            if (Event.current.isKey
            && Event.current.type == EventType.KeyDown
            && Event.current.keyCode == KeyCode.Delete)
            {
                BaseNode.toDelete = this;
                Event.current.Use();
            }
        }

        void DrawConnectDots()
        {
            if (TreeNode && TreeNode.Parent)
            {
                GUI.Button (
                   new Rect(drawRect.center.x - ConnectorSize.x * 0.5f, drawRect.yMin - ConnectorSize.y, ConnectorSize.x, ConnectorSize.y),
                   new GUIContent()
                );
            }

            Vector2 offset = Vector2.zero;

            var childNodes = TreeNode.GetChildNodes();
            if (childNodes != null && childNodes.Count > 0)
            {
                offset.x += ConnectorSize.x;
                GUI.Button (
                    new Rect(drawRect.center.x - ConnectorSize.x * 0.5f, drawRect.yMax, ConnectorSize.x, ConnectorSize.y),
                    new GUIContent()
                );
            }

            if (TreeNode.HasChildrenSlots())
            {
                Vector2 pos = offset + new Vector2(drawRect.center.x - ConnectorSize.x * 0.5f, drawRect.yMax);
                if (GUI.Button (
                    new Rect(pos.x, pos.y, ConnectorSize.x, ConnectorSize.y),
                    new GUIContent("+")
                ))
                {
                    if (OnNewChildNode != null)
                    {
                        OnNewChildNode(TreeNode);
                    }
                }
            }
        }
    }
}
