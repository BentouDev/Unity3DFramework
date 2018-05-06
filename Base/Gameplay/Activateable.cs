using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Base.Gameplay
{
    public class Activateable : MonoBehaviour
    {
        private List<ActionActivator> ActiveActivators = new List<ActionActivator>();
        public  IGUIActivateable      ActivatorMessage;
        public  string                ActivatorUITag = "Activator";

        public void OnLevelLoaded()
        {
            if (!ActivatorMessage)
            {
                var go = GameObject.FindGameObjectWithTag(ActivatorUITag);
                if (go)
                {
                    ActivatorMessage = go.GetComponentInChildren<IGUIActivateable>();
                }
            }
        }

        void Update()
        {
            if (!ActivatorMessage)
                return;

            ActivatorMessage.SetText(string.Empty);

            if (!BaseGame.Instance.IsPlaying())
                return;
        
            if (!ActiveActivators.Any())
                return;

            ActivatorMessage.SetText(ActiveActivators.Last().Message);

            if (Input.GetButtonDown("Submit"))
            {
                ActiveActivators.Last().Activate(this);
            }
        }

        public void PushActivator(ActionActivator activator)
        {
            if (!ActiveActivators.Contains(activator))
            {
                // Debug.LogFormat("{0} :: New Activator {1}", Time.realtimeSinceStartup, activator);
                ActiveActivators.Add(activator);
            }
        }

        public void PopActivator(ActionActivator activator)
        {
            if (ActiveActivators.Remove(activator))
            {
                // Debug.LogFormat("{0} :: Lost Activator {1}", Time.realtimeSinceStartup, activator);
            }
        }
    }
}
