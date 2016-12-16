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
    }
}
