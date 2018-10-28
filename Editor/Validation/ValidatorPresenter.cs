using System;
using System.Collections.Generic;
using Framework.Editor;
using UnityEditor;

namespace Framework.Editor
{
    public class ValidatorPresenter : EditorPresenter
    {
        protected ValidatorWindow View;

        protected HashSet<ValidationEntry> Entries = new HashSet<ValidationEntry>();

        public ValidatorPresenter(ValidatorWindow view)
        {
            View = view;
        }

        public class ValidationEntry : IEquatable<ValidationEntry>
        {
            public UnityEngine.Object Target;
            public ValidationResult   Result;

            public bool Equals(ValidationEntry other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return Equals(Target, other.Target) && Equals(Result, other.Result);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ValidationEntry) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return ((Target != null ? Target.GetHashCode() : 0) * 397) ^ (Result != null ? Result.GetHashCode() : 0);
                }
            }
        }
        
        internal override void OnDraw()
        {
            // ToDo: why is this not working?
            Entries.RemoveWhere(e => !e.Target || e.Target == null);

            View.DrawHeader(Entries);
            View.DrawList(Entries);
        }

        internal override void OnProjectChange()
        {
            Entries.Clear();
        }

        public void OnRemoveValidation(ValidationResult result)
        {
            Entries.RemoveWhere(r => r.Result.Equals(result));
        }

        public void OnRegisterValidation(ValidationResult result, UnityEngine.Object target)
        {
            Entries.Add(new ValidationEntry()
            {
                Result = result,
                Target = target
            });
        }
    }
}