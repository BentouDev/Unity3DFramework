using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Framework
{
    public interface IDialogStateHandler : ITickable
    {
        void Init(DialogGameState state);
        bool Supports(DialogState state);
        void Begin(DialogState state);
        void End();
    }

    public abstract class DialogStateHandler<TState> : MonoBehaviour, IDialogStateHandler where TState : DialogState
    {
        protected TState CurrentState;
        protected DialogInstance Dialog;

        public void Init(DialogGameState state)
        {
            Dialog = state.CurrentDialog;

            OnInit();
        }

        public bool Supports(DialogState state)
        {
            return state is TState;
        }

        public void Begin(DialogState state)
        {
            CurrentState = (TState) state;
            OnBegin();
        }

        public void Tick()
        {
            OnTick();
        }

        public void FixedTick()
        {
            OnFixedTick();
        }

        public void LateTick()
        {
            OnLateTick();
        }

        public void End()
        {
            OnEnd();
            CurrentState = null;
        }

        protected virtual void OnInit()
        { }

        protected virtual void OnBegin()
        { }

        protected virtual void OnTick()
        { }

        protected virtual void OnFixedTick()
        { }

        protected virtual void OnLateTick()
        { }

        protected virtual void OnEnd()
        { }
    }
}
