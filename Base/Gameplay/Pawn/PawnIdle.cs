using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PawnIdle : PawnState
    {
        protected override void OnProcessMovement(Vector3 direction)
        {
            Pawn.SimulateMovement(direction);

            if (Pawn.IsGrounded)
            {
                if (direction.magnitude > Mathf.Epsilon)
                    Pawn.SwitchState<PawnMove>();
            }
            else
            {
                Pawn.SwitchState<PawnAir>();
            }
        }
    }
}
