using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.AI.Editor
{
    public class BehaviourTreeEditorNode : GraphNode
    {
        public static readonly int NodeWidth = 120;
        public static readonly int NodeHeight = 40;

        private readonly BehaviourTree TreeAsset;
        public BehaviourTreeNode TreeNode { get; private set; }
        private readonly BehaviourTreeEditorPresenter Presenter;

        private Rect TextRect = new Rect();
        private bool HasParent;

        public override string Name => TreeNode.Name;

        public BehaviourTreeEditorNode(BehaviourTree tree, BehaviourTreeNode node, BehaviourTreeEditorPresenter presenter)
        {
            TreeAsset = tree;
            TreeNode  = node;
            Presenter = presenter;
            Position  = TreeNode.EditorPosition;

            Size = new Vector3(NodeWidth, NodeHeight);

            WindowTitle = GUIContent.none;
            WindowStyle = SpaceEditorStyles.GraphNodeBackground;
        }
        
        public override Vector2 GetParentConnectPosition(GraphNode parent)
        {
            return new Vector2(drawRect.center.x, drawRect.yMin + 2);
        }

        public override void GetChildConnectPositions(GraphNode child, IList<Vector2> positions)
        {
            if (TreeNode.IsParentNode())
            {
                var asParent  = TreeNode.AsParentNode();
                var nodeIndex = connectedTo.FindIndex(x => x.Target.Owner == child);
                if (nodeIndex != -1)
                {
                    var offset = ConnectorSize.x * nodeIndex;
                    if (asParent.HasChildrenSlots())
                        offset -= ConnectorSize.x * asParent.GetChildNodes().Count * 0.5f;

                    positions.Add(new Vector2(drawRect.center.x + offset, drawRect.yMax + ConnectorSize.y * 0.25f));
                    
                    return;
                }
            }

            positions.Add(drawRect.center);
        }

        protected override bool CanConnectTo(Slot targetSlot, Slot childSlot)
        {
            if (!(childSlot.Owner is BehaviourTreeEditorNode asBehaviour))
                return false;
            
            return !asBehaviour.HasParent && base.CanConnectTo(targetSlot, childSlot);
        }

        protected override void OnConnectToParent(Connection eventSource)
        {
            HasParent = true;
        }

        protected override void OnConnectToChild(Connection connection)
        {
            if (connection.Target.Owner is BehaviourTreeEditorNode asBehaviour)
            {
                Presenter.OnConnectNodes(TreeNode, asBehaviour.TreeNode);
            }
        }

        protected override void OnParentDisconnected(Connection node)
        {
            HasParent = false;
        }

        protected override void OnSelected(bool value)
        {
            if (Editor.CurrentMouseMode == null)
                Selection.activeObject = TreeNode;
        }

        protected override void OnGUI()
        {
            WindowStyle = Selected
                ? SpaceEditorStyles.GraphNodeBackgroundSelected
                : SpaceEditorStyles.GraphNodeBackground;
            TreeNode.EditorPosition = Position;

            GUI.Box(drawRect, GUIContent.none, WindowStyle);
            DrawConnectDots(drawRect);
        }

        protected override void OnDrawContent()
        {
            // var textDimensions = EditorStyles.largeLabel.CalcSize(new GUIContent(Name));

            float x_padding = 4;
            float y_padding = 4;

            TextRect.Set(
                drawRect.x + x_padding, // + 0.5f * (drawRect.width - textDimensions.x), 
                drawRect.y + y_padding, // + 0.5f * (drawRect.height - textDimensions.y), 
                drawRect.width - 2 * x_padding, 20
            );

            if (TreeAsset.RootNode == TreeNode)
            {
                GUI.Label(TextRect, new GUIContent("Root"), EditorStyles.whiteBoldLabel);
            }
            else
            {
                GUI.Label(TextRect, TreeNode.name, EditorStyles.whiteLargeLabel);
//                var newName = 
//                if (newName != TreeNode.name)
//                {
//                    Undo.RecordObject(TreeNode, "Rename node");
//                    TreeNode.name = newName;
//                }
            }

            TextRect.Set(
                TextRect.x,
                TextRect.y + 16,
                TextRect.width,
                TextRect.height
            );
            
            GUI.Label(TextRect, new GUIContent(TreeNode.GetType().Name, TreeNode.Description), EditorStyles.whiteLabel);
        }

        void DrawConnectDots(Rect dotRect)
        {
            Vector2 offset = Vector2.zero;

            if (TreeNode.IsParentNode())
            {
                var asParent = TreeNode.AsParentNode();
                var nodeCount = asParent.GetChildNodes().Count;

                if (nodeCount > 0)
                {
                    if (asParent.HasChildrenSlots())
                        offset.x -= ConnectorSize.x * nodeCount * 0.5f;

                    for (int i = 0; i < nodeCount; i++)
                    {
                        GUI.Box (
                            new Rect(dotRect.center.x - ConnectorSize.x * 0.5f + offset.x, dotRect.yMax - ConnectorSize.y * 0.25f, ConnectorSize.x, ConnectorSize.y),
                            new GUIContent(), SpaceEditorStyles.DotFlowTarget
                        );

                        GUI.Box (
                            new Rect(dotRect.center.x - ConnectorSize.x * 0.5f + offset.x, dotRect.yMax - ConnectorSize.y * 0.25f, ConnectorSize.x, ConnectorSize.y),
                            new GUIContent(), SpaceEditorStyles.DotFlowTargetFill
                        );

                        GUI.color = Color.white;

                        offset.x += ConnectorSize.x;
                    }
                }

                if (asParent.HasChildrenSlots())
                {
                    Vector2 pos  = offset + new Vector2(dotRect.center.x - ConnectorSize.x * 0.5f, dotRect.yMax - ConnectorSize.y * 0.25f);
                    Rect    rect = new Rect(pos.x, pos.y, ConnectorSize.x, ConnectorSize.y);

                    if (rect.Contains(Event.current.mousePosition))
                    {
                        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
                        {
                            Presenter.OnEmptyDotClicked(this, rect.center);
                        }
                    }

                    GUI.Box(rect, GUIContent.none, SpaceEditorStyles.DotFlowTarget);
                }
            }
        }
    }
}