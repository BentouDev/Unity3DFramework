using Framework;
using UnityEditor.PackageManager;
using UnityEngine;

namespace Framework
{
    public class ParamEquals : Condition
    {
        [RequireValue]
        public SerializedType InputType;

        [Validate(nameof(OnValidateFirstParam))]
        [TypeRestricted(TypeRestricted.TypeSource.Field, nameof(InputType))]
        public Variant First;

        [Validate(nameof(OnValidateSecondParam))]
        [TypeRestricted(TypeRestricted.TypeSource.Field, nameof(InputType))]
        public Variant Second;

        public override bool IsSatisfied()
        {
            return First.Equals(Second);
        }

        void OnValidate()
        {
            First.HoldType = InputType;
            Second.HoldType = InputType;
        }

        public override string GetDescription()
        {
            string first = First.Get() == null ? "first" : First.Get().ToString();
            string second = Second.Get() == null ? "second" : Second.Get().ToString();
            return $"[{first}] equals [{second}]";
        }

        public override void OnSetupParametrizedProperties()
        {
            SetupParameters(this);
        }
        
    #if UNITY_EDITOR
        ValidationResult OnValidateFirstParam()
        {
            return OnValidateParam(First);
        }
        
        ValidationResult OnValidateSecondParam()
        {
            return OnValidateParam(Second);
        }
        
        ValidationResult OnValidateParam(Variant param)
        {
            if (string.IsNullOrEmpty(param.Name))
                return new ValidationResult(ValidationStatus.Error,"Parameter reference must be set!");
            return ValidationResult.Ok;
        }
    #endif
    }
}