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
        public abstract void Begin();
        public abstract void End();
        public abstract void Tick();
        public abstract void FixedTick();
        public abstract void LateTick();
    }

    public abstract class DialogStateHandler<TState> : IDialogStateHandler where TState : DialogState
    {
        public TState          CurrentState { get; private set; }
        public DialogInstance  Dialog       { get; private set; }
        public DialogGameState Manager      { get; private set; }

        public override void Init(DialogGameState state)
        {
            Manager = state;

            OnInit();
        }

        public override bool Supports(DialogState state)
        {
            return state is TState;
        }

        public override void Begin()
        {
            Dialog       = Manager.CurrentDialog;
            CurrentState = (TState) Manager.CurrentState;
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
