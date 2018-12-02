using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.AI;
using UnityEngine;

namespace Framework
{
    public abstract class KnownType
    {
        public System.Type HoldType;
        public string GenericName;

#if UNITY_EDITOR
        public delegate void TypeDrawFunc(Rect rect, GenericParameter param, bool label);
        public delegate void TypeLayoutFunc(GenericParameter param, bool label);
        
        public TypeDrawFunc DrawFunc;
        public TypeLayoutFunc LayoutFunc;
#endif

        public abstract IValue CreateValue(GenericParameter param);
        public abstract PropertyReference CreateProperty<T>(T instance, string name);

        private static readonly Dictionary<string, KnownType> _knownTypes = new Dictionary<string, KnownType>();
        public static Dictionary<string, KnownType> Register => _knownTypes;

        private static System.Type _scriptableObjectType;
        public static System.Type ScriptableObjectType
        {
            get
            {
                if (_scriptableObjectType == null)
                    _scriptableObjectType = typeof(ScriptableObject);
                return _scriptableObjectType;
            }
        }

        private static System.Type _componentType;

        public static System.Type ComponentType
        {
            get
            {
                if (_componentType == null)
                    _componentType = typeof(Component);
                return _componentType;
            }
        }

        private static System.Type _objectType;

        public static System.Type ObjectType
        {
            get
            {
                if (_objectType == null)
                    _objectType = typeof(UnityEngine.Object);
                return _objectType;
            }
        }
        
        public static List<KnownType> GetKnownTypes()
        {
            return _knownTypes.Values.ToList();
        }

        public static void InsertKnownType<T>(KnownType<T> info)
        {
            _knownTypes[typeof(T).FullName] = info;
        }
    }

    public class KnownType<T> : KnownType
    {
        protected internal System.Func<GenericParameter, T> Getter;
        protected internal System.Action<GenericParameter, T> Setter;

        protected internal KnownType(string name)
        {
            GenericName = name;
            HoldType = typeof(T);
        }

        public override IValue CreateValue(GenericParameter param)
        {
            return new Value<T>(param.HoldType.Type, param.GetAs<T>());
        }

        public override PropertyReference CreateProperty<U>(U instance, string name)
        {
            return new PropertyReference<U, T>(instance, name);
        }
    }
}