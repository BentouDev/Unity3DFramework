using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class Controller : MonoBehaviour
    {
        [Header("Debug")]
        public bool InitOnStart;

        [Header("Pawn")]
        public bool FindByTag;
        public string PawnTag;
        public BasePawn Pawn;

        void Start()
        {
            if (!InitOnStart)
                return;

            Init();
        }

        public void Init()
        {
            if (FindByTag && Pawn == null)
            {
                var go = GameObject.FindGameObjectWithTag(PawnTag);
                Pawn = go ? go.GetComponentInChildren<BasePawn>() : null;
            }

            if (Pawn != null) Pawn.Init();

            OnInit();
        }

        public void Tick()
        {
            OnProcessControll();
        }

        public void FixedTick()
        {
            OnFixedTick();
        }

        public void LateTick()
        {
            OnLateTick();
        }

        protected virtual void OnInit()
        { }

        protected virtual void OnProcessControll()
        { }

        protected virtual void OnFixedTick()
        { }

        protected virtual void OnLateTick()
        { }
    }
}
