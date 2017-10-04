using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public abstract class DialogGameState : GameState
    {
        public DialogInstance CurrentDialog { get; private set; }

        public DialogState CurrentState { get; private set; }

        private IDialogStateHandler CurrentHandler;

        private List<IDialogStateHandler> AllHandlers = new List<IDialogStateHandler>();
        
        protected void EndDialog()
        {
            foreach (var actor in CurrentDialog.Actors.Where(a => a.Type == DialogInstance.ActorType.Dynamic))
            {
                actor.Actor = null;
            }

            CurrentState = null;
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

            SwitchState(CurrentDialog.Dialog.FirstState);
        }

        public void SwitchState(DialogState state)
        {
            if (state == null)
            {
                Debug.LogError("Theres no state to switch to", CurrentState);
                ReturnToPreviousState();
            }
            else
            {
                var handler = AllHandlers.FirstOrDefault(h => h.Supports(state));
                if (handler != null)
                {
                    if (CurrentHandler != null)
                        CurrentHandler.End();

                    CurrentState = state;
                    CurrentHandler = handler;

                    CurrentHandler.Begin(CurrentState);
                }
                else
                {
                    Debug.LogError("Unable to handle state", CurrentState);
                    ReturnToPreviousState();
                }
            }
        }

        protected abstract void ReturnToPreviousState();

        protected override void OnTick()
        {
            if (CurrentHandler != null)
                CurrentHandler.Tick();
        }

        protected override void OnFixedTick()
        {
            if (CurrentHandler != null)
                CurrentHandler.FixedTick();
        }

        protected override void OnLateTick()
        {
            if (CurrentHandler != null)
                CurrentHandler.LateTick();
        }

        protected override void OnEnd()
        {
            EndDialog();
        }
    }
}
