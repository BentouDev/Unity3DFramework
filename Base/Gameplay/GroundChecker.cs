using UnityEngine;

namespace Framework
{
    public abstract class GroundChecker : MonoBehaviour
    {
        public bool IsGrouneded { get; private set; }
        
        public void CheckGround()
        {
            IsGrouneded = OnCheckGround();
        }

        protected abstract bool OnCheckGround();
    }
}