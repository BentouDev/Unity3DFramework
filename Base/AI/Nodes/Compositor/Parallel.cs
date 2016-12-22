using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Parallel : CompositeNode<Parallel>
    {
        public override string Name
        {
            get { return "Parallel"; }
        }

        public override string Description
        {
            get { return "Executes children in parallel"; }
        }

        protected override NodeResult OnUpdate()
        {
            throw new System.NotImplementedException();
        }
    }
}
