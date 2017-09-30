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
        public List<GenericParameter> Parameters = new List<GenericParameter>();

        [SerializeField]
        public List<BehaviourTreeNode> Nodes = new List<BehaviourTreeNode>();

        [HideInInspector]
        [SerializeField]
        public BehaviourTreeNode RootNode;
        
        public void BuildEmptyBlackboard(Blackboard blackboard)
        {
            GenericParameter.BuildKnownTypeList();
            
            foreach (GenericParameter parameter in Parameters)
            {
                blackboard.InsertFromParameter(parameter);
            }
        }
        
        public void GetAllEnabledNodes(List<BehaviourTreeNode> nodes)
        {
            ProcessNodeRecurrent(RootNode, nodes);
        }

        public List<BehaviourTreeNode> GetAllEnabledNodes()
        {
            List<BehaviourTreeNode> nodes = new List<BehaviourTreeNode>();

            GetAllEnabledNodes(nodes);

            return nodes;
        }

        private void ProcessNodeRecurrent(BehaviourTreeNode node, List<BehaviourTreeNode> nodes)
        {
            nodes.Add(node);

            if (!node.IsParentNode())
                return;

            var children = node.AsParentNode().GetChildNodes();
            foreach (BehaviourTreeNode child in children)
            {
                ProcessNodeRecurrent(child, nodes);
            }
        }

        public bool Contains(BehaviourTreeNode node)
        {
            return FindNodeRecursive(RootNode, node);
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
                for (int i = 0; i < childNodes.Count; i++)
                {
                    if (FindNodeRecursive(childNodes[i], toFind))
                        return true;
                }
            }

            return false;
        }
    }
}
