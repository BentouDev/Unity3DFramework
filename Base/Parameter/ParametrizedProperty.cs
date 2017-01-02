using System.Collections;
using System.Collections.Generic;
using Framework.AI;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public struct ParametrizedProperty
    {
        [SerializeField]
        public PropertyReference Property;

        [SerializeField]
        public GenericParameter Parameter;
        
        [SerializeField]
        public bool Constant;

        public void CreateProperty<T>(T instance, string name)
        {
            Property = Parameter.CreatePropertyReference(instance, name);
        }
    }

    public abstract class PropertyReference
    {
        public abstract void SetToParameter  (GenericParameter parameter);
        public abstract void GetFromParameter(GenericParameter parameter);
    }

    public class PropertyReference<T, F> : PropertyReference
    {
        public System.Func  <T, F> Getter;
        public System.Action<T, F> Setter;

        private readonly T _instance;
        private readonly System.Reflection.PropertyInfo _propertyInfo;

        public PropertyReference(T instance, string propertyName)
        {
            _instance = instance;
            _propertyInfo = typeof(T).GetProperty(propertyName);//, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public);
            
            Debug.Log(string.Format("Creating PropertyReference for '{0}' in type '{1}' ", typeof(F).Name, typeof(T).Name));

            Getter = (System.Func  <T, F>) System.Delegate.CreateDelegate(typeof(System.Func  <T, F>), _propertyInfo.GetGetMethod());
            Setter = BuildSetter(_propertyInfo);// (System.Action<T, F>) System.Delegate.CreateDelegate(typeof(System.Action<T, F>), _propertyInfo.GetSetMethod());
        }
        
        private System.Action<T, F> BuildSetter(System.Reflection.PropertyInfo propertyInfo)
        {
            var typedMi = propertyInfo.GetSetMethod();
            var obj     = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
            var param   = System.Linq.Expressions.Expression.Parameter(typeof(F), "parameter");

            var castParam = System.Linq.Expressions.Expression.Convert(param, propertyInfo.PropertyType);

            var call = System.Linq.Expressions.Expression.Call(obj, typedMi, castParam);

            return System.Linq.Expressions.Expression.Lambda<System.Action<T, F>>(call, obj, param).Compile();

            var expr = System.Linq.Expressions.Expression.Lambda<System.Action<T, F>>(
                    System.Linq.Expressions.Expression.Convert(
                        System.Linq.Expressions.Expression.Call(
                            obj,//System.Linq.Expressions.Expression.Convert(param, typedMi.ReturnType),
                            typedMi,
                            System.Linq.Expressions.Expression.Convert(param, typedMi.ReturnType)
                        ),
                        typeof(F)
                    ),
                    obj, param
                );
            return expr.Compile();
        }

        private System.Action<T, F> CreateSetter(System.Reflection.PropertyInfo propertyInfo)
        {
            var instance  = System.Linq.Expressions.Expression.Parameter(typeof(T), "instance");
            var parameter = System.Linq.Expressions.Expression.Parameter(typeof(F), "param");// was property type

            var cast = System.Linq.Expressions.Expression.TypeAs(parameter, propertyInfo.PropertyType);

            var methodCall = System.Linq.Expressions.Expression.Call(instance, propertyInfo.GetSetMethod(), cast);//param

            //var cast = System.Linq.Expressions.Expression.Convert(methodCall, typeof(F));

            var typeAs = System.Linq.Expressions.Expression.Convert(methodCall, typeof(F));
            
            return System.Linq.Expressions.Expression.Lambda<System.Action<T, F>>(typeAs, instance, parameter).Compile();
        }

        public override void GetFromParameter(GenericParameter parameter)
        {
            Setter(_instance, parameter.GetAs<F>());
        }

        public override void SetToParameter(GenericParameter parameter)
        {
            parameter.SetAs(Getter(_instance));
        }
    }
}
