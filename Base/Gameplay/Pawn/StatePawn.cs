using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    [SelectionBase]
    public class StatePawn : BasePawn
    {
        public PawnState CurrentState { get; protected set; }
        public PawnState LastState    { get; protected set; }

        [HideInInspector]
        public List<PawnState> AllStates = new List<PawnState>();
        
        protected override void OnInit()
        {
            if (AllStates != null)
                AllStates.Clear();
            else 
                AllStates = new List<PawnState>();

            AllStates.AddRange(GetComponentsInChildren<PawnState>());

            foreach (PawnState state in AllStates)
            {
                state.Init(this);
            }

            SwitchState<PawnIdle>();
        }

        public void SwitchState(PawnState state)
        {
            LastState = CurrentState;

            if (LastState) LastState.DoEnd();
            CurrentState = state;
            if (CurrentState) CurrentState.DoStart();
        }

        public void SwitchState<T>() where T : PawnState
        {
            SwitchState(AllStates.FirstOrDefault(s => s is T));
        }

        protected override void OnProcessMovement(Vector3 direction)
        {
            if (CurrentState != null)
            {
                CurrentState.ProcessMovement(direction);
            }
        }

        internal void SimulateMovement(Vector3 direction)
        {
            if (direction.magnitude > Movement.MinimalForceThreshold)
            {
                CurrentDirection = direction;
                CurrentSpeed += Movement.Acceleration * Time.fixedDeltaTime;
            }
            else
            {
                CurrentSpeed -= Movement.Friction * Time.fixedDeltaTime;
            }

            CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0, GetMaxSpeed());

            var flatVelocity    = new Vector3(CurrentDirection.x, 0, CurrentDirection.z);
            var appliedVelocity = flatVelocity * CurrentSpeed;

            if (IsGrounded && StickToGround)
            {
                Quaternion slope = Quaternion.FromToRotation(Vector3.up, LastGroundHit.normal);
                appliedVelocity  = slope * appliedVelocity;
            }

            Velocity = appliedVelocity;
        }

        protected override void OnTick()
        {
            if (!IsAlive())
                return;

            if (CurrentState != null)
            {
                CurrentState.Tick();
            }
        }

        protected override void OnFixedTick()
        {
            if (!IsAlive())
                return;

            if (CurrentState != null)
            {
                CurrentState.FixedTick();
            }
        }

        protected override void OnLateTick()
        {
            if (!IsAlive())
                return;

            FaceMovementDirection(15);

            if (CurrentState != null)
            {
                CurrentState.LateTick();
            }
        }

        private void OnGUI()
        {
            if (!DrawDebug)
                return;

            PrintDebug();
            Print("Current State : " + CurrentState);
        }
    }
}
