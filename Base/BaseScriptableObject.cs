using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class BaseScriptableObject : ScriptableObject, IBaseObject
    {
        public virtual IDataSetProvider GetProvider()
        {
            return null;
        }

        public virtual void SetParameter(string paramName, GenericParameter parameter, bool constant = false)
        { }

        public virtual void ClearParameter(string paramName)
        { }

#if UNITY_EDITOR
        public Dictionary<string, ValidationResult> LastValidation = new Dictionary<string, ValidationResult>();

        public ValidationResult PreviousResult(string memberName)
        {
            ValidationResult result = null;
            LastValidation.TryGetValue(memberName, out result);
            return result;
        }

        public void UpdateValidation(string memberName, ValidationResult result)
        {
            LastValidation[memberName] = result;
        }
#endif
    }
}