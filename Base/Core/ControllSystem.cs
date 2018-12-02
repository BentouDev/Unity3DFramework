using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class ControllSystem : Framework.BaseBehaviour, ITickable
    {
        public bool InitOnStart;
        public bool EnableOnStart;
        public bool AutoUpdate = false;

        private int DisableCounter;

        public bool IsEnabled => DisableCounter == 0;

        private bool IsInitialized;
     
        public List<Controller> AllControllers;
   
        void Start()
        {
            IsInitialized = false;
            
            if (InitOnStart) 
                Init();
            
            if (EnableOnStart) 
                Enable();
        }

        public void Init()
        {
            if (IsInitialized)
                return;

            if (AllControllers == null)
                AllControllers = new List<Controller>();
            else
                AllControllers.Clear();

            AllControllers.AddRange(FindObjectsOfType<Controller>());

            foreach (Controller controller in AllControllers)
            {
                controller.Init();
            }

            IsInitialized = true;
        }

        public void Register(Controller controller)
        {
            if (!AllControllers.Contains(controller))
                AllControllers.Add(controller);
        }

        public void Unregister(Controller controller)
        {
            AllControllers.Remove(controller);
        }

        public void Enable()
        {
            DisableCounter = Mathf.Max(DisableCounter - 1, 0);
            if (DisableCounter == 0)
            {
                foreach (Controller controller in AllControllers)
                {
                    controller.EnableInput();
                }
            }
        }

        public void Disable()
        {
            DisableCounter++;

            foreach (Controller controller in AllControllers)
            {
                controller.DisableInput();
            }
        }

        public void Tick()
        {
            if (!IsInitialized)
                return;
            
            foreach (Controller controller in AllControllers)
            {
                controller.Tick();
            }
        }

        public void FixedTick()
        {
            if (!IsInitialized)
                return;

            foreach (Controller controller in AllControllers)
            {
                controller.FixedTick();
            }
        }

        public void LateTick()
        {
            if (!IsInitialized)
                return;

            foreach (Controller controller in AllControllers)
            {
                controller.LateTick();
            }
        }

        void Update()
        {
            if (AutoUpdate) Tick();
        }

        void FixedUpdate()
        {
            if (AutoUpdate) FixedTick();
        }

        void LateUpdate()
        {
            if (AutoUpdate) LateTick();
        }
    }
}