using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
    [System.Serializable]
    public class GenericParameter
    {
#if UNITY_EDITOR
        public delegate void DrawFunc(Rect rect, GenericParameter param);
        public delegate void LayoutFunc(GenericParameter param);
#endif

        public class KnownTypeInfo
        {
            protected internal System.Func<GenericParameter, Blackboard.IValue> Creator;

            public System.Type HoldType;
            public string GenericName;

#if UNITY_EDITOR
            public DrawFunc DrawFunc;
            public LayoutFunc LayoutFunc;
#endif
        }

        class KnownTypeInfo<T> : KnownTypeInfo
        {
            protected internal System.Func<GenericParameter, T> Getter;
            protected internal System.Action<GenericParameter, T> Setter;

            protected internal KnownTypeInfo(string name)
            {
                GenericName = name;
                HoldType = typeof(T);
            }
        }
        
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

        private static System.Type _scriptableObjectType;
        private static System.Type ScriptableObjectType
        {
            get
            {
                if (_scriptableObjectType == null)
                    _scriptableObjectType = typeof(ScriptableObject);
                return _scriptableObjectType;
            }
        }

        private static System.Type _componentType;
        private static System.Type ComponentType
        {
            get
            {
                if (_componentType == null)
                    _componentType = typeof(Component);
                return _componentType;
            }
        }

        private static readonly Dictionary<string, KnownTypeInfo> KnownTypes = new Dictionary<string, KnownTypeInfo>();

#if UNITY_EDITOR
        public static Dictionary<string, KnownTypeInfo> GetKnownTypeDictionary()
        {
            return KnownTypes;
        }
#endif

        public static List<KnownTypeInfo> GetKnownTypes()
        {
            return KnownTypes.Values.ToList();
        }

        private static void InsertKnownType<T>(KnownTypeInfo<T> info)
        {
            KnownTypes[typeof(T).FullName] = info;
        }

        public static void BuildKnownTypeList()
        {
            InsertKnownType
            (
                new KnownTypeInfo<bool>("bool")
                {
                    Getter = parameter => parameter._floats[0] > 0,
                    Setter = (parameter, value) => { parameter._floats[0] = value ? 1 : -1; },
                    Creator = parameter => new Blackboard.Value<bool>(parameter.HoldType.Type)
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<int>("int")
                {
                    Getter = parameter => Mathf.RoundToInt(parameter._floats[0]),
                    Setter = (parameter, value) => { parameter._floats[0] = value; },
                    Creator = parameter => new Blackboard.Value<int>(parameter.HoldType.Type)
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<float>("float")
                {
                    Getter = parameter => parameter._floats[0],
                    Setter = (parameter, value) => { parameter._floats[0] = value; },
                    Creator = parameter => new Blackboard.Value<float>(parameter.HoldType.Type)
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<Vector2>("Vector2")
                {
                    Getter = parameter => new Vector2(parameter._floats[0], parameter._floats[1]),
                    Setter = (parameter, vector2) => { parameter._floats[0] = vector2.x; parameter._floats[1] = vector2.y; },
                    Creator = parameter => new Blackboard.Value<Vector2>(parameter.HoldType.Type)
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<Vector3>("Vector3")
                {
                    Getter = parameter => new Vector3(parameter._floats[0], parameter._floats[1], parameter._floats[2]),
                    Setter = (parameter, vector3) => { parameter._floats[0] = vector3.x; parameter._floats[1] = vector3.y; parameter._floats[2] = vector3.z; },
                    Creator = parameter => new Blackboard.Value<Vector3>(parameter.HoldType.Type)
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<GameObject>("GameObject")
                {
                    Getter = parameter => parameter._object as GameObject,
                    Setter = (parameter, value) => { parameter._object = value; },
                    Creator = parameter => new Blackboard.Value<GameObject>(parameter.HoldType.Type)
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<Component>("MonoBehaviour...")
                {
                    Getter = parameter => parameter._object as Component,
                    Setter = (parameter, value) => { parameter._object = value; },
                    Creator = parameter => new Blackboard.Value<Component>(parameter.HoldType.Type)
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<ScriptableObject>("ScriptableObject...")
                {
                    Getter = parameter => parameter._object as ScriptableObject,
                    Setter = (parameter, value) => { parameter._object = value; },
                    Creator = parameter => new Blackboard.Value<ScriptableObject>(parameter.HoldType.Type)
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
        
        public Blackboard.IValue CreateValue()
        {
#if UNITY_EDITOR
            var knownType = GetKnownType(HoldType.Type);
            if (knownType != null)
            {
                return knownType.Creator(this);
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

            ((KnownTypeInfo<T>)KnownTypes[type.FullName]).Setter(this, value);
        }

        public T GetAs<T>()
        {
            var type = typeof(T);
            TypeGuard(type);

            return ((KnownTypeInfo<T>)KnownTypes[type.FullName]).Getter(this);
        }

#if UNITY_EDITOR
        public static void Layout(GenericParameter parameter)
        {
            var typename = parameter.HoldType.Type.FullName;

            KnownTypeInfo info;
            if(KnownTypes.TryGetValue(typename, out info))
            {
                info.LayoutFunc(parameter);
            }
        }

        public static void Draw(Rect drawRect, GenericParameter parameter)
        {
            var typename = parameter.HoldType.Type.FullName;

            KnownTypeInfo info;
            if (KnownTypes.TryGetValue(typename, out info))
            {
                info.DrawFunc(drawRect, parameter);
            }

            if (parameter.HoldType.Type.IsSubclassOf(ComponentType)
            && KnownTypes.TryGetValue(ComponentType.FullName, out info))
            {
                info.DrawFunc(drawRect, parameter);
            }

            if (parameter.HoldType.Type.IsSubclassOf(ScriptableObjectType)
            && KnownTypes.TryGetValue(ScriptableObjectType.FullName, out info))
            {
                info.DrawFunc(drawRect, parameter);
            }
        }

        public static string GetDisplayedName(System.Type type)
        {
            var typename = type.FullName;

            KnownTypeInfo info;
            if (KnownTypes.TryGetValue(typename, out info))
            {
                return info.GenericName;
            }

            if (type.IsSubclassOf(ComponentType)
            && KnownTypes.ContainsKey(ComponentType.FullName))
            {   
                return type.Name;
            }

            if (type.IsSubclassOf(ScriptableObjectType)
            && KnownTypes.ContainsKey(ScriptableObjectType.FullName))
            {
                return type.Name;
            }

            return null;
        }

        public static KnownTypeInfo GetKnownType(System.Type type)
        {
            var typename = type.FullName;

            KnownTypeInfo info;
            if (KnownTypes.TryGetValue(typename, out info))
            {
                return info;
            }

            if (type.IsSubclassOf(ComponentType))
            {
                typename = ComponentType.FullName;

                if (KnownTypes.TryGetValue(typename, out info))
                {
                    return info;
                }
            }

            if (type.IsSubclassOf(ScriptableObjectType))
            {
                typename = ScriptableObjectType.FullName;

                if (KnownTypes.TryGetValue(typename, out info))
                {
                    return info;
                }
            }

            return null;
        }
#endif
    }
}
