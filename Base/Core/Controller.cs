using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    public abstract class Controller : MonoBehaviour
    {
        private readonly List<string> DebugTxt = new List<string>();

        private static ControllSystem _cachedSystem;

        public enum DebugMode
        {
            None,
            WhenEnabled,
            Always
        }

        [Header("Debug")]
        public DebugMode DrawDebug;
        public bool InitOnStart;
        public bool UseCachedControllSystem = true;
        public ControllSystem System;

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

        public void KillImmediate()
        {
            System.Unregister(this);
            
            Pawn.SafeDestroy(Pawn.gameObject);
            this.SafeDestroy(gameObject);
        }

        private ControllSystem FindSystem()
        {
            var result = System;
            if (result)
                return result;

            if (UseCachedControllSystem)
                result = _cachedSystem;
            if (result)
                return result;

            result = BaseGame.Instance.GetControllers();
            if (result)
                return result;

            result = FindObjectOfType<ControllSystem>();
            return result;
        }

        public void Init()
        {
            if (FindByTag && Pawn == null)
            {
                var go = GameObject.FindGameObjectWithTag(PawnTag);
                Pawn = go ? go.GetComponentInChildren<BasePawn>() : null;
            }

            if (Pawn != null) Pawn.Init();

            System = FindSystem();
            if (System)
            {
                if (!_cachedSystem)
                    _cachedSystem = System;
                
                System.Register(this);
                OnInit();
            }
            else
            {
                Debug.LogError("Unable to initialize controller - system not found!", this);
            }
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
            if (DrawDebug == DebugMode.None)
                return;

            if (DrawDebug == DebugMode.WhenEnabled && !Enabled)
                return;
            
            OnDrawDebug();

            var  debugPos  = (Camera.main ?? Camera.current).WorldToScreenPoint(Pawn.transform.position);

            if (debugPos.z > 0)
            {
                Rect debugRect = new Rect(debugPos.x - 100, Screen.height - debugPos.y - DebugTxt.Count * 20, 500, 30);

                foreach (var txt in DebugTxt)
                {
                    GUI.Label(debugRect, txt);
                    debugRect.Set(debugRect.x, debugRect.y + 20, debugRect.width, debugRect.height);
                }
            }
            
            DebugTxt.Clear();
        }
        
        protected virtual void OnDrawDebug()
        { }

        protected void Print(string str)
        {
            DebugTxt.Add(str);
        }
    }
}
