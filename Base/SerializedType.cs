using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SerializedType : IEquatable<SerializedType>
{
    [SerializeField]
    private string _serializedTypeName;

    public string SerializedTypeName
    {
        get
        {
            return _serializedTypeName;
        }
    }

    private System.Type _type;
    public System.Type Type
    {
        get
        {
            if (_type == null && !string.IsNullOrEmpty(_serializedTypeName))
            {
                var found = System.Type.GetType(_serializedTypeName);
                if (found == null)
                {
                    _type = typeof(void);
                    Debug.LogErrorFormat("Unable to find type {0}", _serializedTypeName);
                }

                _type = found;
            }

            return _type;
        }

        set
        {
            _type = value;
            _serializedTypeName = _type != null ? _type.AssemblyQualifiedName : null;
        }
    }

    public SerializedType(System.Type type)
    {
        _type = type;
        _serializedTypeName = _type != null ? _type.AssemblyQualifiedName : null;
    }

    public bool Equals(SerializedType other)
    {
        return other != null && string.Equals(this._serializedTypeName, other._serializedTypeName);
    }
}
