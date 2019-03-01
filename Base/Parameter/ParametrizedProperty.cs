using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using UnityEngine;

namespace Framework
{
    public interface IPropertyOwner
    {
        void CallSetter(PropertyReference property, GenericParameter param);
        void CallGetter(PropertyReference property, GenericParameter param);
    }

    public class PropertyOwner<T> : IPropertyOwner
    {
        private T _instance;
        
        public PropertyOwner(T instance)
        {
            _instance = instance;
        }

        public void CallSetter(PropertyReference property, GenericParameter param)
        {
            property.GetFromParameter(_instance, param);
        }

        public void CallGetter(PropertyReference property, GenericParameter param)
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
        public GenericParameter Parameter;
        
        [SerializeField]
        public bool Constant;

        public void Initialize<TOwner>(TOwner instance, string propName)
        {
            Owner = new PropertyOwner<TOwner>(instance);
            
            var knownType = KnownType.GetKnownType(typeof(TOwner));
            if (knownType != null)
            {
                Property = knownType.GetProperty(Parameter.HoldType.Type, propName);
                
                if (Constant)
                    Owner.CallSetter(Property, Parameter);
            }
        }
        
        /*public void GetProperty<T>(T instance, string name)
        {
            var knownType = KnownType.GetKnownType(Parameter.HoldType.Type);
            if (knownType != null)
            {
                Property = knownType.CreateProperty(name);
            }
            
            Property = Parameter.CreatePropertyReference(instance, name);
        }

        public PropertyReference CreatePropertyReference<T>(T instance, string name)
        {
            var knownType = KnownType.GetKnownType(HoldType.Type);
            if (knownType != null)
            {
                return knownType.CreateProperty(instance, name);
            }
            else
            {
                Debug.LogErrorFormat("Known type for {0} not found! Unable to crate IValue.", HoldType.Type);
                return null;
            }
        }

        protected PropertyReference GetProperty<T,U>(string propName)
        {
            if (Property != null)
                return Property;

            var knownType = KnownType.GetKnownType(typeof(T));
            if (knownType != null)
            {
                var propRef = knownType.GetProperty<U>(propName);
                if (propRef != null)
                {
                    Property = propRef;
                }
            }

            return Property;
        }*/

        public void UpdateFromProvider(IDataSetProvider provider)
        {
            provider.SetToParameter(Parameter);
            
            if (Property != null)
                Owner.CallSetter(Property, Parameter);
        }

        public void SetToProvider(IDataSetProvider provider)
        {
            if (Property != null)
                Owner.CallGetter(Property, Parameter);
            
            provider.GetFromParameter(Parameter);
        }
    }

    [System.Serializable]
    public abstract class PropertyReference
    {
        public abstract bool IsValid();
        public abstract void SetToParameter  <T>(T instance, GenericParameter parameter);
        public abstract void GetFromParameter<T>(T instance, GenericParameter parameter);
    }

    [System.Serializable]
    public class PropertyReference<T, F> : PropertyReference// where T : class
    {
        public System.Func<T, F> Getter;
        public System.Action<T, F> Setter;
    
        // private readonly T _instance;
        private readonly System.Reflection.PropertyInfo _propertyInfo;
        private readonly System.Reflection.FieldInfo _fieldInfo;
    
        public override bool IsValid()
        {
            return Getter != null && Setter != null;
        }
    
        public PropertyReference(string propertyName)
        {
            // _instance = instance;
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
            return System.Linq.Expressions.Expression.Lambda<System.Func<T, F>>(field, obj).Compile();
        }
    
        public static System.Action<T, F> BuildFieldSetter(System.Reflection.FieldInfo fieldInfo)
        {
            var obj = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
            var param = System.Linq.Expressions.Expression.Parameter(typeof(F), "parameter");
            var field = System.Linq.Expressions.Expression.Field(obj, fieldInfo);
    
            var assignment = System.Linq.Expressions.Expression.Assign(field, param);
    
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
    
        public override void GetFromParameter<U>(U instance, GenericParameter parameter)
        {
            Setter((T)(object) instance, parameter.GetAs<F>());
        }
    
        public override void SetToParameter<U>(U instance, GenericParameter parameter)
        {
            parameter.SetAs(Getter((T)(object) instance));
        }
    }
}
