using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Selector : CompositeNode
    {
        public override string Name
        {
            get { return "Selector"; }
        }

        public override string Description
        {
            get { return "Returns success when any child return success, returns failrue when none"; }
        }
    }
}

