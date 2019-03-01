using Framework;

namespace DefaultNamespace
{
    public class ParamEquals : Condition
    {
        [RequireValue]
        public SerializedType InputType;

        [RequireValue]
        [Parametrized]
        public GenericParameter First;

        [RequireValue]
        [Parametrized]
        public GenericParameter Second;

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
            return $"{First} equals {Second}";
        }
    }
}