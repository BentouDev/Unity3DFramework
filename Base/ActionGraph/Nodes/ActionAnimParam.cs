using System;
using UnityEngine;

namespace Framework
{
    public class ActionAnimParam : ActionGraphNode<ActionAnimParam>
    {
        public static readonly string TriggerMetadata = "_AnimTrigger";

        [SerializeField]
        [HideInInspector]
        public GenericParameter AnimParam = new GenericParameter();

        [Parametrized] 
        [VisibleInInspector]
        public RuntimeAnimatorController Anim;
    }
}