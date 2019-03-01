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

        public virtual GenericParameter GetParameter(string paramName, SerializedType holdType)
        {
            return null;
        }

        public virtual bool IsParameterConstant(string name)
        {
            return false;
        }

        public void SetParameter(string paramName, GenericParameter parameter, bool constant = false)
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