using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public abstract class BaseBehaviour : MonoBehaviour, IBaseObject
    {
        public IDataSetProvider GetProvider()
        {
            return null;
        }

        public virtual ParameterReference GetParameter(string paramName, SerializedType holdType)
        {
            return null;
        }

        public virtual Variant GetVariant(string propertyName, SerializedType type)
        {
            return null;
        }

        public virtual bool IsParameterConstant(string name)
        {
            return false;
        }

        public void SetParameter(string paramName, ParameterReference parameter)
        { }

        public void SetParameterConst(string paramName, Variant value)
        { }

        public void ClearParameter(string paramName)
        { }

#if UNITY_EDITOR
        public Dictionary<string, ValidationResult> LastValidation = new Dictionary<string, ValidationResult>();

        public event Validation OnValidation;

        public List<Editor.INotify> Notifiers = new List<Editor.INotify>();

        public T GetNotify<T>(Editor.PropertyPath path) where T : Editor.INotify
        {
            return (T) Notifiers.FirstOrDefault(n => n is T && n.IsFromPath(path));
        }

        public virtual void OnPreValidate()
        {
            
        }

        public void OnPostValidate()
        {
            OnValidation?.Invoke();
        }

        public void ValidationSubscribe(Validation callback)
        {
            OnValidation -= callback;
            OnValidation += callback;
        }

        public void ValidationUnsubscribe(Validation callback)
        {
            OnValidation -= callback;
        }

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