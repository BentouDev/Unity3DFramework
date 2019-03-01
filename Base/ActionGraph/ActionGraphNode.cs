using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class ActionGraphNode : ActionGraphNodeBase
    {
        public int Priority;

        [SerializeField]
        [HideInInspector]
        public List<ActionGraphNode> Connections = new List<ActionGraphNode>();
    }

    public abstract class ActionGraphNode<T> : ActionGraphNode where T : ActionGraphNodeBase
    {
        public override void OnSetupParametrizedProperties()
        {
            SetupParameters<T>(this as T);
        }
    }
}