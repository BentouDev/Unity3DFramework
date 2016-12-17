using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Until : DecoratorNode
    {
        public override string Name
        {
            get { return "Until"; }
        }

        public override string Description
        {
            get { return "Repeats children until it returns success"; }
        }

        protected override NodeResult OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}
