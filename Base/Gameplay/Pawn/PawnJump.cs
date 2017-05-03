using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PawnJump : PawnState
    {
        private float JumpTime;

        protected override void OnStart()
        {
            JumpTime = Pawn.Jump.JumpDuration;
        }

        protected override void OnProcessMovement(Vector3 direction)
        {
            Pawn.SimulateMovement(Pawn.CurrentDirection);

            if (JumpTime < Mathf.Epsilon)
            {
                Pawn.SwitchState<PawnAir>();
            }
            else
            {
                Pawn.ForceSum.y = Pawn.Jump.JumpSpeed;
            }
        }

        protected override void OnTick()
        {
            ProcessTime(ref JumpTime);
        }
    }
}
