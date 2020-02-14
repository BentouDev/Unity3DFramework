using System;
using UnityEngine;

namespace Framework
{
    [System.Serializable]
    public class Parameter : IEquatable<Parameter>, ISerializationCallbackReceiver
    {
        [SerializeField]
        private string _guidData;
        
        private Guid _id;
        
        public Guid Id => _id;

        [SerializeField]
        private string _name;

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                Value.Name = value;
            }
        }

        [SerializeField]
        public Variant Value;

        public Parameter()
        {
            _id = Guid.NewGuid();
            Value = new Variant();
        }

        public Parameter(SerializedType type, string name)
        {
            _id = Guid.NewGuid();
            Value = new Variant(type) {Name = name};

            Name = name;
        }

        public Parameter(Type type, string name)
        {
            _id = Guid.NewGuid();

            Value = new Variant(type) {Name = name};

            Name = name;
        }

        public ParameterReference CreateReference()
        {
            return new ParameterReference() { ParameterId = Id };
        }

        public SerializedType GetHoldType()
        {
            return Value.HoldType;
        }

        public bool Equals(Parameter other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return _id.Equals(other._id);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Parameter) obj);
        }

        public override int GetHashCode()
        {
            return _id.GetHashCode();
        }

        public void OnBeforeSerialize()
        {
            _guidData = _id.ToString();
        }

        public void OnAfterDeserialize()
        {
            if (_guidData != null)
                _id = Guid.Parse(_guidData);
        }
    }
}