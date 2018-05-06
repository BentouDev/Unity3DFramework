using UnityEngine;
using UnityEngine.Events;

namespace Framework.Base.Gameplay
{
    [RequireComponent(typeof(SphereCollider))]
    public class ActionActivator : MonoBehaviour
    {
        public bool         Enabled = true;
        public string       Name = "Activate";
        public UnityEvent   Action;
        public Activateable CurrentActivateable { get; private set; }
    
        public void Activate(Activateable act)
        {
            CurrentActivateable = act;
            Enabled = false;

            CurrentActivateable.PopActivator(this);
            Action.Invoke();
        }

        public void Enable()
        {
            Enabled = true;
        }

        public void Disable()
        {
            Enabled = false;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!Enabled || !CurrentActivateable)
                return;
        
            CurrentActivateable.PushActivator(this);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!Enabled)
                return;

            var act = other.gameObject.GetComponentInChildren<Activateable>() ?? other.gameObject.GetComponentInParent<Activateable>();
            if (act) act.PushActivator(this);
        }

        private void OnTriggerExit(Collider other)
        {
            var act = other.gameObject.GetComponentInChildren<Activateable>() ?? other.gameObject.GetComponentInParent<Activateable>();
            if (act) act.PopActivator(this);

            CurrentActivateable = null;
        }
    }
}
