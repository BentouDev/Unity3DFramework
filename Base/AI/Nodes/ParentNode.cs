using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using UnityEngine;

namespace MyNamespace
{
    public abstract class ParentNode : BehaviourTreeNode
    {
        public virtual List<BehaviourTreeNode> GetChildNodes()
        {
            return EmptyList;
        }

        public override bool IsParentNode()
        {
            return true;
        }

        public virtual void AddOrSetChild(BehaviourTreeNode newNode)
        {
            throw new System.InvalidOperationException(string.Format("Unabel to set children to node of type {0}",
                GetType().Name));
        }

        public virtual void RemoveChild(BehaviourTreeNode node)
        {
            throw new System.InvalidOperationException(string.Format("Unabel to remove children from node of type {0}",
                GetType().Name));
        }

        public virtual bool HasChildrenSlots()
        {
            return false;
        }
    }
}
