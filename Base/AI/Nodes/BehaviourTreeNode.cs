using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

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
        private List<ParametrizedProperty> Parameters;

        public Dictionary<string, ParametrizedProperty> ParametrizedProperties = new Dictionary<string, ParametrizedProperty>();

        public AIController CurrentController { get; private set; }
        public Blackboard   CurrentBlackboard { get; private set; }

        private bool IsInUpdate;
        
        protected abstract void OnSetupRequiredParameters();

        protected void SetupParametersForType<T>(T instance) where T : BehaviourTreeNode
        {
            foreach (var key in ParametrizedProperties.Keys.ToList())
            {
//                var paramProp = ParametrizedProperties[key];
//                    paramProp.Initialize<T>(this as T, GetProvider(), key);

//                if (paramProp.Constant)
//                {
//                    paramProp.Property.GetFromParameter(instance, paramProp.Parameter);
//                }
//
//                ParametrizedProperties[key] = paramProp;
            }
        }

        public void SetRequiredParameter(string parameterName, Variant parameter, bool constant = false)
        {
            // ParametrizedProperties[parameterName] = new ParametrizedProperty() { Parameter = parameter, Constant = constant };
        }

        public void ClearRequiredParamerer(string parameterName)
        {
            ParametrizedProperties.Remove(parameterName);
        }

        public bool IsGenericParameterConstant(string parameterName)
        {
            if (ParametrizedProperties == null)
                return false;

            ParametrizedProperty value;

            if (ParametrizedProperties.TryGetValue(parameterName, out value))
                return value.Constant;

            return false;
        }
        
        public int GetGenericParameterIndex(string parameterName, System.Type type, List<Variant> parameters)
        {
            int result = -1;

            if (ParametrizedProperties == null)
                return result;

            ParametrizedProperty value;

            if (!ParametrizedProperties.TryGetValue(parameterName, out value))
                return result;

//            if(value.Parameter.HoldType.Type == type)
//            {
//                result = parameters.FindIndex(p => p.Name.Equals(value.Parameter.Name)
//                                                && p.HoldType.Equals(value.Parameter.HoldType)
//                                                && p.HoldType.Type == type);
//            }

            return result;
        }

        public void OnBeforeSerialize()
        {
            RequiredKeys = ParametrizedProperties.Keys  .ToList();
            Parameters   = ParametrizedProperties.Values.ToList();
        }

        public void OnAfterDeserialize()
        {
            ParametrizedProperties = new Dictionary<string, ParametrizedProperty>();

            for (int i = 0; i < RequiredKeys.Count; i++)
            {
                ParametrizedProperties[RequiredKeys[i]] = Parameters[i];
            }

            RequiredKeys.Clear();
            Parameters  .Clear();
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
                throw new System.InvalidOperationException(string.Format("Cannot Node {0} is not a Parent Node!", GetType().Name));
#endif
            return (ParentNode) this;
        }

        public void Init()
        {
            OnSetupRequiredParameters();
            OnInit();
        }

        protected virtual void OnInit() { }

        public virtual void OnStart(AIController controller) { }

        public virtual void OnEnd(AIController controller) { }
        
        protected void GetFromBlackboard(Blackboard blackboard)
        {
            foreach (var pair in ParametrizedProperties.Where(p => !p.Value.Constant))
            {
                // blackboard.SetToParameter(pair.Value.Parameter);
                // pair.Value.Property.GetFromParameter(pair.Value.Parameter);
            }
        }
        
        protected void SetToBlackboard(Blackboard blackboard)
        {
            foreach (var pair in ParametrizedProperties.Where(p => !p.Value.Constant))
            {
                // pair.Value.Property.SetToParameter(pair.Value.Parameter);
                // pair.Value.SetToProvider(blackboard);
                // blackboard.GetFromParameter(pair.Value.Parameter);
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
