using System.Collections.Generic;
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

        [System.Serializable]
        public class ConnectionInfo
        {
            [SerializeField]
            public string Name;
            
            [SerializeField]
            public TConditionList Condition;
            
            // [HideInInspector]
            [SerializeField]
            public int ChildId = -1;
        }

        [SerializeField]
        public List<ConnectionInfo> ConnectionInfos = new List<ConnectionInfo>();
        
        public override void OnSetupParametrizedProperties()
        {
            SetupParameters(this);
        }
    }
}