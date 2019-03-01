using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class ActionSelector : ActionGraphNode<ActionSelector>
    {
        [HideInInspector]
        [SerializeField]
        public SerializedType InputType;

        [System.Serializable]
        public class EntryPoint
        {
            [SerializeField]
            public GenericParameter Input;

            [SerializeField]
            public List<ActionGraphNode> Nodes = new List<ActionGraphNode>();
        }

        [SerializeField]
        public List<EntryPoint> Inputs = new List<EntryPoint>();
    }
}