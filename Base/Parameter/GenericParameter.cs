using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            public string DisplayedName;
            public System.Type HoldType;

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
                DisplayedName = name;
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
        public readonly SerializedType HoldType;

        [SerializeField]
        private Object _object;

        [SerializeField]
        private readonly float[] _floats = new float[4];

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
            KnownTypes[typeof(T).Name] = info;
        }

        public static void BuildKnownTypeList()
        {
            InsertKnownType
            (
                new KnownTypeInfo<bool>("bool")
                {
                    Getter = parameter => parameter._floats[0] > 0,
                    Setter = (parameter, value) => { parameter._floats[0] = value ? 1 : -1; },
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<float>("float")
                {
                    Getter = parameter => parameter._floats[0],
                    Setter = (parameter, value) => { parameter._floats[0] = value; }
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<Vector2>("Vector2")
                {
                    Getter = parameter => new Vector2(parameter._floats[0], parameter._floats[1]),
                    Setter = (parameter, vector2) => { parameter._floats[0] = vector2.x; parameter._floats[1] = vector2.y; }
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<Vector3>("Vector3")
                {
                    Getter = parameter => new Vector3(parameter._floats[0], parameter._floats[1], parameter._floats[2]),
                    Setter = (parameter, vector3) => { parameter._floats[0] = vector3.x; parameter._floats[1] = vector3.y; parameter._floats[2] = vector3.z; }
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<GameObject>("GameObject")
                {
                    Getter = parameter => parameter._object as GameObject,
                    Setter = (parameter, value) => { parameter._object = value; }
                }
            );

            InsertKnownType
            (
                new KnownTypeInfo<MonoBehaviour>("MonoBehaviour...")
                {
                    Getter = parameter => parameter._object as MonoBehaviour,
                    Setter = (parameter, value) => { parameter._object = value; }
                }
            );
        }

        private void TypeGuard(System.Type type)
        {
            System.Func<Vector2> getter = () => new Vector2();

            System.Action<Vector2> setter = (v) => { };

            if (type != HoldType.Type)
            {
                throw new System.InvalidOperationException (
                    string.Format("Unable to use type '{0}' on parameter of type {1}!", 
                    type.AssemblyQualifiedName, HoldType.SerializedTypeName)
                );
            }
        }

        public void SetAs<T>(T value)
        {
            var type = typeof(T);
            TypeGuard(type);

            ((KnownTypeInfo<T>)KnownTypes[type.Name]).Setter(this, value);
        }

        public T GetAs<T>()
        {
            var type = typeof(T);
            TypeGuard(type);

            return ((KnownTypeInfo<T>)KnownTypes[type.Name]).Getter(this);
        }

#if UNITY_EDITOR
        public static void Layout(GenericParameter parameter)
        {
            var typename = parameter.HoldType.Type.Name;

            KnownTypeInfo info;
            if(KnownTypes.TryGetValue(typename, out info))
            {
                info.LayoutFunc(parameter);
            }
        }

        public static void Draw(Rect drawRect, GenericParameter parameter)
        {
            var typename = parameter.HoldType.Type.Name;

            KnownTypeInfo info;
            if (KnownTypes.TryGetValue(typename, out info))
            {
                info.DrawFunc(drawRect, parameter);
            }
        }
#endif
    }
}
