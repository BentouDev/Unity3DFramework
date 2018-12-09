using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DerivedType : IEquatable<DerivedType>
{
    [SerializeField]
    private SerializedType _baseType;

    [SerializeField]
    private SerializedType _typeValue;

    public SerializedType TypeValue
    {
        get { return _typeValue; }
        set
        {
            if (_typeValue.Type == null 
                || !_typeValue.Type.IsSubclassOf(_baseType.Type))
                _typeValue = value;
            else
                Debug.LogErrorFormat("Unable to set type {0} as derivative of {1}", value.Type, _baseType.Type);
        }
    }

    public SerializedType BaseType
    {
        get { return _baseType; }
        set 
        { 
            _baseType = value;

            if (_typeValue.Type == null 
            || !_typeValue.Type.IsSubclassOf(value.Type))
                _typeValue.Type = null;
        }
    }

    public DerivedType()
    {
        _baseType = new SerializedType();
        _typeValue = new SerializedType();
    }

    public DerivedType(string constructor)
    {
        var parts = constructor?.Split('@');
        if (parts != null)
        {
            switch (parts.Length)
            {
                case 1:
                    _baseType = new SerializedType(parts[0]);
                    break;
                case 2:
                    _baseType = new SerializedType(parts[0]);
                    _typeValue = new SerializedType(parts[1]);
                    break;
            }
        }
    }

    public DerivedType(System.Type baseType, System.Type derivedType)
    {
        if (derivedType.IsSubclassOf(baseType))
        {
            _baseType = new SerializedType(baseType);
            _typeValue = new SerializedType(derivedType);
        }
    }

    public override string ToString()
    {
        if (_baseType == null)
            return "null";

        return $"{_baseType.SerializedTypeName}@{_typeValue?.SerializedTypeName}";
    }

    public bool Equals(DerivedType other)
    {
        return other != null 
            && SerializedType.Equals(other._baseType, other._baseType) 
            && SerializedType.Equals(other._typeValue, _typeValue);
    }
}

[System.Serializable]
public class SerializedType : IEquatable<SerializedType>
{
    [SerializeField]
    private string _serializedTypeName;

    [SerializeField]
    private string _serializedMetadata;

    public string SerializedTypeName => _serializedTypeName;
    public string Metadata => _serializedMetadata;

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

    public SerializedType()
    {
        _type = null;
        _serializedTypeName = null;
    }

    public SerializedType(System.Type type, string metadata = null)
    {
        _type = type;
        _serializedTypeName = _type != null ? _type.AssemblyQualifiedName : null;
        _serializedMetadata = metadata;
    }
    
    public SerializedType(string type, string metadata = null)
    {
        _serializedTypeName = type;
        _type = Type;
        _serializedMetadata = metadata;
    }

    public bool Equals(SerializedType other)
    {
        return other != null && string.Equals(_serializedTypeName, other._serializedTypeName) 
                             && ((string.IsNullOrEmpty(_serializedMetadata) && string.IsNullOrEmpty(other._serializedMetadata))
                                 || string.Equals(_serializedMetadata, other._serializedMetadata));
    }

    public override string ToString()
    {
        string meta = String.IsNullOrWhiteSpace(_serializedMetadata) ? "System.Type" : _serializedMetadata;
        string type = Type?.Name ?? "None";
        return $"{type} ({meta})";
    }
}
