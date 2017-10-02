using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class DialogGameState : GameState
    {
        private DialogInstance CurrentDialog;
        
        protected void EndDialog()
        {
            foreach (var actor in CurrentDialog.Actors.Where(a => a.Type == DialogInstance.ActorType.Dynamic))
            {
                actor.Actor = null;
            }
        }

        public void StartDialog(DialogInstance instance)
        {
            if (CurrentDialog)
                EndDialog();

            CurrentDialog = instance;

            foreach (var actor in CurrentDialog.Actors.Where(a => a.Type == DialogInstance.ActorType.Dynamic))
            {
                var go = GameObject.FindGameObjectWithTag(actor.Tag);
                if (go != null)
                {
                    actor.Actor = go.GetComponent<DialogActor>();
                }

                if (actor.Actor)
                    Debug.LogWarning(string.Format("Missing actor '{0}' for dialog '{1}'!", actor.Name, CurrentDialog.Dialog.name), CurrentDialog);
            }
        }

        protected override void OnTick()
        {

        }

        protected override void OnEnd()
        {
            EndDialog();
        }
    }
}
