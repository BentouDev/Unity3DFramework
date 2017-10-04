using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEngine;

namespace Framework
{
    public abstract class IDialogStateHandler : MonoBehaviour, ITickable
    {
        public abstract void Init(DialogGameState state);
        public abstract bool Supports(DialogState state);
        public abstract void Begin(DialogState state);
        public abstract void End();
        public abstract void Tick();
        public abstract void FixedTick();
        public abstract void LateTick();
    }

    public abstract class DialogStateHandler<TState> : IDialogStateHandler where TState : DialogState
    {
        protected TState CurrentState;
        protected DialogInstance Dialog;

        public override void Init(DialogGameState state)
        {
            Dialog = state.CurrentDialog;

            OnInit();
        }

        public override bool Supports(DialogState state)
        {
            return state is TState;
        }

        public override void Begin(DialogState state)
        {
            CurrentState = (TState) state;
            OnBegin();
        }

        public override void Tick()
        {
            OnTick();
        }

        public override void FixedTick()
        {
            OnFixedTick();
        }

        public override void LateTick()
        {
            OnLateTick();
        }

        public override void End()
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
