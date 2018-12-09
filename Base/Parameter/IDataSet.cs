namespace Framework
{
    public interface IDataSet
    {
        void Init();
        
        void InsertFromParameter(GenericParameter parameter);

        bool GetFromParameter(GenericParameter parameter);

        bool SetToParameter(GenericParameter parameter);

        bool HasValue(System.Type type, string name);

        bool HasValue<T>(string name);

        T GetValue<T>(string name);

        void SetValue<T>(string name, T newValue);
    }
}