using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framework.Editor
{
    public class PropertyIndex : IEquatable<PropertyIndex>
    {
        public string Name;
        public Type Type;

        public bool Equals(PropertyIndex other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Type == other.Type && string.Equals(Name, other.Name);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyIndex) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Name != null ? Name.GetHashCode() : 0) * 397) ^ (Type != null ? Type.GetHashCode() : 0);
            }
        }
    }
    
    [System.Serializable]
    public class PropertyPath : IEquatable<PropertyPath>
    {
        [SerializeField]
        private List<PropertyIndex> _path = new List<PropertyIndex>();

        public void Append(string label, Type type)
        {
            _path.Add(new PropertyIndex(){Name = label, Type = type});
        }

        public void Clear()
        {
            _path.Clear();
        }

        public Type GetLastType()
        {
            return _path.Any() ? _path.Last().Type : null;
        }

        public FieldInfo MaterializeToFieldInfo(Type targetType)
        {
            FieldInfo result = null;
            Type owningType = targetType;

            foreach (var index in _path)
            {
                Type indexType = typeof(IList).IsAssignableFrom(index.Type) 
                    ? index.Type.GenericTypeArguments[0] 
                    : index.Type;

                var info = owningType.GetField(index.Name);
                if (info == null || info.GetUnderlyingType() != index.Type)
                {
                    return null;
                }

                result = info;
                owningType = indexType;
            }

            return result;
        }

        public bool Equals(PropertyPath other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            if (_path.Count != other._path.Count) return false;

            for (int i = 0; i < _path.Count; i++)
            {
                if (!_path[i].Equals(other._path[i]))
                    return false;
            }
            
            return true;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((PropertyPath) obj);
        }

        public override int GetHashCode()
        {
            return (_path != null ? _path.GetHashCode() : 0);
        }
    }
}