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

        [HideInInspector]
        public Vector3 CircleDirection;

        [HideInInspector]
        public Vector3 Damping;

        [HideInInspector]
        public Vector3 Acceleration;

        [HideInInspector]
        public Vector3 Friction;

        [HideInInspector]
        public Vector3 VelocityChange;

        [System.Serializable]
        public enum MovementType
        {
            Velocity,
            Physics,
            MovementPhysics
        }

        [SerializeField]
        public MovementType MovementFactor = MovementType.MovementPhysics;
        
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
            if (state == null)
                return;

            if (state != CurrentState)
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

        internal Vector3 LimitFlatDiagonalVector(Vector3 vector, float maxLength)
        {
//            float pythagoras = ((vector.x * vector.x) + (vector.z * vector.z));
//            if (pythagoras > (maxLength * maxLength))
//            {
//                float magnitude = Mathf.Sqrt(pythagoras);
//                float multiplier = maxLength / magnitude;
//                vector.x *= multiplier;
//                vector.y *= multiplier;
//            }

            var inputCircle = new Vector3
            (
                vector.x * Mathf.Sqrt(1 - vector.z * vector.z * 0.5f),
                0,
                vector.z * Mathf.Sqrt(1 - vector.x * vector.x * 0.5f)
            );

            return inputCircle;
        }
        
        internal void CalcMovement()
        {
            if (CurrentDirection.magnitude > Movement.MinimalVelocityThreshold)
                CircleDirection = LimitFlatDiagonalVector(new Vector3(CurrentDirection.x, 0, CurrentDirection.z), 1);

            /*Acceleration    = CircleDirection * Movement.Acceleration;
            Friction        = -Velocity.normalized * Movement.Friction;

            var flatCurDir = new Vector2(Velocity.x, Velocity.z).normalized;
            var flatNewDir = new Vector2(Acceleration.x, Acceleration.z).normalized;
            var dot        = Vector2.Dot(flatCurDir, flatNewDir);
            var dampingStr = Mathf.Cos(1 - ((dot + 1) * 0.5f));
            
            if (CircleDirection.magnitude > Movement.MinimalForceThreshold)
            {
                Damping         = dampingStr * Acceleration * Time.fixedDeltaTime;
                VelocityChange  = Acceleration * Time.fixedDeltaTime;
                VelocityChange += Damping;
            }
            else
            {
                VelocityChange = Friction * Time.fixedDeltaTime;
                VelocityChange = Vector3.ClampMagnitude(VelocityChange, Velocity.magnitude);
            }*/

            if (CurrentDirection.magnitude > Movement.MinimalForceThreshold)
            {
                CurrentSpeed += Movement.Acceleration * Time.fixedDeltaTime;
                // VelocityChange = CircleDirection * Movement.Acceleration * Time.fixedDeltaTime;
            }
            else
            {
                CurrentSpeed -= Movement.Friction * Time.fixedDeltaTime;
                // VelocityChange = -Velocity.normalized * Movement.Friction * Time.fixedDeltaTime;
            }

            CurrentSpeed = Mathf.Clamp(CurrentSpeed, 0, MaxSpeed);

            if (CurrentSpeed < Mathf.Epsilon)
                CurrentSpeed = 0;

            VelocityChange = CircleDirection * CurrentSpeed;

            if (IsGrounded && StickToGround)
            {
                Quaternion slope = Quaternion.FromToRotation(Vector3.up, LastGroundHit.normal);
                VelocityChange   = slope * VelocityChange;
            }
            
            Velocity = VelocityChange;
            Velocity = Vector3.ClampMagnitude(Velocity, MaxSpeed);

            if (Velocity.magnitude < Movement.MinimalForceThreshold)
                Velocity = Vector3.zero;
        }

        internal void SimulateMovement(Vector3 direction)
        {
            if (Vector3.Distance(CurrentDirection, direction) > FaceTreshold)
                LastFaceTime = Time.time;
            
            CurrentDirection = direction;
        }

        internal void RefreshAnimator()
        {
            if (!Anim || !Anim.isActiveAndEnabled)
                return;

            Vector3 target = Vector3.zero;
            switch (MovementFactor)
            {
                case MovementType.Velocity:
                    target = Velocity;
                    break;
                case MovementType.Physics:
                    target = Body.velocity;
                    break;
                case MovementType.MovementPhysics:
                    target = Body.velocity.magnitude < Velocity.magnitude 
                           ? Body.velocity : Velocity; 
                    break;
            }
            
            float movementFactor = target.magnitude / Movement.MaxSpeed;

            if (!string.IsNullOrEmpty(Animation.MovementBlend))
                Anim.SetFloat(Animation.MovementBlend, movementFactor);

            if (!string.IsNullOrEmpty(Animation.AirBoolean))
                Anim.SetBool(Animation.AirBoolean, !IsGrounded);

            if (!string.IsNullOrEmpty(Animation.MoveBoolean))
                Anim.SetBool(Animation.MoveBoolean, Mathf.Abs(movementFactor) > 0.1f);
        }

        protected override void OnStop()
        {
            Velocity = Vector3.zero;
            CurrentSpeed = 0;
            Body.velocity = Vector3.zero;

            SwitchState<PawnIdle>();

            if (CurrentState)
                CurrentState.Tick();
        }

        protected override void OnTick()
        {
            if (IsAlive())
            {
                if (CurrentState != null)
                {
                    CurrentState.Tick();
                }
            }

            RefreshAnimator();
        }

        protected override void OnFixedTick()
        {
            if (IsAlive())
            {
                if (CurrentState != null)
                {
                    CurrentState.FixedTick();
                }
            }

            CalcMovement();
        }

        protected override void OnLateTick()
        {
            if (!IsAlive())
                return;
            
            FaceMovementDirection(20);

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
