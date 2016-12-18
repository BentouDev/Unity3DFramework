using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Negation : DecoratorNode
    {
        public override string Name
        {
            get { return "Negation"; }
        }

        public override string Description
        {
            get { return "Boolean negation of child result"; }
        }
        
        protected override NodeResult OnUpdate()
        {
            var childResult = CurrentController.CheckNodeStatus(DecoratedNode);
            if (childResult != NodeResult.Running)
            {
                if (childResult == NodeResult.Success)
                    return NodeResult.Failrue;
                else if (childResult == NodeResult.Failrue)
                    return NodeResult.Success;
            }

            return NodeResult.Running;
        }
    }
}
