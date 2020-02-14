using System;
using System.Collections.Generic;
using Framework.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace Framework
{
    public interface IValue
    {
        System.Type GetValueType();

        void SetTo(Variant parameter);

        void GetFrom(Variant parameter);
    }

    public class Value<T> : IValue
    {
        private readonly System.Type _type;

        private T value;

        public Value(System.Type storedType)
        {
            _type = storedType;
        }

        public Value(System.Type storedType, T defValue)
        {
            _type = storedType;
            value = defValue;
        }

        public System.Type GetValueType()
        {
            return _type;
        }

        public void SetTo(Variant parameter)
        {
            parameter.SetAs<T>(value);
        }

        public void GetFrom(Variant parameter)
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

        public override string ToString()
        {
            return value != null ? value.ToString() : "null";
        }
    }

    [Serializable]
    public class DataBank : IDataSet, ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        private Dictionary<string, int> Index = new Dictionary<string, int>();

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("Values")]
        private List<Parameter> Serialized = new List<Parameter>();

        [HideInInspector]
        public List<IValue> Runtime;

        public bool HasRuntime => Runtime != null;

        public void OnBeforeSerialize()
        {
            if (HasRuntime)
            {
                List<string> Names = Serialized.ConvertAll(p => p.Name);
                Serialized.Clear();
                for (int i = 0; i < Runtime.Count; i++)
                {
                    var param = new Parameter(Runtime[i].GetValueType(), Names[i]);
                    Runtime[i].SetTo(param.Value);

                    Serialized.Add(param);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            RebuildIndex();

            foreach (Parameter parameter in Serialized)
            {
                InsertFromParameter(parameter, false);
            }
        }
        
        public List<Parameter> GetSerialized()
        {
            return Serialized;
        }

        public List<Pair<string, IValue>> GetPairs()
        {
            List<Pair<string, IValue>> pairs = new List<Pair<string, IValue>>();
            if (HasRuntime)
            {
                for (int i = 0; i < Runtime.Count; i++)
                {
                    pairs.Add(new Pair<string, IValue>(Serialized[i].Name, Runtime[i]));
                }
            }
            else
            {
                for (int i = 0; i < Serialized.Count; i++)
                {
                    IValue val = Serialized[i].Value.CreateValue();
                    val.GetFrom(Serialized[i].Value);
                    pairs.Add(new Pair<string, IValue>(Serialized[i].Name, val));
                }
            }

            return pairs;
        }
        
        public void SetValues(List<Parameter> values)
        {
            Serialized = values;
        }

        private int GetIndexOf(string name)
        {
            int index;
            return (Index.TryGetValue(name, out index)) ? index : -1;
        }

        private void RebuildIndex()
        {
            Index.Clear();
            for (int i = 0; i < Serialized.Count; i++)
            {
                if (Serialized[i] != null && !string.IsNullOrEmpty(Serialized[i].Name))
                    Index[Serialized[i].Name] = i;
                else 
                    Debug.LogError($"DataBank '{this}' has property at index {i} serialized as null!");
            }
        }

        public void Init()
        {
            RebuildIndex();
        }

        public void InsertFromParameter(Parameter parameter)
        {
            InsertFromParameter(parameter, true);
        }

        private void InsertFromParameter(Parameter parameter, bool insertIfMissing)
        {
            if (parameter == null || string.IsNullOrEmpty(parameter.Name))
                return;

            int index = GetIndexOf(parameter.Name);
            if (index == -1)
            {
                if (HasRuntime)
                {
                    index = Runtime.Count;
                    IValue val = parameter.Value.CreateValue();
                    val.GetFrom(parameter.Value);
                    Runtime.Add(val);
                }
                else if (insertIfMissing)
                {
                    index = Serialized.Count;
                    Serialized.Add(parameter);
                }

                Index[parameter.Name] = index;
            }
        }

        public bool GetFromParameter(Variant parameter)
        {
            int index = GetIndexOf(parameter.Name);
            if (index != -1)
            {
                if (HasRuntime)
                {
                    if (Runtime[index].GetValueType() == parameter.HoldType.Type)
                    {
                        Runtime[index].GetFrom(parameter);
                        return true;
                    }
                }
                else
                {
                    if (Serialized[index].GetHoldType() == parameter.HoldType)
                    {
                        Serialized[index].Value.Set(parameter.Get());
                        return true;                        
                    }
                }
            }

            return false;
        }

        public bool SetToParameter(Variant parameter)
        {
            int index = GetIndexOf(parameter.Name);
            if (index != -1)
            {
                if (HasRuntime)
                {
                    if (Runtime[index].GetValueType() == parameter.HoldType.Type)
                    {
                        Runtime[index].SetTo(parameter);
                        return true;
                    }
                }
                else
                {
                    if (Serialized[index].GetHoldType() == parameter.HoldType)
                    {
                        parameter.CopyFrom(Serialized[index].Value);
                        return true;
                    }
                }
            }

            return false;
        }

        public bool HasValue(System.Type type, string name)
        {
            int index = GetIndexOf(name);
            if (index != -1)
            {
                if (HasRuntime)
                    return Runtime[index].GetValueType() == type;
                else
                    // ToDo: remove this allocation
                    return Serialized[index].GetHoldType().Equals(new SerializedType(type));
            }

            return false;
        }

        public bool HasValue<T>(string name)
        {
            int index = GetIndexOf(name);
            if (index != -1)
            {
                if (HasRuntime)
                    return Runtime[index] is Value<T>;
                else
                    // ToDo: remove this allocation
                    return Serialized[index].GetHoldType().Equals(new SerializedType(typeof(T)));
            }

            return false;
        }

        public T GetValue<T>(string name)
        {
            int index = GetIndexOf(name);
            if (index != -1)
            {
                IValue value;

                if (HasRuntime)
                {
                    value = Runtime[index];
                }
                else
                {
                    value = Serialized[index].Value.CreateValue();
                    value.GetFrom(Serialized[index].Value);
                }
                
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                Value<T> valueT = value as Value<T>;
                if (valueT != null)
                {
                    return valueT.Get();
                }
                else
                {
                    Debug.LogErrorFormat("Parameter name {0} is not of type {1} in blackboard {2}", name, typeof(T).Name);
                }
#else
                return ((Value<T>)value).Get();
#endif                
            }

            return default(T);
        }

        public void SetValue<T>(string name, T newValue)
        {
            int index = GetIndexOf(name);
            if (index != -1)
            {
                if (HasRuntime)
                {
                    IValue value = Runtime[index];
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    Value<T> valueT = value as Value<T>;
                    if (valueT != null)
                    {
                        valueT.Set(newValue);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Parameter name {0} is not of type {1} in dataset {2}", name, typeof(T).Name, this);
                    }
#else
                    ((Value<T>)value).Value.Set(newValue);
#endif
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (Serialized[index].GetHoldType().Equals(new SerializedType(typeof(T))))
                    {
                        Serialized[index].Value.SetAs<T>(newValue);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Parameter name {0} is not of type {1} in dataset {2}", name, typeof(T).Name, this);
                    }
#else
                    Serialized[index].Value.SetAs<T>(newValue);
#endif
                }
            }
        }
    }
}