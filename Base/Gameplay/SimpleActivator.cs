using UnityEngine;
using UnityEngine.Events;

namespace Framework.Base.Gameplay
{
    public class SimpleActivator : MonoBehaviour
    {
        public enum TriggerMode
        {
            Enter,
            Leave
        }

        public bool Enabled = true;
        public TriggerMode Mode = TriggerMode.Enter;
        public UnityEvent Action;

        public void Enable()
        {
            Enabled = true;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Enabled || Mode != TriggerMode.Enter)
                return;

            var act = other.gameObject.GetComponentInChildren<Activateable>() ?? other.gameObject.GetComponentInParent<Activateable>();
            if (act)
            {
                Enabled = false;
                Action.Invoke();
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!Enabled || Mode != TriggerMode.Leave)
                return;

            var act = other.gameObject.GetComponentInChildren<Activateable>() ?? other.gameObject.GetComponentInParent<Activateable>();
            if (act)
            {
                Enabled = false;
                Action.Invoke();
            }
        }
    }
}