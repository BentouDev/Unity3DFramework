using System.Collections.Generic;

namespace Framework
{
    public interface IDataSetProvider
    {
        List<GenericParameter> GetParameters();
        List<GenericParameter> GetParameters(System.Predicate<GenericParameter> param);
        bool CanEditObject(UnityEngine.Object obj);
        bool HasObject(UnityEngine.Object obj);
        void SetToParameter(GenericParameter parameter);
        void GetFromParameter(GenericParameter parameter);
    }

    public delegate void Validation();

    public interface IBaseObject
    {
        IDataSetProvider GetProvider();
        GenericParameter GetParameter(string paramName, SerializedType holdType);

        bool IsParameterConstant(string name);

        void SetParameter(string paramName, GenericParameter parameter, bool constant = false);
        void ClearParameter(string paramName);

#if UNITY_EDITOR
        T GetNotify<T>(Editor.PropertyPath path) where T : Editor.INotify;
        
        event Validation OnValidation;

        void OnPostValidate();

        void ValidationSubscribe(Validation callback);

        void ValidationUnsubscribe(Validation callback);

        ValidationResult PreviousResult(string memberName);
        void UpdateValidation(string memberName, ValidationResult result);
#endif
    }
}