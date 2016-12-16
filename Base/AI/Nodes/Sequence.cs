using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Sequence : CompositeNode
    {
        protected int CurrentIndex;

        public override string Name
        {
            get { return "Sequence"; }
        }

        public override string Description
        {
            get { return "Executes childs in sequence, stops execution and returns failrue on first failrue. Returns success when all childs returns success."; }
        }
    }
}
