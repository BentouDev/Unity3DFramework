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
        private readonly BehaviourTreeNode TreeNode;

        public override string Name { get { return TreeNode.Name; } }

        public BehaviourTreeEditorNode(BehaviourTree tree, BehaviourTreeNode node)
        {
            TreeAsset = tree;
            TreeNode  = node;
            Position  = TreeNode.EditorPosition;

            Size = new Vector3(NodeWidth, NodeHeight);

            WindowTitle = GUIContent.none;
            WindowStyle = SpaceEditorStyles.GraphNodeBackground;
        }

        protected override void OnGUI(int id)
        {
            TreeNode.EditorPosition = Position;
        }
        
        protected override void OnDrawContent(int id)
        {
            var textDimensions = EditorStyles.largeLabel.CalcSize(new GUIContent(Name));

            Rect winRect = new Rect(0.5f * (drawRect.width - textDimensions.x), 0.5f * (drawRect.height - textDimensions.y), textDimensions.x, textDimensions.y);
            GUI.Label(winRect, new GUIContent(Name, TreeNode.Description), EditorStyles.largeLabel);
        }
    }
}