using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class PawnAir : PawnState
    {
        private float AirTime;

        protected override void OnProcessMovement(Vector3 direction)
        {
            Pawn.SimulateMovement(Pawn.CurrentDirection);

            if (AirTime < Mathf.Epsilon)
            {
                Pawn.SwitchState<PawnFall>();
            }
            else
            {
                Pawn.ForceSum.y = 0;
            }
        }

        protected override void OnTick()
        {
            ProcessTime(ref AirTime);
        }
    }
}
