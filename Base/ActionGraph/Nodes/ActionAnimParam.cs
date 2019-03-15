using System;
using UnityEngine;

namespace Framework
{
    public class ActionAnimParam : ActionGraphNode<ActionAnimParam>
    {
        public static readonly string TriggerMetadata = "_AnimTrigger";

        [SerializeField]
        [HideInInspector]
        public Variant AnimParam = new Variant();

        [Parametrized] 
        [VisibleInInspector]
        public RuntimeAnimatorController Anim;
    }
}