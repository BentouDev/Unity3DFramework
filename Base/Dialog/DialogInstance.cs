using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework
{
    public class DialogInstance : MonoBehaviour
    {
        public Dialog Dialog;
        
        private void OnValidate()
        {
            ReloadDialog();

            foreach (var info in Functions)
            {
                info.FunctionCount = info.Function.GetPersistentEventCount();
            }
        }
        
        [System.Serializable]
        public enum ActorType
        {
            Static,
            Dynamic
        }

        [System.Serializable]
        public class ActorInfo
        {
            [SerializeField]
            public string Name;

            [SerializeField]
            public ActorType Type;

            [SerializeField]
            public DialogActor Actor;

            [SerializeField]
            public string Tag;
        }

        [System.Serializable]
        public class FunctionInfo
        {
            [SerializeField]
            public string Name;

            [SerializeField]
            public UnityEvent Function;

            [SerializeField]
            public int FunctionCount;
        }

        [SerializeField]
        public List<ActorInfo> Actors;

        [SerializeField]
        public List<FunctionInfo> Functions;

        public bool ReloadDialog()
        {
            if (Dialog == null)
                return false;

            List<ActorInfo> actors = new List<ActorInfo>();
            foreach (var actor in Dialog.Actors)
            {
                var instance = Actors.FirstOrDefault(a => a.Name.Equals(actor.name));
                if (instance != null)
                {
                    actors.Add(instance);
                }
                else
                {
                    actors.Add(new ActorInfo(){ Name = actor.name });
                }
            }

            List<FunctionInfo> funcs = new List<FunctionInfo>();
            foreach (var function in Dialog.Functions)
            {
                var instance = Functions.FirstOrDefault(f => f.Name.Equals(function.name));
                if (instance != null)
                {
                    funcs.Add(instance);
                }
                else
                {
                    funcs.Add(new FunctionInfo(){ Name = function.name });
                }
            }
            
            Actors = actors;
            Functions = funcs;
            return true;
        }

#if UNITY_EDITOR
        [MenuItem("CONTEXT/Dialog/Reload")]
        static void ReloadCommand(MenuCommand command)
        {
            DialogInstance dlg = (DialogInstance)command.context;
            if (dlg.ReloadDialog())
                Debug.Log("Reloaded dialog instance " + dlg.name);
        }
#endif
    }
}
