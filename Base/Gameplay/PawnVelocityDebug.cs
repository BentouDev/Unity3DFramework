using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    [RequireComponent(typeof(StatePawn))]
    public class PawnVelocityDebug : MonoBehaviour
    {
        public bool Draw;
        public Color Velocity = Color.red;
        public Color Direction = Color.yellow;
        public Color Speed = Color.blue;

        private StatePawn Pawn;

        void Start()
        {
            Pawn = GetComponent<StatePawn>();
        }

        void Update()
        {
            if (!Draw)
                return;

            Debug.DrawRay(Pawn.transform.position, Pawn.Velocity, Velocity);
            Debug.DrawRay(Pawn.transform.position, Pawn.CurrentDirection.normalized, Direction);
            Debug.DrawRay(Pawn.transform.position, Pawn.CurrentDirection * Pawn.CurrentSpeed, Speed);
        }
    }
}
