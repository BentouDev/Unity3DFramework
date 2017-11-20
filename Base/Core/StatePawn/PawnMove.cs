using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PawnMove : PawnState
    {
        protected override void OnProcessMovement(Vector3 direction)
        {
            Pawn.SimulateMovement(direction);

            if (Pawn.IsGrounded)
            {
                if (direction.magnitude < Mathf.Epsilon)
                {
                    Pawn.SwitchState<PawnIdle>();
                }
                else
                {
                    Pawn.ForceSum.y = 0;
                }
            }
            else
            {
                Pawn.SwitchState<PawnAir>();
            }
        }
    }
}
