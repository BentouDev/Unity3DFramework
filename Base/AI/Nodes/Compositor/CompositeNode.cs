using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using UnityEngine;

namespace Framework.AI
{
    [System.Serializable]
    public abstract class CompositeNode : ParentNode
    {
        [SerializeField]
        [HideInInspector]
        public List<BehaviourTreeNode> ChildNodes;

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

    public abstract class CompositeNode<T> : CompositeNode where T : BehaviourTreeNode
    {
        protected sealed override void OnSetupRequiredParameters()
        {
            base.SetupParametersForType<T>(this as T);
        }
    }
}
