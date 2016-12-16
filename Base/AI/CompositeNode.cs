using System.Collections;
using System.Collections.Generic;
using MyNamespace;
using UnityEngine;

namespace Framework.AI
{
    public abstract class CompositeNode : ParentNode
    {
        public List<BehaviourTreeNode> ChildNodes { get; private set; }

        protected CompositeNode()
        {
            ChildNodes = new List<BehaviourTreeNode>();
        }

        public override List<BehaviourTreeNode> GetChildNodes()
        {
            return ChildNodes;
        }

        public override void AddOrSetChild(BehaviourTreeNode newNode)
        {
            newNode.Parent = this;
            ChildNodes.Add(newNode);
        }

        public override void RemoveChild(BehaviourTreeNode node)
        {
            ChildNodes.Remove(node);
        }

#if UNITY_EDITOR
        public override bool HasChildrenSlots()
        {
            return true;
        }
#endif
    }
}
