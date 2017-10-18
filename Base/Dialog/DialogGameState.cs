using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public abstract class DialogGameState : GameState
    {
        [Range(0,5)]
        public float Delay = 1;

        public  DialogInstance      CurrentDialog { get; private set; }
        public  DialogState         CurrentState  { get; private set; }
        private DialogState         NextState;
        private IDialogStateHandler CurrentHandler;
        private bool                IsExiting;

        private List<IDialogStateHandler> AllHandlers = new List<IDialogStateHandler>();
        
        protected void EndDialog()
        {
            CurrentHandler?.End();
            CurrentHandler = null;
            CurrentState   = null;

            CurrentDialog.OnDialogEnd.Invoke();
            CurrentDialog.CleanUp();
            CurrentDialog = null;
        }

        protected override void OnStart()
        {
            IsExiting = false;

            if (AllHandlers == null)
                AllHandlers = new List<IDialogStateHandler>();
            else
                AllHandlers.Clear();
            
            AllHandlers.AddRange(FindObjectsOfType<IDialogStateHandler>());

            foreach (var handler in AllHandlers)
            {
                handler.Init(this);
            }
        }

        public void StartDialog(DialogInstance instance)
        {
            if (CurrentDialog)
            {
                EndDialog();
                StartDialogInternal(instance);
            }
            else
            {
                StartCoroutine(CoStartDialogDelayed(instance));
            }
        }

        IEnumerator CoStartDialogDelayed(DialogInstance instance)
        {
            yield return new WaitForSecondsRealtime(Delay);
            StartDialogInternal(instance);
        }

        private void StartDialogInternal(DialogInstance instance)
        {
            CurrentDialog = instance;
            CurrentDialog.Init();
            CurrentDialog.OnDialogStart.Invoke();

            foreach (var actor in CurrentDialog.Actors.Where(a => a.Type == DialogInstance.ActorType.Dynamic))
            {
                var go = GameObject.FindGameObjectWithTag(actor.Tag);
                if (go != null)
                {
                    actor.Actor = go.GetComponentInChildren<DialogActor>() ?? go.GetComponentInParent<DialogActor>();
                }

                if (!actor.Actor)
                    Debug.LogWarning(string.Format("Missing actor '{0}' for dialog '{1}'!", actor.Name, CurrentDialog.Dialog.name), CurrentDialog);
            }

            SwitchState(CurrentDialog.Dialog.FirstState);
        }

        public void SwitchState(DialogState state)
        {
            if (state == null)
            {
                Debug.LogError("Theres no state to switch to", CurrentState);
                Exit();
            }

            NextState = state;
        }

        public void Exit()
        {
            IsExiting = true;
        }

        protected abstract void ReturnToPreviousState();

        protected override void OnTick()
        {
            if (IsExiting)
            {
                ReturnToPreviousState();
                return;
            }

            if (NextState)
            {
                CurrentHandler?.End();
                CurrentHandler = null;

                var handler = AllHandlers.FirstOrDefault(h => h.Supports(NextState));
                if (handler != null)
                {
                    CurrentState   = NextState;
                    CurrentHandler = handler;
                    NextState      = null;

                    CurrentHandler.Begin();
                }
                else
                {
                    Debug.LogError("Unable to handle state", CurrentState);
                    ReturnToPreviousState();
                }
            }
            else
            {
                if (CurrentHandler != null)
                    CurrentHandler.Tick();
            }
        }

        protected override void OnFixedTick()
        {
            if (CurrentHandler != null && !NextState)
                CurrentHandler.FixedTick();
        }

        protected override void OnLateTick()
        {
            if (CurrentHandler != null && !NextState)
                CurrentHandler.LateTick();
        }

        protected override void OnEnd()
        {
            EndDialog();
        }
    }
}
