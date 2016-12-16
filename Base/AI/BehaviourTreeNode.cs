using System.Collections;
using System.Collections.Generic;
using MyNamespace;
using UnityEngine;

namespace Framework.AI
{
    public abstract class BehaviourTreeNode : ScriptableObject
    {
        protected static readonly List<BehaviourTreeNode> EmptyList = new List<BehaviourTreeNode>();

        public abstract string Name { get; }
        public abstract string Description { get; }

        public ParentNode Parent { get; set; }

        public NodeResult LastResult { get; private set; }

        public bool IsRootNode()
        {
            return Parent == null;
        }

        public virtual bool IsParentNode()
        {
            return false;
        }

        public ParentNode AsParentNode()
        {
            return (ParentNode) this;
        }

        public virtual void Init()
        {

        }

        public virtual void OnStart(AIController controller)
        {

        }

        public NodeResult CallUpdate(AIController controller, Blackboard blackboard)
        {
            LastResult = OnUpdate(controller, blackboard);
            return LastResult;
        }

        protected abstract NodeResult OnUpdate(AIController controller, Blackboard blackboard);

        public virtual void OnEnd(AIController controller)
        {

        }

#if UNITY_EDITOR
        [HideInInspector] public Vector2 EditorPosition;
#endif
    }
}
