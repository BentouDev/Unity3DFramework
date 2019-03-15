using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using UnityEngine;

namespace Framework
{
    public interface IPropertyOwner
    {
        void CallSetter(PropertyReference property, Variant param);
        void CallGetter(PropertyReference property, Variant param);
    }

    public class PropertyOwner<T> : IPropertyOwner
    {
        private T _instance;
        
        public PropertyOwner(T instance)
        {
            _instance = instance;
        }

        public void CallSetter(PropertyReference property, Variant param)
        {
            property.GetFromParameter(_instance, param);
        }

        public void CallGetter(PropertyReference property, Variant param)
        {
            property.SetToParameter(_instance, param);
        }
    }

    [System.Serializable]
    public class ParametrizedProperty
    {
        private IPropertyOwner Owner;
        
        [SerializeField]
        private PropertyReference Property;

        [SerializeField]
        public ParameterReference Parameter;

        private Variant _localValue;

        [SerializeField]
        public bool Constant;

        public void Initialize<TOwner>(TOwner instance, IDataSetProvider provider, string propName)
        {
            if (Owner != null && Property != null)
                return;

            Owner = new PropertyOwner<TOwner>(instance);

            var parameter = Parameter.Get(provider);
            var knownType = KnownType.GetKnownType(typeof(TOwner));
            if (knownType != null)
            {
                bool isParametrized = parameter != null && !Constant;
                var type = isParametrized ? parameter.GetHoldType().Type : _localValue.HoldType.Type;
                Property = knownType.GetProperty<TOwner>(type, propName);

                if (_localValue == null)
                    _localValue = new Variant(type);

                if (Constant)
                    Owner.CallSetter(Property, isParametrized ? parameter.Value : _localValue);
            }
        }

        public void UpdateFromProvider(IDataSetProvider provider)
        {
            provider.SetToVariant(Parameter, _localValue);

            if (Property != null)
                Owner.CallSetter(Property, _localValue);
        }

        public void SetToProvider(IDataSetProvider provider)
        {
            if (Property != null)
                Owner.CallGetter(Property, _localValue);

            provider.SetFromVariant(Parameter, _localValue);
        }

        public void SetValue(Variant value)
        {
            if (Owner != null)
                Owner.CallSetter(Property, value);
            else
            {
                if (_localValue == null)
                    _localValue = new Variant(value.HoldType);

                _localValue.Set(value.Get());
            }
        }

        public void GetValue(Variant variant)
        {
            if (Owner != null)
                Owner.CallGetter(Property, variant);
            else
            {
                if (_localValue == null)
                    _localValue = new Variant(variant.HoldType);

                variant.Set(_localValue.Get());
            }
        }
    }

    [System.Serializable]
    public abstract class PropertyReference
    {
        public abstract bool IsValid();
        public abstract void SetToParameter  <T>(T instance, Variant parameter);
        public abstract void GetFromParameter<T>(T instance, Variant parameter);
    }

    [System.Serializable]
    public class PropertyReference<T, F> : PropertyReference
    {
        public System.Func<T, F> Getter;
        public System.Action<T, F> Setter;
    
        private readonly System.Reflection.PropertyInfo _propertyInfo;
        private readonly System.Reflection.FieldInfo _fieldInfo;
    
        public override bool IsValid()
        {
            return Getter != null && Setter != null;
        }
    
        public PropertyReference(string propertyName)
        {
            _propertyInfo = typeof(T).GetProperty(propertyName);
            _fieldInfo = typeof(T).GetField(propertyName);
    
            if (_propertyInfo != null)
            {
                Getter = BuildPropertyGetter(_propertyInfo);
                Setter = BuildPropertySetter(_propertyInfo);
            }
            else if (_fieldInfo != null)
            {
                Getter = BuildFieldGetter(_fieldInfo);
                Setter = BuildFieldSetter(_fieldInfo);
            }
            else
            {
                Debug.LogError($"Unable to create PropertyReference, no property nor field named {propertyName}");
            }
        }
    
        public static System.Func<T, F> BuildFieldGetter(System.Reflection.FieldInfo fieldInfo)
        {
            var obj = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
            var field = System.Linq.Expressions.Expression.Field(obj, fieldInfo);
            var castField = System.Linq.Expressions.Expression.Convert(field, fieldInfo.FieldType);
            return System.Linq.Expressions.Expression.Lambda<System.Func<T, F>>(castField, obj).Compile();
        }
    
        public static System.Action<T, F> BuildFieldSetter(System.Reflection.FieldInfo fieldInfo)
        {
            var obj = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
            var param = System.Linq.Expressions.Expression.Parameter(typeof(F), "parameter");
            var field = System.Linq.Expressions.Expression.Field(obj, fieldInfo);

            var castParam = System.Linq.Expressions.Expression.Convert(param, fieldInfo.FieldType);
    
            var assignment = System.Linq.Expressions.Expression.Assign(field, castParam);
    
            return System.Linq.Expressions.Expression.Lambda<System.Action<T, F>>(assignment, obj, param).Compile();
        }
    
        public static System.Func<T, F> BuildPropertyGetter(System.Reflection.PropertyInfo propertyInfo)
        {
            return (System.Func<T, F>) System.Delegate.CreateDelegate
            (
                typeof(System.Func<T, F>),
                propertyInfo.GetGetMethod()
            );
        }
    
        public static System.Action<T, F> BuildPropertySetter(System.Reflection.PropertyInfo propertyInfo)
        {
            var typedMi = propertyInfo.GetSetMethod();
            var obj = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
            var param = System.Linq.Expressions.Expression.Parameter(typeof(F), "parameter");
    
            var castParam = System.Linq.Expressions.Expression.Convert(param, propertyInfo.PropertyType);
    
            var call = System.Linq.Expressions.Expression.Call(obj, typedMi, castParam);
    
            return System.Linq.Expressions.Expression.Lambda<System.Action<T, F>>(call, obj, param).Compile();
        }
    
    //        public static System.Action<T, F> CreateSetter(System.Reflection.PropertyInfo propertyInfo)
    //        {
    //            var instance  = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
    //            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(F), "param");
    //
    //            var cast = System.Linq.Expressions.Expression.TypeAs(parameter, propertyInfo.PropertyType);
    //
    //            var methodCall = System.Linq.Expressions.Expression.Call(instance, propertyInfo.GetSetMethod(), cast);
    //            
    //            var typeAs = System.Linq.Expressions.Expression.Convert(methodCall, typeof(F));
    //            
    //            return System.Linq.Expressions.Expression.Lambda<System.Action<T, F>>(typeAs, instance, parameter).Compile();
    //        }
    
        public override void GetFromParameter<U>(U instance, Variant parameter)
        {
            Setter((T)(object) instance, parameter.GetAs<F>());
        }
    
        public override void SetToParameter<U>(U instance, Variant parameter)
        {
            parameter.SetAs(Getter((T)(object) instance));
        }
    }
}
