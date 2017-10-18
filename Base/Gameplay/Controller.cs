using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class Controller : MonoBehaviour
    {
        [Header("Debug")]
        public bool InitOnStart;
        public bool DrawDebug;

        [Header("Gameplay")]
        public bool StopOnStateChange = true;

        [Header("Pawn")]
        public bool FindByTag;
        public string PawnTag;
        public BasePawn Pawn;

        protected bool Enabled { get; private set; }

        void Start()
        {
            if (!InitOnStart)
                return;

            Init();
        }

        public void EnableInput()
        {
            Enabled = true;
        }

        public void DisableInput()
        {
            Enabled = false;
        }

        public void Init()
        {
            if (FindByTag && Pawn == null)
            {
                var go = GameObject.FindGameObjectWithTag(PawnTag);
                Pawn = go ? go.GetComponentInChildren<BasePawn>() : null;
            }

            if (Pawn != null) Pawn.Init();

            var system = FindObjectOfType<ControllSystem>();
            if (system)
                system.Register(this);

            OnInit();
        }

        public void Stop()
        {
            OnStop();
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

        protected virtual void OnStop()
        { }

        protected virtual void OnProcessControll()
        { }

        protected virtual void OnFixedTick()
        { }

        protected virtual void OnLateTick()
        { }

        protected void OnDestroy()
        {
            var system = FindObjectOfType<ControllSystem>();
            if (system)
                system.Unregister(this);
        }

        void OnGUI()
        {
            if (!DrawDebug)
                return;

            LastY = 0;

            OnDrawDebug();
        }

        protected virtual void OnDrawDebug()
        { }

        public void Print(string str)
        {
            GUI.Label(new Rect(Screen.width - 200, Screen.height - 40 - LastY, 400, 30), str);
            LastY -= 20;
        }

        protected float LastY;
    }
}
