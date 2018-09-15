using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public class ParamList : Framework.AI.Blackboard, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private List<string> Keys;

        [SerializeField]
        [HideInInspector]
        private List<GenericParameter> Values;

        public void OnBeforeSerialize()
        {
            Keys   = Entries.Keys  .ToList();
            Values = Entries.Values.Select(x =>
            {
                var param = new GenericParameter(x.GetValueType());
                x.SetTo(param);
                return param;
            }).ToList();
        }

        public void OnAfterDeserialize()
        {
            foreach (GenericParameter parameter in Values)
            {
                InsertFromParameter(parameter);
            }
        }

        public GenericParameter Field;
    }
}