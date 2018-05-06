using UnityEngine;
using UnityEngine.Events;

namespace Framework.Base.Gameplay
{
    [RequireComponent(typeof(SphereCollider))]
    public class ActionActivator : MonoBehaviour
    {
        public  bool         Enabled = true;
        public  string       Message = "Activate";
        public  UnityEvent   Action;
        private Activateable LastActivateable;
    
        public void Activate(Activateable act)
        {
            LastActivateable = act;
            Enabled = false;

            LastActivateable.PopActivator(this);
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
            if (!Enabled || !LastActivateable)
                return;
        
            LastActivateable.PushActivator(this);
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

            LastActivateable = null;
        }
    }
}
