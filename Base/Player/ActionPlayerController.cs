using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public class ActionPlayerController : BasePlayerController, IActionGraphOwner
    {
        [Header("LockOn")]
        public Transform CurrentLockOnTarget;
        
        private Vector2 _currentInput;
        public Vector2 CurrentInput => _currentInput;

        [Header("Input")]
        [RequireValue]
        public GenericInputBuffer Buffer;

        [RequireValue]
        public InputScheme Scheme;

        [Header("Actions")]
        [RequireValue]
        public ActionGraph Actions;

        protected override void OnInit()
        {
            base.OnInit();
            Buffer.DefineButtons(Scheme.Buttons.ConvertAll(b => b.Name));
        }

        protected void GatherInput()
        {
            _currentInput.x = Input.GetAxis(Scheme.MoveX);
            _currentInput.y = Input.GetAxis(Scheme.MoveY);

            foreach (var button in Scheme.Buttons)
            {
                Buffer.HandleButton(Input.GetKey(button.Key), button.Name);
            }
        }

        protected override void OnProcessControll()
        {
            if (!Pawn)
                return;

            Vector3 direction = Vector3.zero;

            if (Enabled)
            {
                GatherInput();

                if (CurrentLockOnTarget)
                {
                    var rawDistance = transform.position - CurrentLockOnTarget.position;
                    Pawn.DesiredForward = -Vector3.Normalize(rawDistance);
                }

                var flatVelocity = new Vector3(CurrentInput.x, 0, CurrentInput.y);
                direction = Quaternion.LookRotation(Vector3.Normalize(Pawn.DesiredForward)) * flatVelocity;
                
                direction.Normalize();
            }
            
            Pawn.ProcessMovement(direction);
            Pawn.Tick();
        }
        
        protected override void OnStop()
        {
            Pawn.Stop();
        }

        protected override void OnFixedTick()
        {
            Pawn.FixedTick();
        }

        protected override void OnLateTick()
        {
            var camTransform = PawnCamera.transform;

            if (Enabled)
            {
                if (camTransform.forward.magnitude > 0)
                {
                    Pawn.DesiredForward = Vector3.Slerp(Pawn.DesiredForward, new Vector3(
                            camTransform.forward.x,
                            0,
                            camTransform.forward.z
                        ), Time.deltaTime * 10);
                }

                PawnCamera.OnUpdate();
            }

            Pawn.LateTick();
        }

        [SerializeField]
        [HideInInspector]
        public ParameterBinder Binder = new ParameterBinder();

        public BindingContext GetBindingContext(ActionGraph graph)
        {
            return new BindingContext(Binder, graph.GetProvider());
        }

        public bool IsOwnerOf(ActionGraph graph)
        {
            return Actions == graph;
        }
    }
}