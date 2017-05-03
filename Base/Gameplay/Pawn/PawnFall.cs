using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PawnFall : PawnState
    {
        protected override void OnProcessMovement(Vector3 direction)
        {
            Pawn.SimulateMovement(Pawn.CurrentDirection);

            if (!Pawn.IsGrounded)
                return;

            if (direction.magnitude > 0)
            {
                Pawn.SwitchState<PawnMove>();
            }
            else
            {
                Pawn.SwitchState<PawnIdle>();
            }
        }

        protected override void OnFixedTick()
        {
            if (Pawn.IsGrounded)
                return;

            Pawn.ForceSum.y = Pawn.LastForceSum.y + Pawn.Movement.Gravity * Time.deltaTime;
        }
    }
}
