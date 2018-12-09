using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public class DataSetInstance : BaseBehaviour, IDataSet
    {
        [Header("Runtime")]
        public bool InitOnStart;

        private readonly List<IDataSet> Runtime = new List<IDataSet>();

        public enum Type
        {
            Instance,
            Scene,
            Asset
        }

        [Serializable]
        public struct DataLayer
        {
            [SerializeField]
            public Type Type;

            [SerializeField]
            public DataSetInstance LayeredSet;

            [SerializeField]
            public SceneDataSet SceneSet;

            [SerializeField]
            public DataSet AssetSet;
        }
        
        [Header("Layers")]
        [SerializeField]
        public List<DataLayer> Layers = new List<DataLayer>();

        void Start()
        {
            if (InitOnStart)
                Init();
        }

        public void Init()
        {
            foreach (DataLayer layer in Layers)
            {
                switch (layer.Type)
                {
                    case Type.Instance:
                        PushLayer(layer.LayeredSet);
                        break;
                    case Type.Scene:
                        PushLayer(layer.LayeredSet);
                        break;
                    case Type.Asset:
                        PushLayer(layer.AssetSet);
                        break;
                }
            }
        }

        public void PushLayer(IDataSet layer)
        {
            if (layer == null)
                return;

            layer.Init();
            Runtime.Insert(0, layer);
        }

        public void PopLayer(IDataSet layer)
        {
            if (layer == null)
                return;

            if (Runtime.FirstOrDefault() == layer)
            {
                Runtime.Remove(layer);
            }
            else
            {
                Debug.LogError($"Attempt to remove layer {layer} from DataSetInstance {this}", this);
            }
        }

        private IDataSet TopSet => Runtime.LastOrDefault();

        public void InsertFromParameter(GenericParameter parameter)
        {
            TopSet?.InsertFromParameter(parameter);
        }

        public bool GetFromParameter(GenericParameter parameter)
        {
            foreach (var set in Runtime)
            {
                if (set.HasValue(parameter.HoldType.Type, parameter.Name))
                {
                    return set.GetFromParameter(parameter);
                }
            }

            return false;
        }

        public bool SetToParameter(GenericParameter parameter)
        {
            foreach (var set in Runtime)
            {
                if (set.HasValue(parameter.HoldType.Type, parameter.Name))
                {
                    return set.SetToParameter(parameter);
                }
            }

            return false;
        }

        public bool HasValue(System.Type type, string name)
        {
            foreach (var set in Runtime)
            {
                if (set.HasValue(type, name))
                {
                    return true;
                }
            }

            return false;
        }

        public bool HasValue<T>(string name)
        {
            foreach (var set in Runtime)
            {
                if (set.HasValue<T>(name))
                {
                    return true;
                }
            }

            return false;
        }

        public T GetValue<T>(string name)
        {
            foreach (var set in Runtime)
            {
                if (set.HasValue<T>(name))
                {
                    return set.GetValue<T>(name);
                }
            }

            return default(T);
        }

        public void SetValue<T>(string name, T newValue)
        {
            foreach (var set in Runtime)
            {
                if (set.HasValue<T>(name))
                {
                    set.SetValue<T>(name, newValue);
                }
            }
        }
    }
}