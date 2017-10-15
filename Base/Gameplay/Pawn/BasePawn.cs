using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class BasePawn : MonoBehaviour, ITickable
    {
        [Header("Debug")]
        public bool InitOnStart;
        public bool DrawDebug;

        [Header("Base Components")]
        public Rigidbody Body;
        public Damageable Damageable;
        public Animator Anim;

        [Header("Ground Checking")]
        public LayerMask RaycastMask;
        public Vector3 RaycastOrigin;
        public float RaycastRadius = 1;
        public float RaycastLength = 1;
        protected RaycastHit LastGroundHit;

        public enum FaceMode
        {
            Velocity,
            Physics,
            Direction
        }

        public FaceMode Face;

        [System.Serializable]
        public struct MovementInfo
        {
            [SerializeField]
            public float MaxSpeed;

            [SerializeField]
            public float Acceleration;

            [SerializeField]
            public float Friction;

            [SerializeField]
            public float MinimalForceThreshold;
            
            [SerializeField]
            public float MinimalVelocityThreshold;

            [SerializeField]
            public float Gravity;
        }

        [System.Serializable]
        public struct JumpInfo
        {
            [SerializeField]
            public float JumpSpeed;

            [SerializeField]
            public float JumpHeight;

            public float JumpDuration => JumpHeight / JumpSpeed;
        }

        [System.Serializable]
        public struct AnimationInfo
        {
            [SerializeField]
            public string MovementBlend;

            [SerializeField]
            public string AirBoolean;

            [SerializeField]
            public string MoveBoolean;
        }

        [SerializeField]
        [Header("Movement")]
        public bool StickToGround;

        public MovementInfo Movement;
        public JumpInfo Jump;

        [HideInInspector]
        public Vector3 ForceSum;

        [HideInInspector]
        public Vector3 LastForceSum;

        [HideInInspector]
        public Vector3 Velocity;

        [HideInInspector]
        public Vector3 LastVelocity;

        [HideInInspector]
        public Vector3 LastPosition;

        [HideInInspector]
        public float CurrentSpeed;

        [HideInInspector]
        public Vector3 CurrentDirection;

        public Vector3 DeltaPosition => Body.position - LastPosition;

        public bool IsGrounded { get; protected set; }

        public Vector3 DesiredForward { get; set; }

        [Header("Animation")]
        public AnimationInfo Animation;
        
        Vector3[] GetRaycastOffsets()
        {
            return new []
            {
                transform.rotation * (Vector3.forward * RaycastRadius),
                transform.rotation * (Vector3.back    * RaycastRadius),
                transform.rotation * (Vector3.right   * RaycastRadius),
                transform.rotation * (Vector3.left    * RaycastRadius)
            };
        }

        private void Start()
        {
            if (!InitOnStart)
                return;

            Init();
        }

        public void Init()
        {
            this.TryInit(ref Damageable);
            this.TryInit(ref Body);

            LastPosition     = transform.position;
            CurrentDirection = transform.forward;

            OnInit();
        }

        public bool IsAlive()
        {
            return Damageable == null || Damageable.IsAlive;
        }

        public virtual float GetMaxSpeed()
        {
            return Movement.MaxSpeed;
        }

        public void CheckGrounded()
        {
            var orign = transform.TransformPoint(RaycastOrigin);

            IsGrounded = Physics.Raycast(orign, Vector3.down, out LastGroundHit, RaycastLength, RaycastMask);
            if (IsGrounded)
            {
                // Debug.DrawLine(LastGroundHit.point, LastGroundHit.point + LastGroundHit.normal, Color.cyan);
            }
            else
            {
                foreach (Vector3 offset in GetRaycastOffsets())
                {
                    IsGrounded |= Physics.Raycast(orign + offset, Vector3.down, out LastGroundHit, RaycastLength, RaycastMask);
                    if (IsGrounded)
                        break;
                }
            }
        }

        public void ProcessMovement(Vector3 direction)
        {
            OnProcessMovement(direction);
        }

        protected virtual void ApplyMovement()
        {
            Body.velocity = Velocity + ForceSum;

            LastForceSum = ForceSum;
            LastVelocity = Velocity;
            LastPosition = Body.position;

            ForceSum = Vector3.Lerp(ForceSum, Vector3.zero, Time.fixedDeltaTime);
            if (Mathf.Abs(ForceSum.magnitude) < Movement.MinimalForceThreshold)
            {
                ForceSum = Vector3.zero;
            }
        }
        
        public void FaceMovementDirection(float speed)
        {
            Vector3 target = Vector3.zero;

            switch (Face)
            {
                case FaceMode.Direction:
                    target = CurrentDirection;
                    break;
                case FaceMode.Velocity:
                    target = Velocity;
                    break;
                case FaceMode.Physics:
                    target = Body.velocity;
                    break;
            }
            
            if (target.magnitude > 0)
            {
                //var velDir       = new Vector2(velocity.x, velocity.z).normalized;
                //var dot          = Vector2.Dot(new Vector2(transform.forward.x, transform.forward.z), velDir);
                //var coeff        = 1 - ((dot + 1) * 0.5f);

                var flatVelocity = new Vector3(target.x, 0, target.z).normalized;

                transform.forward = Vector3.Lerp 
                (
                    transform.forward,
                    flatVelocity,
                    Time.fixedDeltaTime * speed
                );
            }
        }

        public void Tick()
        {
            OnTick();
        }

        public void Stop()
        {
            OnStop();
        }

        public void FixedTick()
        {
            CheckGrounded();
            OnFixedTick();

            ApplyMovement();
        }

        public void LateTick()
        {
            OnLateTick();
        }

        protected virtual void OnInit()
        { }

        protected virtual void OnStop()
        { }

        protected virtual void OnProcessMovement(Vector3 direction)
        { }

        protected virtual void OnTick()
        { }

        protected virtual void OnFixedTick()
        { }

        protected virtual void OnLateTick()
        { }

        private void Reset()
        {
            Movement = new MovementInfo()
            {
                Gravity = -9.81f,
                MaxSpeed = 9,
                //Friction = 6,
                //Acceleration = 6,
                MinimalVelocityThreshold = 0.1f,
                MinimalForceThreshold = 0.01f
            };

            Jump = new JumpInfo()
            {
                JumpHeight = 2,
                JumpSpeed = 9
            };

            Animation = new AnimationInfo()
            {
                MovementBlend = "Forward",
                AirBoolean = "InAir"
            };
        }
        
        private void OnDrawGizmosSelected()
        {
            var orign = transform.TransformPoint(RaycastOrigin);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(orign, orign + Vector3.down * RaycastLength);

            foreach (Vector3 offset in GetRaycastOffsets())
            {
                Gizmos.DrawLine(orign + offset, orign + offset + Vector3.down * RaycastLength);
            }

            Gizmos.DrawWireSphere(orign, RaycastRadius);
        }

        protected void PrintDebug()
        {
            LastY = 0;
            Print("Grounded : " + IsGrounded);
            Print("Direction : " + CurrentDirection);
            Print("Velocity : " + Velocity);
            Print("ForceSum : " + ForceSum);
            Print("Forward : " + transform.forward);
            Print("Desired Forward : " + DesiredForward);
        }

        public void Print(string str)
        {
            GUI.Label(new Rect(10,10+LastY,400,30), str);
            LastY += 20;
        }
        
        private float LastY;
    }
}
