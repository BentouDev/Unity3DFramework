using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Framework.AI
{
    /// <summary>
    /// Works as a memory for AI
    /// </summary>
    public class Blackboard : Framework.BaseBehaviour
    {
        [System.AttributeUsage(System.AttributeTargets.Property)]
        public class Required : PropertyAttribute { }

        public interface IValue
        {
            System.Type GetValueType();

            void SetTo(GenericParameter parameter);

            void GetFrom(GenericParameter parameter);
        }

        public class Value<T> : IValue
        {
            private readonly System.Type _type;

            private T value;

            public Value(System.Type storedType)
            {
                _type = storedType;
            }

            public System.Type GetValueType()
            {
                return _type;
            }

            public void SetTo(GenericParameter parameter)
            {
                parameter.SetAs<T>(value);
            }

            public void GetFrom(GenericParameter parameter)
            {
                value = parameter.GetAs<T>();
            }

            public T Get()
            {
                return value;
            }

            public void Set(T newValue)
            {
                value = newValue;
            }
        }

        // Naive implementation
        public Dictionary<string, IValue> Entries = new Dictionary<string, IValue>();

        public void InsertFromParameter(GenericParameter parameter)
        {
            if (!HasValue(parameter.HoldType.Type, parameter.Name))
            {
                Entries[parameter.Name] = parameter.CreateValue();
            }
        }

        public void GetFromParameter(GenericParameter parameter)
        {
            IValue storedValue;
            if (Entries.TryGetValue(parameter.Name, out storedValue) && storedValue.GetValueType() == parameter.HoldType.Type)
            {
                storedValue.GetFrom(parameter);
            }
            else
            {
                Debug.LogError(string.Format("No key '{0}' of type '{1}' in Blackboard", parameter.Name, parameter.HoldType.Type.Name), this);
            }
        }

        public void SetToParameter(GenericParameter parameter)
        {
            IValue storedValue;
            if (Entries.TryGetValue(parameter.Name, out storedValue) && storedValue.GetValueType() == parameter.HoldType.Type)
            {
                storedValue.SetTo(parameter);
            }
            else
            {
                Debug.LogError(string.Format("No key '{0}' of type '{1}' in Blackboard", parameter.Name, parameter.HoldType.Type.Name), this);
            }
        }

        public bool HasValue(System.Type type, string name)
        {
            IValue value;
            if (Entries.TryGetValue(name, out value))
            {
                return value.GetValueType() == type;
            }

            return false;
        }

        public bool HasValue<T>(string name)
        {
            IValue value;
            if (Entries.TryGetValue(name, out value))
            {
                return value is Value<T>;
            }

            return false;
        }

        public T GetValue<T>(string name)
        {
            IValue value;
            if (Entries.TryGetValue(name, out value))
            {
#if UNITY_EDITOR
                Value<T> valueT = value as Value<T>;
                if (valueT != null)
                {
                    return valueT.Get();
                }
                else
                {
                    Debug.LogErrorFormat("Parameter name {0} is not of type {1} in blackboard {2}", name, typeof(T).Name, this);
                }
#else
            return ((Value<T>)value).Get();
#endif
            }

            return default(T);
        }

        public void SetValue<T>(string name, T newValue)
        {
            IValue value;
            if (Entries.TryGetValue(name, out value))
            {
#if UNITY_EDITOR
                Value<T> valueT = value as Value<T>;
                if (valueT != null)
                {
                    valueT.Set(newValue);
                }
                else
                {
                    Debug.LogErrorFormat("Parameter name {0} is not of type {1} in blackboard {2}", name, typeof(T).Name, this);
                }
#else
            ((Value<T>)value).Set(newValue);
#endif
            }
        }
    }
}
