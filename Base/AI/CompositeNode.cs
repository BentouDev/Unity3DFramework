using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public abstract class CompositeNode : BehaviourTreeNode
    {
        public List<BehaviourTreeNode> ChildNodes { get; private set; }

        public override List<BehaviourTreeNode> GetChildNodes()
        {
            return ChildNodes;
        }

        public override void AddOrSetChild(BehaviourTreeNode newNode)
        {
            if(ChildNodes == null)
                ChildNodes = new List<BehaviourTreeNode>();

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
