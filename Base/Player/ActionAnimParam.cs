using UnityEngine;

namespace Framework
{
    public class ActionAnimParam : ActionGraphNode
    {
        public static readonly string TriggerMetadata = "_AnimTrigger";

        [SerializeField]
        [HideInInspector]
        public GenericParameter AnimParam = new GenericParameter();

        [Parametrized] [VisibleInInspector] public Animator Anim;// { get; set; }
    }
}