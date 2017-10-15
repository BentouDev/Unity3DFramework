using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    [RequireComponent(typeof(StatePawn))]
    public class PawnVelocityDebug : MonoBehaviour
    {
        public bool DrawVectors;
        public bool PrintValues;
        public Color Velocity = Color.yellow;
        public Color Acceleration = Color.blue;
        public Color Friction = Color.red;
        public Color Damping = Color.green;

        private StatePawn Pawn;

        void Start()
        {
            Pawn = GetComponent<StatePawn>();
        }

        void Update()
        {
            if (!DrawVectors)
                return;

            var flatCurDir = new Vector2(Pawn.Velocity.x, Pawn.Velocity.z).normalized;
            var flatNewDir = new Vector2(Pawn.Acceleration.x, Pawn.Acceleration.z).normalized;
            var dot = Vector2.Dot(flatCurDir, flatNewDir);

            var newDot = 1 - ((dot + 1) * 0.5f);

            Debug.DrawRay(Pawn.transform.position, Pawn.Velocity, Velocity);
            Debug.DrawRay(Pawn.transform.position, Pawn.Acceleration, Acceleration);
            Debug.DrawRay(Pawn.transform.position, Pawn.Friction, Friction);
            Debug.DrawRay(Pawn.transform.position, Pawn.Damping, Damping);
        }

        void OnGUI()
        {
            if (!PrintValues)
                return;

            var flatCurDir = new Vector2(Pawn.Velocity.x, Pawn.Velocity.z).normalized;
            var flatNewDir = new Vector2(Pawn.Acceleration.x, Pawn.Acceleration.z).normalized;
            var dot = Vector2.Dot(flatCurDir, flatNewDir);

            var newDot = 1 - ((dot + 1) * 0.5f);

            var rad = 1;//Mathf.PI / 180;
            var cosDot = Mathf.Cos(rad * newDot);


            Pawn.Print("cosD : " + cosDot);
            Pawn.Print("nDOT : " + newDot);
            Pawn.Print("DOT : " + dot);
            Pawn.Print("Damping : " + Pawn.Damping);
            Pawn.Print("FlatVelocity : " + Pawn.Velocity.magnitude);
            Pawn.Print("Acceleration : " + Pawn.Acceleration);
            Pawn.Print("Friction : " + Pawn.Friction);
            Pawn.Print("VelocityChange : " + Pawn.VelocityChange);
            Pawn.Print("Friction : " + Pawn.Friction);
            Pawn.Print("Velocity % : " + (100 * Pawn.Velocity.magnitude / Pawn.Movement.MaxSpeed) + "%");
        }
    }
}
