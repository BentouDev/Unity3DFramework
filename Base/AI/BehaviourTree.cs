using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Data/Behaviour Tree", fileName = "New Behaviour Tree")]
#endif
    public class BehaviourTree : ScriptableObject
    {
        [SerializeField]
        public List<EditorParameter> Parameters;

        [HideInInspector]
        [SerializeField]
        public BehaviourTreeNode RootNode;

        public List<BehaviourTreeNode> NodeList { get; set; }

        public void BuildEmptyBlackboard(Blackboard blackboard)
        {
            // Create empty parameters
        }

        public bool IsPreprocessed()
        {
            return NodeList != null && NodeList.Count > 0;
        }

        public void Preprocess()
        {
            NodeList.Clear();
            ProcessNodeRecurrent(RootNode);
        }

        private void ProcessNodeRecurrent(BehaviourTreeNode node)
        {
            NodeList.Add(node);

            if (node.IsParentNode())
            {
                var children = node.AsParentNode().GetChildNodes();
                foreach (BehaviourTreeNode child in children)
                {
                    ProcessNodeRecurrent(child);
                }
            }
        }

        public bool Contains(BehaviourTreeNode node)
        {
            if (IsPreprocessed())
                return NodeList.Contains(node);
            else
            {
                return FindNodeRecursive(RootNode, node);
            }
        }

        private bool FindNodeRecursive(BehaviourTreeNode node, BehaviourTreeNode toFind)
        {
            if (node == null)
                return false;

            if (node == toFind)
                return true;

            if (node.IsParentNode())
            {
                var childNodes = node.AsParentNode().GetChildNodes();
                foreach (BehaviourTreeNode childNode in childNodes)
                {
                    if (FindNodeRecursive(childNode, toFind))
                        return true;
                }
            }

            return false;
        }
    }
}
