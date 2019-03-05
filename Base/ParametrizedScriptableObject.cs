using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public abstract class ParametrizedScriptableObject : BaseScriptableObject, ISerializationCallbackReceiver
    {
        public abstract void OnSetupParametrizedProperties();

        [SerializeField]
        [HideInInspector]
        private List<ParametrizedProperty> Parameters;

        [SerializeField]
        [HideInInspector]
        private List<string> ParameterNames;

        public Dictionary<string, ParametrizedProperty> ParametrizedProperties = new Dictionary<string, ParametrizedProperty>();

        protected void SetupParameters<T>(T instance) where T : ParametrizedScriptableObject
        {
            foreach (var (key, property) in ParametrizedProperties)
            {
                property.Initialize<T>(instance, key);
            }
        }

        public void UpdateFromParameters()
        {
            foreach (var pair in ParametrizedProperties.Where(p => !p.Value.Constant))
            {
                pair.Value.UpdateFromProvider(GetProvider());
            }
        }

        public void UploadToParameters()
        {
            foreach (var pair in ParametrizedProperties.Where(p => !p.Value.Constant))
            {
                pair.Value.SetToProvider(GetProvider());
            }
        }
        
        public override bool IsParameterConstant(string paramName)
        {
            if (ParametrizedProperties.TryGetValue(paramName, out var property))
            {
                return property.Constant;
            }

            return false;
        }

        public override GenericParameter GetParameter(string paramName, SerializedType holdType)
        {
            if (ParametrizedProperties.TryGetValue(paramName, out var property))
            {
                return property.Parameter;
            }

            return null;
        }

        public override void SetParameter(string paramName, GenericParameter parameter, bool constant = false)
        {
            if (ParametrizedProperties.TryGetValue(paramName, out var prop))
            {
                prop.Constant = constant;
                prop.Parameter = parameter;
            }
            else
            {
                ParametrizedProperties[paramName] = new ParametrizedProperty()
                {
                    Constant = constant, 
                    Parameter = parameter
                };                
            }
        }

        public override void ClearParameter(string paramName)
        {
            ParametrizedProperties.Remove(paramName);
        }

        public void OnBeforeSerialize()
        {
            ParameterNames = ParametrizedProperties.Keys  .ToList();
            Parameters     = ParametrizedProperties.Values.ToList();
        }

        public void OnAfterDeserialize()
        {
            ParametrizedProperties = new Dictionary<string, ParametrizedProperty>();

            for (int i = 0; i < ParameterNames.Count; i++)
            {
                ParametrizedProperties[ParameterNames[i]] = Parameters[i];
            }

            ParameterNames.Clear();
            Parameters.Clear();
        }
    }
}