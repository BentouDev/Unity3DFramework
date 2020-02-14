using System;
using System.Linq;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public class ParameterReference : ISerializationCallbackReceiver
    {
        public static readonly ParameterReference None = new ParameterReference() { ParameterId = Guid.Empty };
        
        [SerializeField]
        public System.Guid ParameterId;

        [SerializeField]
        private string _guid;

        public Parameter Get(IDataSetProvider provider)
        {
            // ToDo: guid based dictionary lookup!
            return provider.GetParameters().FirstOrDefault(p => p.Id == ParameterId);
        }

        public bool IsValid()
        {
            return ParameterId != System.Guid.Empty;
        }

        public void OnBeforeSerialize()
        {
            _guid = ParameterId.ToString();
        }

        public void OnAfterDeserialize()
        {
            ParameterId = Guid.Parse(_guid);
        }
    }
}