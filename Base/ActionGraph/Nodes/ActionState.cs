using System.Collections.Generic;
using Framework.Actions;
using Framework.Editor;
using UnityEngine;

namespace Framework
{
    public class ActionState : ActionGraphNode
    {
        public enum EWorkMode
        {
            Single,
            LoopUntilInterrupt,
            LoopUntilEvent,
            LoopUntilCondition
        }

        public EWorkMode WorkMode;

        public Condition InterruptCondition;
        public string InterruptEvent;

        [SerializeField]
        public List<BaseAction> Actions;

        [System.Serializable]
        public class ConnectionInfo
        {
            [SerializeField]
            public string Name;
            
            [SerializeField]
            public TConditionList Condition = new TConditionList();
            
            [HideInInspector]
            [SerializeField]
            public int ChildId = -1;
        }

        [Header("Outputs")]
        [Space]
        [SerializeField]
        public List<ConnectionInfo> ConnectionInfos = new List<ConnectionInfo>();
        
        public override void OnSetupParametrizedProperties()
        {
            SetupParameters(this);
        }
    }
}