namespace Framework
{
    public interface IBaseObject
    {
#if UNITY_EDITOR
        ValidationResult PreviousResult(string memberName);
        void UpdateValidation(string memberName, ValidationResult result);
#endif
    }
}