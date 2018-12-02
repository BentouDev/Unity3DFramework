using System.Collections.Generic;

namespace Framework
{
    public interface IDataSetProvider
    {
        List<GenericParameter> GetParameters();
        List<GenericParameter> GetParameters(System.Predicate<GenericParameter> param);
        bool CanEditObject(UnityEngine.Object obj);
        bool HasObject(UnityEngine.Object obj);
    }

    public interface IBaseObject
    {
        IDataSetProvider GetProvider();
        void SetParameter(string paramName, GenericParameter parameter, bool constant = false);
        void ClearParameter(string paramName);

#if UNITY_EDITOR
        ValidationResult PreviousResult(string memberName);
        void UpdateValidation(string memberName, ValidationResult result);
#endif
    }
}