using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public abstract class TaskNode : BehaviourTreeNode { }

    public abstract class TaskNode<T> : TaskNode where T : BehaviourTreeNode
    {
        protected sealed override void OnSetupRequiredParameters()
        {
            base.SetupParametersForType<T>(this as T);
        }
    }
}
