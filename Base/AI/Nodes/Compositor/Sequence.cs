using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class Sequence : CompositeNode<Sequence>
    {
        [Blackboard.Required]
        public int CurrentChildIndex { get; set; }

        public override string Name
        {
            get { return "Sequence"; }
        }

        public override string Description
        {
            get { return "Executes childs in sequence, stops execution and returns failrue on first failrue.\nReturns success when all childs returns success."; }
        }
        
        protected override NodeResult OnUpdate()
        {
            var currentChild = GetChildNodes()[CurrentChildIndex];
            var currentChildResult = CurrentController.CheckNodeStatus(currentChild);
            if (currentChildResult != NodeResult.Running)
            {
                if (currentChildResult == NodeResult.Failrue)
                {
                    CurrentChildIndex = 0;
                    return NodeResult.Failrue;
                }
                if (currentChildResult == NodeResult.Suspended)
                {
                    SwitchToNode(ChildNodes[CurrentChildIndex]);
                    return NodeResult.Running;
                }
                else
                {
                    CurrentChildIndex++;

                    if (CurrentChildIndex == ChildNodes.Count)
                    {
                        CurrentChildIndex = 0;
                        return NodeResult.Success;
                    }

                    SwitchToNode(ChildNodes[CurrentChildIndex]);
                }
            }

            return NodeResult.Running;
        }
    }
}
