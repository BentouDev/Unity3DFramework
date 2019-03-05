using System;

namespace Framework
{
    public class TypeRestricted : BaseEditorAttribute
    {
        public enum TypeSource
        {
            Value,
            Field,
            Property,
            Method
        }

        public Type Type;
        public TypeSource Source;
        public string SourceValue;

        public TypeRestricted(Type type)
        {
            Type = type;
            Source = TypeSource.Value;
        }

        public TypeRestricted(TypeSource source, string value)
        {
            Source = source;
            SourceValue = value;

            if (Source == TypeSource.Value)
            {
                Type = new SerializedType(value).Type;
            }
        }
    }
}