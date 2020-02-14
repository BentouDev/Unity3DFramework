using System.Collections.Generic;

namespace Framework
{
    public interface IDataSetProvider
    {
        List<Parameter> GetParameters();
        List<Parameter> GetParameters(System.Predicate<Parameter> param);

        bool TryGetParameter(ParameterReference reference, out Parameter param);

        void SetToVariant(ParameterReference parameter, Variant localValue);
        void SetFromVariant(ParameterReference parameter, Variant localValue);
        
        bool CanEditObject(UnityEngine.Object obj);
        bool HasObject(UnityEngine.Object obj);

    }

    public delegate void Validation();

    public interface IBaseObject
    {
        IDataSetProvider GetProvider();

        ParameterReference GetParameter(string paramName, SerializedType holdType);

        Variant GetVariant(string propertyName, SerializedType type);

        bool IsParameterConstant(string name);

        void SetParameter(string paramName, ParameterReference parameter);

        void SetParameterConst(string paramName, Variant value);
        
        void ClearParameter(string paramName);

#if UNITY_EDITOR
        T GetNotify<T>(Editor.PropertyPath path) where T : Editor.INotify;
        
        event Validation OnValidation;

        void OnPreValidate();

        void OnPostValidate();

        void ValidationSubscribe(Validation callback);

        void ValidationUnsubscribe(Validation callback);

        ValidationResult PreviousResult(string memberName);
        void UpdateValidation(string memberName, ValidationResult result);
#endif
    }
}
