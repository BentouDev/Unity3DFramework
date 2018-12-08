using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    public abstract class Controller : BaseBehaviour
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
        [Tooltip("Should init Controller on Start? Uncheck, if pawn spawning is more complex and defined in eg. GameState")]
        public bool InitOnStart = true;

        [Tooltip("Use common static ControlSystem. Useful when no main Game and System instance is not set")]
        public bool UseCachedControllSystem = true;
        public ControllSystem System;

        [Header("Gameplay")]
        [Tooltip("Stops controller processing on game state change")]
        public bool StopOnStateChange = true;

        [Header("Pawn")]
        [Tooltip("Looks for Pawn by tag if not present")]
        [FormerlySerializedAs("FindByTag")]
        public bool FindPawnByTag;

        [Validate("ValidateFindByTag")]
        public string PawnTag;
        [Validate("ValidatePawn")]
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
            if (FindPawnByTag && Pawn == null)
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

            var  debugPos = (Camera.main ? Camera.main : Camera.current).WorldToScreenPoint(Pawn ? Pawn.transform.position : transform.position);

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
        
#if UNITY_EDITOR
        public ValidationResult ValidatePawn()
        {
            if (!Pawn && !FindPawnByTag)
                return new ValidationResult(ValidationStatus.Warning, "Pawn is not set and finding by tag is disabled.");
            return ValidationResult.Ok;
        }

        public ValidationResult ValidateFindByTag()
        {
            bool noTag = FindPawnByTag && string.IsNullOrWhiteSpace(PawnTag);
            if (noTag)
            {
                if (Pawn)
                    return new ValidationResult(ValidationStatus.Warning, "Pawn is present, but Tag shouldn't be empty if FindByTag is enabled");

                return new ValidationResult(ValidationStatus.Error, "Tag cannot be empty if FindByTag is enabled");
            }

            return ValidationResult.Ok;
        }
#endif
    }
}
