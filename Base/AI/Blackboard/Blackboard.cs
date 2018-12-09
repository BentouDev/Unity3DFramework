using System;
using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Framework.AI
{
    /// <summary>
    /// Works as a memory for AI
    /// </summary>
    public class Blackboard : Framework.BaseBehaviour, IDataSet
    {
        [System.AttributeUsage(System.AttributeTargets.Property)]
        public class Required : PropertyAttribute { }

        public DataSet Data;

        public void Init()
        {
            Data.Init();
        }

        public void InsertFromParameter(GenericParameter parameter)
        {
            Data.InsertFromParameter(parameter);
        }

        public bool GetFromParameter(GenericParameter parameter)
        {
            if (!Data.GetFromParameter(parameter))
            {
                Debug.LogError(string.Format("No key '{0}' of type '{1}' in Blackboard", parameter.Name, parameter.HoldType.Type.Name), this);
                return false;
            }

            return true;
        }

        public bool SetToParameter(GenericParameter parameter)
        {
            if (!Data.SetToParameter(parameter))
            {
                Debug.LogError(string.Format("No key '{0}' of type '{1}' in Blackboard", parameter.Name, parameter.HoldType.Type.Name), this);
                return false;
            }

            return true;
        }

        public bool HasValue(Type type, string name)
        {
            return Data.HasValue(type, name);
        }

        public bool HasValue<T>(string name)
        {
            return Data.HasValue<T>(name);
        }

        public T GetValue<T>(string name)
        {
            return Data.GetValue<T>(name);
        }

        public void SetValue<T>(string name, T newValue)
        {
            Data.SetValue<T>(name, newValue);
        }
    }
}
