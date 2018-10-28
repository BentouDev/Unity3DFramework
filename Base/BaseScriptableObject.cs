using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class BaseScriptableObject : ScriptableObject, IBaseObject
    {
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