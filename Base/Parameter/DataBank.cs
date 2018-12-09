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

        public Value(System.Type storedType, T defValue)
        {
            _type = storedType;
            value = defValue;
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

        public override string ToString()
        {
            return value != null ? value.ToString() : "null";
        }
    }

    [Serializable]
    public class DataBank : IDataSet
    {
        [SerializeField]
        [HideInInspector]
        private Dictionary<string, int> Index = new Dictionary<string, int>();

        [SerializeField]
        [HideInInspector]
        [FormerlySerializedAs("Values")]
        private List<GenericParameter> Serialized = new List<GenericParameter>();

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
                    var param = new GenericParameter(Runtime[i].GetValueType());
                    Runtime[i].SetTo(param);

                    param.Name = Names[i];
                    Serialized.Add(param);
                }
            }
        }

        public void OnAfterDeserialize()
        {
            RebuildIndex();

            foreach (GenericParameter parameter in Serialized)
            {
                InsertFromParameter(parameter);
            }
        }
        
        public List<GenericParameter> GetSerialized()
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
                    IValue val = Serialized[i].CreateValue();
                    val.GetFrom(Serialized[i]);
                    pairs.Add(new Pair<string, IValue>(Serialized[i].Name, val));
                }
            }

            return pairs;
        }
        
        public void SetValues(List<GenericParameter> values)
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
                Index[Serialized[i].Name] = i;
            }
        }

        public void Init()
        {
            RebuildIndex();
        }

        public void InsertFromParameter(GenericParameter parameter)
        {
            int index = GetIndexOf(parameter.Name);
            if (index == -1)
            {
                if (HasRuntime)
                {
                    index = Runtime.Count;
                    IValue val = parameter.CreateValue();
                    val.GetFrom(parameter);
                    Runtime.Add(val);
                }
                else
                {
                    index = Serialized.Count;
                    Serialized.Add(parameter);
                }

                Index[parameter.Name] = index;
            }
        }

        public bool GetFromParameter(GenericParameter parameter)
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
                    if (Serialized[index].HoldType == parameter.HoldType)
                    {
                        Serialized[index] = parameter;
                        return true;                        
                    }
                }
            }

            return false;
        }

        public bool SetToParameter(GenericParameter parameter)
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
                    if (Serialized[index].HoldType == parameter.HoldType)
                    {
                        parameter.CopyFrom(Serialized[index]);
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
                    return Serialized[index].HoldType.Equals(new SerializedType(type));
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
                    return Serialized[index].HoldType.Equals(new SerializedType(typeof(T)));
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
                    value = Serialized[index].CreateValue();
                    value.GetFrom(Serialized[index]);
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
                    ((Value<T>)value).Set(newValue);
#endif
                }
                else
                {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                    if (Serialized[index].HoldType.Equals(new SerializedType(typeof(T))))
                    {
                        Serialized[index].SetAs<T>(newValue);
                    }
                    else
                    {
                        Debug.LogErrorFormat("Parameter name {0} is not of type {1} in dataset {2}", name, typeof(T).Name, this);
                    }
#else
                    Serialized[index].SetAs<T>(newValue);
#endif
                }
            }
        }
    }
}