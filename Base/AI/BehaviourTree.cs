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
        [HideInInspector]
        public BehaviourTreeNode RootNode;

        private List<BehaviourTreeNode> NodeList;
        
        public void BuildEmptyBlackboard(Blackboard blackboard)
        {
            // Create empty parameters
        }

        public bool IsPreprocessed()
        {
            return NodeList != null && NodeList.Count > 0;
        }

        public void PreprocessTree()
        {
            NodeList.Clear();
            ProcessNodeRecurrent(RootNode);
        }

        private void ProcessNodeRecurrent(BehaviourTreeNode node)
        {
            NodeList.Add(node);

            var children = node.GetChildNodes();
            foreach (BehaviourTreeNode child in children)
            {
                ProcessNodeRecurrent(child);
            }
        }
    }
}
