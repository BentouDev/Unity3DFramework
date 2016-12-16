using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Works as a memory for AI
/// </summary>
public class Blackboard : MonoBehaviour
{
    public class Required : PropertyAttribute
    {
        public string KeyName;
    }

    public interface IValue
    {
#if UNITY_EDITOR
        System.Type GetValueType();
#endif
    }

    public class Value<T> : IValue
    {
        T value;

#if UNITY_EDITOR
        private static System.Type _type;

        static Value()
        {
            _type = typeof(T);
        }

        public System.Type GetValueType()
        {
            return _type;
        }
#endif

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
    public Dictionary<string, IValue> Values = new Dictionary<string, IValue>();

    public bool HasValue<T>(string name)
    {
        IValue value;
        if (Values.TryGetValue(name, out value))
        {
            return value is Value<T>;
        }

        return false;
    }

    public T GetValue<T>(string name)
    {
        IValue value;
        if (Values.TryGetValue(name, out value))
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
        if (Values.TryGetValue(name, out value))
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
