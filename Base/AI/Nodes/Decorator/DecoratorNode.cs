using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public abstract class DecoratorNode : ParentNode
    {
        [SerializeField]
        [HideInInspector]
        public BehaviourTreeNode DecoratedNode;

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

    public abstract class DecoratorNode<T> : DecoratorNode where T : BehaviourTreeNode
    {
        protected override void OnSetupRequiredParameters()
        {
            base.SetupParametersForType<T>(this as T);
        }
    }
}
