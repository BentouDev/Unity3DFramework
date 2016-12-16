using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public class EditorParameter
    {
        public EditorParameter(ParameterType type)
        {
            HoldType = type;
        }

        [System.Serializable]
        public enum ParameterType
        {
            Int,
            Float,
            Bool,
            Vec2,
            Vec3,
            Object
        }

        [SerializeField]
        public ParameterType HoldType;

        [SerializeField]
        public string Name;

        [SerializeField]
        public int IntValue;

        [SerializeField]
        public float FloatValue;

        [SerializeField]
        public bool BoolValue;

        [SerializeField]
        public Vector2 Vec2Value;

        [SerializeField]
        public Vector3 Vec3Value;

        [SerializeField]
        public Vector4 Vec4Value;

        [SerializeField]
        public Object ObjectValue;

        [SerializeField]
        public string SerializedType;

        private System.Type _type;

        public System.Type Type
        {
            get
            {
                if (_type == null && !string.IsNullOrEmpty(SerializedType))
                    _type = System.Type.GetType(SerializedType);
                return _type;
            }

            set
            {
                _type = value;
                SerializedType = _type != null ? _type.AssemblyQualifiedName : null;
            }
        }
    }
}
