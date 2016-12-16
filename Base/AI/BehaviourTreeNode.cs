using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public abstract class BehaviourTreeNode : ScriptableObject
    {
        protected static readonly List<BehaviourTreeNode> EmptyList = new List<BehaviourTreeNode>();

        public abstract string Name { get; }
        public abstract string Description { get; }

        public BehaviourTreeNode Parent { get; set; }

        public virtual List<BehaviourTreeNode> GetChildNodes()
        {
            return EmptyList;
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
        
        public virtual void OnStart()
        {

        }

        public virtual void OnEnd()
        {

        }

#if UNITY_EDITOR
        [HideInInspector] public Vector2 EditorPosition;

        public virtual bool HasChildrenSlots()
        {
            return false;
        }
#endif
    }
}
