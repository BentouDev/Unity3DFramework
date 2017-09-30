using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework.AI
{
    public class AIController : Controller
    {
        public BasePawn BasePawn;

        public BehaviourTree BehaviourTreeTemplate;

        public Blackboard Blackboard;

        public BehaviourTreeNode CurrentActingNode { get; private set; }

        private bool ShouldSuspendExecution;

        private readonly List<BehaviourTreeNode> NodeList = new List<BehaviourTreeNode>();

        private readonly LinkedList<int> NodeScheduler = new LinkedList<int>();

        private readonly Dictionary<int, NodeResult> NodeStatuses = new Dictionary<int, NodeResult>();

        private bool DidSwitchToAnotherNode;
        
        protected override void OnInit()
        {
            if (BehaviourTreeTemplate)
            {
                BehaviourTreeTemplate.BuildEmptyBlackboard(Blackboard);
                BehaviourTreeTemplate.GetAllEnabledNodes(NodeList);

                foreach (BehaviourTreeNode node in NodeList)
                {
                    node.Init();
                }
            }
            else
            {
                Debug.LogError("No Behaviour for AIController!", this);
            }

            ShouldSuspendExecution = false;
        }

        public void PrewarmScheduler()
        {
            for (int i = 0; i < NodeList.Count; i++)
            {
                NodeScheduler.AddLast(i);
                NodeStatuses[i] = NodeResult.Success;
            }
        }

        protected override void OnProcessControll()
        {
            if (NodeScheduler.Count == 0)
                PrewarmScheduler();

            // Start!
            ShouldSuspendExecution = false;

            // Keep running
            while (!ShouldSuspendExecution)
            {
                // We reached the end of scheduler
                if (NodeScheduler.Count == 0)
                {
                    ShouldSuspendExecution = true;
                    break;
                }

                if (CurrentActingNode == null)
                {
                    // If node is null, get fresh from scheduler
                    ChangeNode(NodeList[NodeScheduler.First.Value]);
                    NodeScheduler.RemoveFirst();
                }

                if (CurrentActingNode != null)
                {
                    var result = CurrentActingNode.CallUpdate(this, Blackboard);
                    var changed = UpdateNodeStatus(CurrentActingNode, result);

                    if (result != NodeResult.Running && changed)
                    {
                        if (CurrentActingNode.Parent != null)
                        {
                            // We continue, allow parent to process result and wait till parent sets new task node
                            ScheduleBefore(CurrentActingNode.Parent);
                            CurrentActingNode = null;
                        }
                        else
                        {
                            // We reached the end of scheduler
                            CurrentActingNode = null;
                            ShouldSuspendExecution = true;
                            NodeScheduler.Clear();
                        }
                    }
                    else
                    {
                        // Suspend and allow this node progress
                        ShouldSuspendExecution = true;
                    }
                }
                else
                {
                    // Failed because of error?
                    ShouldSuspendExecution = true;
                    Debug.LogError("Unexpected null instead of CurrentActingNode!");
                }

                // If we switched nodes, allow to take them from scheduler
                if (DidSwitchToAnotherNode)
                {
                    CurrentActingNode = null;
                    ShouldSuspendExecution = true;
                }
            }

            DidSwitchToAnotherNode = false;
        }

        public NodeResult CheckNodeStatus(BehaviourTreeNode node)
        {
            int index = NodeList.IndexOf(node);

            if (index >= 0 && index < NodeList.Count && NodeStatuses.ContainsKey(index))
            {
                return NodeStatuses[index];
            }

            Debug.LogErrorFormat("Unable to check status of node '{0}' of index '{1}', its not present in BehaviourTree!", node, index);
            throw new System.InvalidOperationException("Unable to check status of node");
        }

        private bool UpdateNodeStatus(BehaviourTreeNode node, NodeResult result)
        {
            bool changed = false;

            int index = NodeList.IndexOf(node);
            if (index >= 0 && index < NodeList.Count)
            {
                changed = NodeStatuses[index] != result;
                NodeStatuses[index] = result;
            }

            return changed;
        }

        private void ChangeNode(BehaviourTreeNode newNode)
        {
            if (CurrentActingNode) CurrentActingNode.OnEnd(this);

            CurrentActingNode = newNode;

            if (CurrentActingNode) CurrentActingNode.OnStart(this);
        }

        public void RequestSwitchToNode(BehaviourTreeNode parent, BehaviourTreeNode node)
        {
            DidSwitchToAnotherNode = true;

            if (CurrentActingNode == parent && node.Parent == parent)
            {
                ScheduleBefore(node);
                // CurrentActingNode = null; // ???
            }
            else
            {
                Debug.LogErrorFormat("Unable to switch to node {0} from {1}, because its not its parent.", node, parent);
            }
        }

        public void ScheduleBefore(BehaviourTreeNode node)
        {
            if (node == null)
                return;

            int index = NodeList.IndexOf(node);
            if (index >= 0 && index < NodeList.Count)
            {
                NodeScheduler.AddFirst(index);
            }
        }
    }
}
