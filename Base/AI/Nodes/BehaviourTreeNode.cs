using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MyNamespace;
using UnityEngine;
using UnityEngine.Events;

namespace Framework.AI
{
    [System.Serializable]
    public abstract class BehaviourTreeNode : ScriptableObject, ISerializationCallbackReceiver
    {
        protected static readonly List<BehaviourTreeNode> EmptyList = new List<BehaviourTreeNode>();

        public abstract string Name { get; }
        public abstract string Description { get; }

        [SerializeField]
        [HideInInspector]
        public ParentNode Parent;

        [SerializeField]
        [HideInInspector]
        private List<string> RequiredKeys;

        [SerializeField]
        [HideInInspector]
        private List<GenericParameter> RequiredParameters;

        public Dictionary<string, GenericParameter> BlackboardRequired = new Dictionary<string, GenericParameter>();

        public AIController CurrentController { get; private set; }
        public Blackboard CurrentBlackboard { get; private set; }

        private bool IsInUpdate;

        public void SetRequiredParameter(string parameterName, GenericParameter parameter)
        {
            BlackboardRequired[parameterName] = parameter;
        }

        public void ClearRequiredParamerer(string parameterName)
        {
            BlackboardRequired.Remove(parameterName);
        }

        public int GetGenericParameterIndex(string parameterName, System.Type type, List<GenericParameter> parameters)
        {
            int result = -1;

            if (BlackboardRequired == null)
                return result;

            GenericParameter value;

            if (!BlackboardRequired.TryGetValue(parameterName, out value))
                return result;

            if(value.HoldType.Type == type)
            {
                result = parameters.FindIndex(p => p.Name.Equals(value.Name)
                                                   && p.HoldType.Equals(value.HoldType)
                                                   && p.HoldType.Type == type);
            }

            return result;
        }

        public void OnBeforeSerialize()
        {
            RequiredKeys = BlackboardRequired.Keys.ToList();
            RequiredParameters = BlackboardRequired.Values.ToList();
        }

        public void OnAfterDeserialize()
        {
            BlackboardRequired = new Dictionary<string, GenericParameter>();
            for (int i = 0; i < RequiredKeys.Count; i++)
            {
                BlackboardRequired[RequiredKeys[i]] = RequiredParameters[i];
            }

            RequiredKeys.Clear();
            RequiredParameters.Clear();
        }

        public bool IsRootNode()
        {
            return Parent == null;
        }

        public virtual bool IsParentNode()
        {
            return false;
        }

        public ParentNode AsParentNode()
        {
#if UNITY_EDITOR
            if(!IsParentNode())
                throw new InvalidOperationException(string.Format("Cannot Node {0} is not a Parent Node!", GetType().Name));
#endif
            return (ParentNode) this;
        }

        public virtual void OnInit() { }

        public virtual void OnStart(AIController controller) { }

        public virtual void OnEnd(AIController controller) { }
        
        private void GetFromBlackboard(Blackboard blackboard)
        {
            foreach (var pair in BlackboardRequired)
            {
                blackboard.SetToParameter(pair.Value);
            }
        }

        private void SetToBlackboard(Blackboard blackboard)
        {
            foreach (var pair in BlackboardRequired)
            {
                blackboard.GetFromParameter(pair.Value);
            }
        }

        protected void SwitchToNode(BehaviourTreeNode node)
        {
            if (IsInUpdate && CurrentController)
            {
                CurrentController.RequestSwitchToNode(this, node);
            }
            else
            {
                CurrentController.RequestSwitchToNode(null, null);
                Debug.LogError("Unable to switch state outside of OnUpdate()!", this);
            }
        }

        public NodeResult CallUpdate(AIController controller, Blackboard blackboard)
        {
            CurrentController = controller;
            CurrentBlackboard = blackboard;
            
            GetFromBlackboard(CurrentBlackboard);

            IsInUpdate = true;

            var result = OnUpdate();

            IsInUpdate = false;

            SetToBlackboard(CurrentBlackboard);

            CurrentController = null;
            CurrentBlackboard = null;

            return result;
        }

        protected abstract NodeResult OnUpdate();

        public static string GetNodeTypeName(System.Type type)
        {
            if (type.IsSubclassOf(typeof(CompositeNode)))
                return "Composite";
            if (type.IsSubclassOf(typeof(DecoratorNode)))
                return "Decorator";
            if (type.IsSubclassOf(typeof(TaskNode)))
                return "Task";

            return "Unknown";
        }

#if UNITY_EDITOR
        [HideInInspector] public Vector2 EditorPosition;
#endif
    }
}
