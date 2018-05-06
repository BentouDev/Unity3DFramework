using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Framework.Base.Gameplay
{
    public class Activateable : MonoBehaviour
    {
        [Header("Debug")]
        public  bool DrawDebug;
        public  bool PoolInput;
        
        [Header("UI")]
        public  IGUIActivateable      ActivatorMessage;
        public  string                ActivatorUITag = "Activator";
        private List<ActionActivator> ActiveActivators = new List<ActionActivator>();

        private bool CanSubmit => ActivatorMessage 
                               && BaseGame.Instance.IsPlaying()
                               && ActiveActivators.Any();

        public void OnLevelLoaded()
        {
            if (!ActivatorMessage)
            {
                var go = GameObject.FindGameObjectWithTag(ActivatorUITag);
                if (go)
                {
                    ActivatorMessage = go.GetComponentInChildren<IGUIActivateable>();
                    ActivatorMessage?.Init(this);
                }
            }
        }

//        void Update()
//        {
//            if (!ActivatorMessage)
//                return;
//
//            ActivatorMessage.OnActivatorSelected(null);
//
//            if (!BaseGame.Instance.IsPlaying())
//                return;
//        
//            if (!ActiveActivators.Any())
//                return;
//
//            ActivatorMessage.OnActivatorSelected();
//
//            if (Input.GetButtonDown("Submit"))
//            {
//                ActiveActivators.Last().Activate(this);
//            }
//        }

        public void PushActivator(ActionActivator activator)
        {
            var lastTop = ActiveActivators.LastOrDefault();
            if (!ActiveActivators.Contains(activator))
            {
                if (DrawDebug) Debug.LogFormat("{0} :: New Activator {1}", Time.realtimeSinceStartup, activator);
                ActiveActivators.Add(activator);
            }

            var newTop = ActiveActivators.LastOrDefault();
            if (lastTop != newTop && ActivatorMessage)
                ActivatorMessage.OnActivatorSelected(newTop);
        }

        public void PopActivator(ActionActivator activator)
        {
            var lastTop = ActiveActivators.LastOrDefault();
            if (ActiveActivators.Remove(activator))
            {
                if (DrawDebug) Debug.LogFormat("{0} :: Lost Activator {1}", Time.realtimeSinceStartup, activator);
            }

            var newTop = ActiveActivators.LastOrDefault();
            if (newTop == null && ActivatorMessage)
                ActivatorMessage.OnActivatorSelected(null);
            else if (lastTop != newTop && CanSubmit)
                ActivatorMessage.OnActivatorSelected(newTop);
        }

        public void Update()
        {
            if (!PoolInput)
                return;

            if (Input.GetButtonDown("Submit"))
            {
                OnSubmit();
            }
        }

        public void OnSubmit()
        {
            if (!CanSubmit)
                return;
            
            var last = ActiveActivators.LastOrDefault();
            if (last) 
                last.Activate(this);
        }
    }
}
