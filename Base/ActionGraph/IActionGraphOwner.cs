using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public enum BindingType
    {
        Unbound,
        Value,
        DataSetVariable,
        LocalProperty
    }

    [System.Serializable]    
    public class ParamBinding
    {
        [SerializeField]
        private BindingType _type;

        public BindingType Type => _type;

        [SerializeField]
        public ParameterReference Param;

        [SerializeField]
        public Variant DefaultValue;

//        [SerializeField]
//        public PropertyReference Property;
//
//        [SerializeField]
//        public ParameterReference DataSetVar;

        [SerializeField]
        public Object Context;

        public IDataSet ContextAsDataSet => Context as IDataSet;

        public void SetBindingType(BindingType type, IDataSetProvider provider)
        {
            OnBindingTypeChanged(type, provider);
        }

        private void OnBindingTypeChanged(BindingType newType, IDataSetProvider provider)
        {
            if (_type == newType)
                return;

            var boundParam = Param.Get(provider);
            switch (newType)
            {
                case BindingType.Value:
                    if (DefaultValue != null)
                        DefaultValue.HoldType = boundParam.GetHoldType();
                    else
                        DefaultValue = new Variant(boundParam.GetHoldType());

                    break;
//                case BindingType.DataSetVariable:
//                    DataSetVar = new ParameterReference();
//                    break;
            }

            _type = newType;
        }
    }

    public interface IParameterBinder
    {
        List<ParamBinding> GetSyncList(List<Parameter> parameters);
    }

    [System.Serializable]
    public class ParameterBinder : IParameterBinder
    {
        [SerializeField]
        public List<ParamBinding> Bindings = new List<ParamBinding>();

        public List<ParamBinding> GetSyncList(List<Parameter> parameters)
        {
            if (parameters == null)
                return null;

            SyncList(parameters);

            return Bindings;
        }

        private void SyncList(List<Parameter> parameters)
        {
            RemoveUnused(parameters);
            FillMissing(parameters);
        }

        private void FillMissing(List<Parameter> parameters)
        {
            foreach (var parameter in parameters)
            {
                if (Bindings.All(b => b.Param.ParameterId != parameter.Id))
                {
                    Bindings.Add(new ParamBinding()
                    {
                        Param = parameter.CreateReference()
                    });
                }
            }
        }

        private void RemoveUnused(List<Parameter> parameters)
        {
            for (int i = 0; i < Bindings.Count;)
            {
                var binding = Bindings[i];
                if (parameters.All(p => p.Id != binding.Param.ParameterId))
                {
                    Bindings.RemoveAt(i);
                }
                else
                {
                    i++;
                }
            }
        }
    }

    public interface IActionGraphOwner
    {
        BindingContext GetBindingContext(ActionGraph graph);
        bool IsOwnerOf(ActionGraph graph);
    }

    public struct BindingContext
    {
        [NotNull]
        public readonly IParameterBinder Binder;

        [NotNull]
        public readonly IDataSetProvider Provider;

        public BindingContext([NotNull] IParameterBinder binder, [NotNull] IDataSetProvider provider)
        {
            Provider = provider;
            Binder = binder;
        }
    }
}