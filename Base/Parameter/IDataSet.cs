namespace Framework
{
    public interface IDataSet
    {
        void Init();
        
        void InsertFromParameter(Parameter parameter);

        bool GetFromParameter(Variant parameter);

        bool SetToParameter(Variant parameter);

        bool HasValue(System.Type type, string name);

        bool HasValue<T>(string name);

        T GetValue<T>(string name);

        void SetValue<T>(string name, T newValue);
    }
}