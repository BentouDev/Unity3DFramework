using UnityEngine;

namespace Framework
{
    public abstract class Controllable<T> : MonoBehaviour
    {
        public abstract bool ProcessInput(T inputData);
    }
}
