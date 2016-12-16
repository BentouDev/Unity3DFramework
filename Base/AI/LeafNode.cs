using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public abstract class LeafNode : BehaviourTreeNode
    {
        public abstract NodeResult OnUpdate();
    }
}
