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
        public abstract PropertyReference GetProperty(System.Type type, string name);
        protected internal abstract PropertyReference CreatePropertyForType<U>(string name);

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

        private static System.Type _animControllerType;
        
        public static System.Type AnimControllerType
        {
            get
            {
                if (_animControllerType == null)
                    _animControllerType = typeof(RuntimeAnimatorController);
                return _animControllerType;
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
        
        public static string GetDisplayedName(System.Type type)
        {
            var typename = type.FullName;

            KnownType info;
            if (KnownType.Register.TryGetValue(typename, out info))
            {
                return info.GenericName;
            }

            if (type.IsSubclassOf(KnownType.AnimControllerType)
                && KnownType.Register.ContainsKey(KnownType.AnimControllerType.FullName))
            {   
                return type.Name;
            }
            
            if (type.IsSubclassOf(KnownType.ComponentType)
                && KnownType.Register.ContainsKey(KnownType.ComponentType.FullName))
            {   
                return type.Name;
            }

            if (type.IsSubclassOf(KnownType.ScriptableObjectType)
                && KnownType.Register.ContainsKey(KnownType.ScriptableObjectType.FullName))
            {
                return type.Name;
            }

            return null;
        }
        
        public static KnownType GetKnownType(System.Type type)
        {
            if (type == null)
                return null;

            var typename = type.FullName;

            KnownType info;
            if (KnownType.Register.TryGetValue(typename, out info))
            {
                return info;
            }

            if (type.IsSubclassOf(KnownType.AnimControllerType))
            {
                typename = KnownType.AnimControllerType.FullName;

                if (KnownType.Register.TryGetValue(typename, out info))
                {
                    return info;
                }
            }
            
            if (type.IsSubclassOf(KnownType.ComponentType))
            {
                typename = KnownType.ComponentType.FullName;

                if (KnownType.Register.TryGetValue(typename, out info))
                {
                    return info;
                }
            }

            if (type.IsSubclassOf(KnownType.ScriptableObjectType))
            {
                typename = KnownType.ScriptableObjectType.FullName;

                if (KnownType.Register.TryGetValue(typename, out info))
                {
                    return info;
                }
            }

            return null;
        }
    }

    public class KnownType<T> : KnownType
    {
        protected internal System.Func<GenericParameter, T> Getter;
        protected internal System.Action<GenericParameter, T> Setter;

        private readonly Dictionary<string, PropertyReference> _properties = new Dictionary<string, PropertyReference>();

        protected internal KnownType(string name)
        {
            GenericName = name;
            HoldType = typeof(T);
        }

        public override IValue CreateValue(GenericParameter param)
        {
            return new Value<T>(param.HoldType.Type, param.GetAs<T>());
        }

        protected internal override PropertyReference CreatePropertyForType<U>(string name)
        {
            return new PropertyReference<U, T>(name);
        }

        public override PropertyReference GetProperty(System.Type type, string name)
        {
            PropertyReference reference;
            if (_properties.TryGetValue(name, out reference))
            {
                var otherType = GetKnownType(type);
                if (otherType != null)
                {
                    reference = otherType.CreatePropertyForType<T>(name);
                    if (reference.IsValid())
                    {
                        _properties[name] = reference;
                    }
                    else
                    {
                        // no delete, let GC do it's job ;__;
                        reference = null;
                    }    
                }
            }

            return reference;
        }
    }
}