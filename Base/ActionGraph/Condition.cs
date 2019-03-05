using System.Collections.Generic;
using Malee;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public class TConditionList : ReorderableArray<Condition>
    { }

    [System.Serializable]
    public abstract class Condition : ParametrizedScriptableObject
    {
        public static readonly string NO_DESCRIPTION = string.Empty;

        public int Priority;

        [SerializeField]
        [HideInInspector]
        public ActionGraph Graph;

        public abstract bool IsSatisfied();
        
        public virtual string GetDescription()
        {
            return NO_DESCRIPTION;
        }

        void OnValidate()
        {
            var desc = GetDescription();
            if (desc.Equals(NO_DESCRIPTION))
                return;

            name = desc;
        }

        public override IDataSetProvider GetProvider()
        {
            return Graph;
        }
    }
}