using System;
using UnityEditor.Experimental.UIElements.GraphView;
using UnityEngine;

namespace Framework
{
    public enum ValidationStatus
    {
        Success,
        Info,
        Warning,
        Error
    }

    public class ValidationResult : IEquatable<ValidationResult>
    {
        public static readonly ValidationResult Ok = new ValidationResult(ValidationStatus.Success, string.Empty);
        
        public ValidationStatus Status;
        public string Message;
        
        public static implicit operator bool(ValidationResult result)
        {
            return result != null && result.Status == ValidationStatus.Success;
        }

        public ValidationResult(ValidationStatus status, string message = "Validation failed!")
        {
            Status = status;
            Message = message;
        }

        public bool Equals(ValidationResult other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return Status == other.Status && string.Equals(Message, other.Message);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ValidationResult) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((int) Status * 397) ^ (Message != null ? Message.GetHashCode() : 0);
            }
        }
    }
    
    public class Validate : BaseEditorAttribute
    {
        public string Validator;

        public Validate(string validator)
        {
            Validator = validator;
        }
    }
}