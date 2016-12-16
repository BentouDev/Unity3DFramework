using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Repeater : DecoratorNode
    {
        public override string Name
        {
            get { return "Repeater"; }
        }

        public override string Description
        {
            get { return "Repeats given times"; }
        }
    }
}
