using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

/// <summary>
/// Works as a memory for AI
/// </summary>
public class Blackboard : MonoBehaviour
{
    public class OfTypeAttribute : PropertyAttribute
    {
        public readonly Type RequiredType;
        
        public OfTypeAttribute(Type type)
        {
            RequiredType = type;
        }
    }

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

    // TODO : try to make this faster, maybe hash of type?
    public bool HasValue(System.Type type, string name)
    {
        IValue value;
        if(Values.TryGetValue(name, out value))
        {
            return value.GetValueType() == type;
        }

        return false;
    }

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

    /*public void SetToParameter(GenericParameter parameter)
    {
        IValue storedValue;
        if (Values.TryGetValue(parameter.Name, out storedValue))
        {
            switch (parameter.HoldTypeEnum)
            {
                case GenericParameter.ParameterType.Bool:
                    parameter.BoolValue = ((Value<bool>) storedValue).Get();
                    break;
                case GenericParameter.ParameterType.Float:
                    parameter.FloatValue = ((Value<float>) storedValue).Get();
                    break;
                case GenericParameter.ParameterType.Int:
                    parameter.IntValue = ((Value<int>) storedValue).Get();
                    break;
                case GenericParameter.ParameterType.Vec2:
                    parameter.Vec2Value = ((Value<Vector2>) storedValue).Get();
                    break;
                case GenericParameter.ParameterType.Vec3:
                    parameter.Vec3Value = ((Value<Vector3>) storedValue).Get();
                    break;
                case GenericParameter.ParameterType.GameObject:
                    parameter.ObjectValue = ((Value<GameObject>)storedValue).Get();
                    break;
                case GenericParameter.ParameterType.MonoBehaviour:
                    parameter.ObjectValue = ((Value<MonoBehaviour>) storedValue).Get();
                    break;
            }
        }
        else
        {
            Debug.LogError(string.Format("No key '{0}' of type '{1}' in Blackboard", parameter.Name, parameter.HoldType.Name), this);
        }
    }

    public void GetFromParameter(GenericParameter parameter)
    {
        if (HasValue(parameter.HoldType, parameter.Name))
        {

        }
        else
        {
            Debug.LogError(string.Format("No key '{0}' of type '{1}' in Blackboard", parameter.Name, parameter.HoldType.Name), this);
        }
    }*/
}
