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
            get { return "Executes childs in sequence, stops execution and returns failrue on first failrue.\nReturns success when all childs returns success."; }
        }
        
        public override void OnInit()
        {
            CurrentIndex = 0;
            LastChildResult = NodeResult.Success;
        }

        protected override NodeResult OnUpdate()
        {
            if (LastChildResult != NodeResult.Running)
            {
                if (LastChildResult == NodeResult.Failrue)
                    return NodeResult.Failrue;
                else
                {
                    CurrentIndex++;

                    if (CurrentIndex == ChildNodes.Count)
                    {
                        CurrentIndex = 0;
                        return NodeResult.Success;
                    }

                    CurrentController.ScheduleBefore(ChildNodes[CurrentIndex]);
                }
            }

            return NodeResult.Running;
        }
    }
}
