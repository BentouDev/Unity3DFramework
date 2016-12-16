using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Negation : DecoratorNode
    {
        public override string Name
        {
            get { return "Inverter"; }
        }

        public override string Description
        {
            get { return "Boolean negation"; }
        }

        protected override NodeResult OnUpdate(AIController controller, Blackboard blackboard)
        {
            throw new System.NotImplementedException();
        }
    }
}
