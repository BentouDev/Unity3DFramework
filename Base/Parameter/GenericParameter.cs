using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Framework.AI;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    [System.Serializable]
    public class GenericParameter : ISerializationCallbackReceiver
    {
        public GenericParameter()
        {
            _curve   = new AnimationCurve();
        }

        public GenericParameter(SerializedType type)
        {
            HoldType = type;
            _curve   = new AnimationCurve();
        }
        
        public GenericParameter(System.Type type)
        {
            HoldType = new SerializedType(type);
            _curve   = new AnimationCurve();
        }

        public static GenericParameter Copy(GenericParameter other)
        {
            var self = new GenericParameter(other.GetType());
            self.CopyFrom(other);
            return self;
        }

        public void CopyFrom(GenericParameter other)
        {
            Name     = other.Name;
            HoldType = other.HoldType;
            
            // By value
            _string  = string.Copy(other._string);
            _curve   = new AnimationCurve(other._curve.keys);
            _floats  = new float[4] { other._floats[0], other._floats[1], other._floats[2], other._floats[3] };
            
            // By reference
            _object  = other._object;
        }

        [SerializeField]
        public string Name;
        
        [SerializeField]
        public SerializedType HoldType;

        [SerializeField]
        private string _string;

        [SerializeField]
        private AnimationCurve _curve;

        [SerializeField]
        private Object _object;

        [SerializeField]
        private float[] _floats = new float[4];

        bool HasSameValue(GenericParameter other)
        {
            if (!HoldType.Equals(other.HoldType))
                return false;

            // TODO implement GenericParam comparison properly
            return true;
        }

        private static KnownType CreateKnownType<T>(string name, string propertyName)
        {
            PropertyInfo propertyInfo = typeof(T).GetProperty(propertyName);
            
            return new KnownType<T>(name)
            {
                Getter = PropertyReference<GenericParameter, T>.BuildPropertyGetter(propertyInfo),
                Setter = PropertyReference<GenericParameter, T>.BuildPropertySetter(propertyInfo),
            };
        }

        public static void BuildKnownTypeList()
        {
            KnownType.InsertKnownType
            (
                new KnownType<string>("string")
                {
                    Getter = parameter => parameter._string,
                    Setter = (parameter, value) => { parameter._string = value; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<bool>("bool")
                {
                    Getter = parameter => parameter._floats[0] > 0,
                    Setter = (parameter, value) => { parameter._floats[0] = value ? 1 : -1; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<int>("int")
                {
                    Getter = parameter => Mathf.RoundToInt(parameter._floats[0]),
                    Setter = (parameter, value) => { parameter._floats[0] = value; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<float>("float")
                {
                    Getter = parameter => parameter._floats[0],
                    Setter = (parameter, value) => { parameter._floats[0] = value; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<AnimationCurve>("Animation Curve")
                {
                    Getter = parameter => parameter._curve,
                    Setter = (parameter, curve) => { parameter._curve = curve; }
                }
            );
            
            KnownType.InsertKnownType
            (
                new KnownType<RuntimeAnimatorController>("Animator Controller")
                {
                    Getter = parameter => parameter._object as RuntimeAnimatorController,
                    Setter = (parameter, controller) => { parameter._object = controller; }
                }
            );
            
            KnownType.InsertKnownType
            (
                new KnownType<Color>("Color")
                {
                    Getter = parameter => new Color(parameter._floats[0], parameter._floats[1], parameter._floats[2], parameter._floats[3]),
                    Setter = (parameter, color) => { parameter._floats[0] = color.r; parameter._floats[1] = color.g; parameter._floats[2] = color.b; parameter._floats[3] = color.a; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<Vector2>("Vector2")
                {
                    Getter = parameter => new Vector2(parameter._floats[0], parameter._floats[1]),
                    Setter = (parameter, vector2) => { parameter._floats[0] = vector2.x; parameter._floats[1] = vector2.y; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<Vector3>("Vector3")
                {
                    Getter = parameter => new Vector3(parameter._floats[0], parameter._floats[1], parameter._floats[2]),
                    Setter = (parameter, vector3) => { parameter._floats[0] = vector3.x; parameter._floats[1] = vector3.y; parameter._floats[2] = vector3.z; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<GameObject>("GameObject")
                {
                    Getter = parameter => parameter._object as GameObject,
                    Setter = (parameter, value) => { parameter._object = value; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<SerializedType>("System.Type")
                {
                    Getter = parameter => new SerializedType(parameter._string),
                    Setter = (parameter, type) => { parameter._string = type.SerializedTypeName; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<DerivedType>("Derivatives of System.Type")
                {
                    Getter = parameter => new DerivedType(parameter._string),
                    Setter = (parameter, type) => { parameter._string = type.ToString(); }
                }
            );
            
            KnownType.InsertKnownType
            (
                new KnownType<Component>("MonoBehaviour...")
                {
                    Getter = parameter => parameter._object as Component,
                    Setter = (parameter, value) => { parameter._object = value; }
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<ScriptableObject>("ScriptableObject...")
                {
                    Getter = parameter => parameter._object as ScriptableObject,
                    Setter = (parameter, value) => { parameter._object = value; }
                }
            );
        }

        private void TypeGuard(System.Type type)
        {
            if (type != HoldType.Type && !HoldType.Type.IsSubclassOf(type))
            {
                throw new System.InvalidOperationException (
                    string.Format("Unable to use type '{0}' on parameter of type {1}!", 
                    type.FullName, HoldType.Type.FullName)
                );
            }
        }

        public IValue CreateValue()
        {
#if UNITY_EDITOR
            var knownType = KnownType.GetKnownType(HoldType.Type);
            if (knownType != null)
            {
                return knownType.CreateValue(this);
            }
            else
            {
                Debug.LogErrorFormat("Known type for {0} not found! Unable to crate IValue.", HoldType.Type);
                return null;
            }
#else
            return GetKnownType(HoldType.Type).CreateValue(this);
#endif
        }

        public void OnBeforeSerialize()
        {
            
        }

        public void OnAfterDeserialize()
        {
            
        }
        
        public void SetAs<T>(T value)
        {
            var type = typeof(T);
            TypeGuard(type);

            ((KnownType<T>)KnownType.Register[type.FullName]).Setter(this, value);
        }

        public T GetAs<T>(bool ommitCache = false)
        {
            var type = typeof(T);
            TypeGuard(type);

            return ((KnownType<T>)KnownType.Register[type.FullName]).Getter(this);
        }

        public object Get()
        {
            if (HoldType.Type == null || string.IsNullOrEmpty(HoldType.Type.FullName))
                return null;
            
            if (KnownType.Register.TryGetValue(HoldType.Type.FullName, out var knownType))
            {
                return knownType.CallGetter(this);    
            }

            return null;
        }

        public void Set(object value)
        {
            if (HoldType.Type == null || string.IsNullOrEmpty(HoldType.Type.FullName))
                return;

            if (KnownType.Register.TryGetValue(HoldType.Type.FullName, out var knownType))
            {
                knownType.CallSetter(this, value);
            }
        }

#if UNITY_EDITOR
        public static void Layout(GenericParameter parameter, bool label)
        {
            var typename = parameter.HoldType.Type.FullName;

            KnownType info;
            if(KnownType.Register.TryGetValue(typename, out info) && info.LayoutFunc != null)
            {
                info.LayoutFunc(parameter, label);
            }

            if (parameter.HoldType.Type.IsSubclassOf(KnownType.AnimControllerType)
            && KnownType.Register.TryGetValue(KnownType.AnimControllerType.FullName, out info)
            && info.LayoutFunc != null)
            {
                info.LayoutFunc(parameter, label);
            }
            
            if (parameter.HoldType.Type.IsSubclassOf(KnownType.ComponentType)
            && KnownType.Register.TryGetValue(KnownType.ComponentType.FullName, out info)
            && info.LayoutFunc != null)
            {
                info.LayoutFunc(parameter, label);
            }

            if (parameter.HoldType.Type.IsSubclassOf(KnownType.ScriptableObjectType)
            && KnownType.Register.TryGetValue(KnownType.ScriptableObjectType.FullName, out info)
            && info.LayoutFunc != null)
            {
                info.LayoutFunc(parameter, label);
            }
        }

        public static void Draw(Rect drawRect, GenericParameter parameter, bool label)
        {
            if (parameter.HoldType.Type != null)
            {
                var typename = parameter.HoldType.Type.FullName;

                KnownType info;
                if (KnownType.Register.TryGetValue(typename, out info) && info.DrawFunc != null)
                {
                    info.DrawFunc(drawRect, parameter, label);
                    return;
                }

                if (parameter.HoldType.Type.IsSubclassOf(KnownType.AnimControllerType)
                    && KnownType.Register.TryGetValue(KnownType.AnimControllerType.FullName, out info)
                    && info.DrawFunc != null)
                {
                    info.DrawFunc(drawRect, parameter, label);
                    return;
                }
                
                if (parameter.HoldType.Type.IsSubclassOf(KnownType.ComponentType)
                    && KnownType.Register.TryGetValue(KnownType.ComponentType.FullName, out info)
                    && info.DrawFunc != null)
                {
                    info.DrawFunc(drawRect, parameter, label);
                    return;
                }

                if (parameter.HoldType.Type.IsSubclassOf(KnownType.ScriptableObjectType)
                    && KnownType.Register.TryGetValue(KnownType.ScriptableObjectType.FullName, out info) 
                    && info.DrawFunc != null)
                {
                    info.DrawFunc(drawRect, parameter, label);
                    return;
                }    
            }
            
            EditorGUI.LabelField(drawRect, new GUIContent(parameter.Name));
        }
#endif

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Name != null ? Name.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (HoldType != null ? HoldType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_string != null ? _string.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_curve != null ? _curve.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_object != null ? _object.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (_floats != null ? _floats.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
