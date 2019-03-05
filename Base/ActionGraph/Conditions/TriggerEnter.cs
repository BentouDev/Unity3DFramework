using UnityEngine;

namespace Framework
{
    public class TriggerEnter : Condition
    {
        [Parametrized]
        public Transform Target;

        private bool Satisfied;

        public override bool IsSatisfied() => Satisfied;

        public override void OnSetupParametrizedProperties()
        {
            SetupParameters(this);
        }

        public override string GetDescription()
        {
            string param = Target ? Target.ToString() : "target";
            return $"On enter [{param}]";
        }
    }
}