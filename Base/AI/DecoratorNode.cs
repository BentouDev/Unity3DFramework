using System;
using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using MyNamespace;
using UnityEngine;

namespace Framework.AI
{
    public abstract class DecoratorNode : ParentNode
    {
        public BehaviourTreeNode DecoratedNode { get; private set; }

        public override List<BehaviourTreeNode> GetChildNodes()
        {
            if(!DecoratedNode)
                return EmptyList;

            return new List<BehaviourTreeNode>() { DecoratedNode };
        }

        public override void AddOrSetChild(BehaviourTreeNode newNode)
        {
            if (DecoratedNode == null)
            {
                newNode.Parent = this;
                DecoratedNode = newNode;
            }
            else
            {
                throw new InvalidOperationException(string.Format("Unable to set another child for Decorator Node descendant '{0}'", GetType().Name));
            }
        }

        public override void RemoveChild(BehaviourTreeNode node)
        {
            if (DecoratedNode == node)
                DecoratedNode = null;
        }

#if UNITY_EDITOR
        public override bool HasChildrenSlots()
        {
            return DecoratedNode == null;
        }
#endif
    }
}
