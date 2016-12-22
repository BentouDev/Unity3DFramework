using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.AI;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    [System.Serializable]
    public class GenericParameter
    {
        public GenericParameter(System.Type type)
        {
            HoldType = new SerializedType(type);
        }
        
        [SerializeField]
        public string Name;
        
        [SerializeField]
        public SerializedType HoldType;

        [SerializeField]
        private Object _object;

        [SerializeField]
        private float[] _floats = new float[4];

        public static void BuildKnownTypeList()
        {
            KnownType.InsertKnownType
            (
                new KnownType<bool>("bool")
                {
                    Getter = parameter => parameter._floats[0] > 0,
                    Setter = (parameter, value) => { parameter._floats[0] = value ? 1 : -1; },
                    ValueCreator = parameter => new Blackboard.Value<bool>(parameter.HoldType.Type)
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<int>("int")
                {
                    Getter = parameter => Mathf.RoundToInt(parameter._floats[0]),
                    Setter = (parameter, value) => { parameter._floats[0] = value; },
                    ValueCreator = parameter => new Blackboard.Value<int>(parameter.HoldType.Type)
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<float>("float")
                {
                    Getter = parameter => parameter._floats[0],
                    Setter = (parameter, value) => { parameter._floats[0] = value; },
                    ValueCreator = parameter => new Blackboard.Value<float>(parameter.HoldType.Type)
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<Vector2>("Vector2")
                {
                    Getter = parameter => new Vector2(parameter._floats[0], parameter._floats[1]),
                    Setter = (parameter, vector2) => { parameter._floats[0] = vector2.x; parameter._floats[1] = vector2.y; },
                    ValueCreator = parameter => new Blackboard.Value<Vector2>(parameter.HoldType.Type)
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<Vector3>("Vector3")
                {
                    Getter = parameter => new Vector3(parameter._floats[0], parameter._floats[1], parameter._floats[2]),
                    Setter = (parameter, vector3) => { parameter._floats[0] = vector3.x; parameter._floats[1] = vector3.y; parameter._floats[2] = vector3.z; },
                    ValueCreator = parameter => new Blackboard.Value<Vector3>(parameter.HoldType.Type)
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<GameObject>("GameObject")
                {
                    Getter = parameter => parameter._object as GameObject,
                    Setter = (parameter, value) => { parameter._object = value; },
                    ValueCreator = parameter => new Blackboard.Value<GameObject>(parameter.HoldType.Type)
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<Component>("MonoBehaviour...")
                {
                    Getter = parameter => parameter._object as Component,
                    Setter = (parameter, value) => { parameter._object = value; },
                    ValueCreator = parameter => new Blackboard.Value<Component>(parameter.HoldType.Type)
                }
            );

            KnownType.InsertKnownType
            (
                new KnownType<ScriptableObject>("ScriptableObject...")
                {
                    Getter = parameter => parameter._object as ScriptableObject,
                    Setter = (parameter, value) => { parameter._object = value; },
                    ValueCreator = parameter => new Blackboard.Value<ScriptableObject>(parameter.HoldType.Type)
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

        public PropertyReference CreatePropertyReference<T>(T instance, string name)
        {
            var knownType = GetKnownType(HoldType.Type);
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
        
        public Blackboard.IValue CreateValue()
        {
#if UNITY_EDITOR
            var knownType = GetKnownType(HoldType.Type);
            if (knownType != null)
            {
                return knownType.ValueCreator(this);
            }
            else
            {
                Debug.LogErrorFormat("Known type for {0} not found! Unable to crate IValue.", HoldType.Type);
                return null;
            }
#else
            return GetKnownType(HoldType.Type).Creator(this);
#endif
        }

        public void SetAs<T>(T value)
        {
            var type = typeof(T);
            TypeGuard(type);

            ((KnownType<T>)KnownType.Register[type.FullName]).Setter(this, value);
        }

        public T GetAs<T>()
        {
            var type = typeof(T);
            TypeGuard(type);

            return ((KnownType<T>)KnownType.Register[type.FullName]).Getter(this);
        }

#if UNITY_EDITOR
        public static void Layout(GenericParameter parameter)
        {
            var typename = parameter.HoldType.Type.FullName;

            KnownType info;
            if(KnownType.Register.TryGetValue(typename, out info))
            {
                info.LayoutFunc(parameter);
            }
        }

        public static void Draw(Rect drawRect, GenericParameter parameter)
        {
            var typename = parameter.HoldType.Type.FullName;

            KnownType info;
            if (KnownType.Register.TryGetValue(typename, out info) && info.DrawFunc != null)
            {
                info.DrawFunc(drawRect, parameter);
            }

            if (parameter.HoldType.Type.IsSubclassOf(KnownType.ComponentType)
            && KnownType.Register.TryGetValue(KnownType.ComponentType.FullName, out info)
            && info.DrawFunc != null)
            {
                info.DrawFunc(drawRect, parameter);
            }

            if (parameter.HoldType.Type.IsSubclassOf(KnownType.ScriptableObjectType)
            && KnownType.Register.TryGetValue(KnownType.ScriptableObjectType.FullName, out info) 
            && info.DrawFunc != null)
            {
                info.DrawFunc(drawRect, parameter);
            }
        }

        public static string GetDisplayedName(System.Type type)
        {
            var typename = type.FullName;

            KnownType info;
            if (KnownType.Register.TryGetValue(typename, out info))
            {
                return info.GenericName;
            }

            if (type.IsSubclassOf(KnownType.ComponentType)
            && KnownType.Register.ContainsKey(KnownType.ComponentType.FullName))
            {   
                return type.Name;
            }

            if (type.IsSubclassOf(KnownType.ScriptableObjectType)
            && KnownType.Register.ContainsKey(KnownType.ScriptableObjectType.FullName))
            {
                return type.Name;
            }

            return null;
        }

        public static KnownType GetKnownType(System.Type type)
        {
            var typename = type.FullName;

            KnownType info;
            if (KnownType.Register.TryGetValue(typename, out info))
            {
                return info;
            }

            if (type.IsSubclassOf(KnownType.ComponentType))
            {
                typename = KnownType.ComponentType.FullName;

                if (KnownType.Register.TryGetValue(typename, out info))
                {
                    return info;
                }
            }

            if (type.IsSubclassOf(KnownType.ScriptableObjectType))
            {
                typename = KnownType.ScriptableObjectType.FullName;

                if (KnownType.Register.TryGetValue(typename, out info))
                {
                    return info;
                }
            }

            return null;
        }
#endif
    }
}
