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

        public override void OnInit()
        {
            LastChildResult = NodeResult.Success;
        }

        protected override NodeResult OnUpdate()
        {
            if (LastChildResult != NodeResult.Running)
            {
                if (LastChildResult == NodeResult.Success)
                    return NodeResult.Failrue;
                else if (LastChildResult == NodeResult.Failrue)
                    return NodeResult.Success;
            }

            return NodeResult.Running;
        }
    }
}
