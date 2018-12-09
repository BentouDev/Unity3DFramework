using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using Framework.Utils;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "New Data Set", menuName = "Data/Data Set")]
#endif
    public class DataSet : ScriptableObject, IDataSet// , ISerializationCallbackReceiver
    {
        [SerializeField]
        [HideInInspector]
        public DataBank Bank = new DataBank();

        public void Init()
        {
            Bank.Init();
        }

        public void InsertFromParameter(GenericParameter parameter)
        {
            Bank.InsertFromParameter(parameter);
        }

        public bool GetFromParameter(GenericParameter parameter)
        {
            return Bank.GetFromParameter(parameter);
        }

        public bool SetToParameter(GenericParameter parameter)
        {
            return Bank.SetToParameter(parameter);
        }

        public bool HasValue(Type type, string name)
        {
            return Bank.HasValue(type, name);
        }

        public bool HasValue<T>(string name)
        {
            return Bank.HasValue<T>(name);
        }

        public T GetValue<T>(string name)
        {
            return Bank.GetValue<T>(name);
        }

        public void SetValue<T>(string name, T newValue)
        {
            Bank.SetValue<T>(name, newValue);
        }
    }
}