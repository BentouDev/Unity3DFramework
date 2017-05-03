using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class PawnState : MonoBehaviour, ITickable
    {
        protected StatePawn Pawn;

        protected PawnState LastState { get { return Pawn.LastState; } }
        protected bool IsGrounded { get { return Pawn.IsGrounded; } }

        protected void SwitchState<T>() where T : PawnState { Pawn.SwitchState<T>(); }

        protected void ProcessTime(ref float time)
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                time = 0;
            }
        }

        public void Init(StatePawn pawn)
        {
            Pawn = pawn;
            OnInit();
        }

        public void DoStart()
        {
            OnStart();
        }
        
        public void ProcessMovement(Vector3 direction)
        {
            OnProcessMovement(direction);
        }

        public void DoEnd()
        {
            OnEnd();
        }

        protected virtual void OnInit()
        { }

        protected virtual void OnStart()
        { }

        protected virtual void OnProcessMovement(Vector3 direction)
        { }

        protected virtual void OnEnd()
        { }

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

        protected virtual void OnTick()
        { }

        protected virtual void OnFixedTick()
        { }

        protected virtual void OnLateTick()
        { }
    }
}
