using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class ControllSystem : MonoBehaviour, ITickable
    {
        public List<Controller> AllControllers;

        public bool InitOnStart;
        public bool EnableOnStart;
        public bool AutoUpdate = false;

        private int DisableCounter;

        public bool IsEnabled => DisableCounter == 0;

        void Start()
        {
            if (InitOnStart) 
                Init();
            
            if (EnableOnStart) 
                Enable();
        }

        public void Init()
        {
            if (AllControllers == null)
                AllControllers = new List<Controller>();
            else
                AllControllers.Clear();

            AllControllers.AddRange(FindObjectsOfType<Controller>());

            foreach (Controller controller in AllControllers)
            {
                controller.Init();
            }
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
            foreach (Controller controller in AllControllers)
            {
                controller.Tick();
            }
        }

        public void FixedTick()
        {
            foreach (Controller controller in AllControllers)
            {
                controller.FixedTick();
            }
        }

        public void LateTick()
        {
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