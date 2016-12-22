using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Succeder : DecoratorNode<Succeder>
    {
        public override string Name
        {
            get { return "Succeder"; }
        }

        public override string Description
        {
            get { return "Ignores children result and always returns success"; }
        }

        protected override NodeResult OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}
