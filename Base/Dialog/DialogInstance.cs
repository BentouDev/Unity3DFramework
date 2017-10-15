using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using System;

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
            Dynamic,
            This
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
        public UnityEvent OnDialogEnd;

        [SerializeField]
        public UnityEvent OnDialogStart;

        [SerializeField]
        public List<ActorInfo> Actors = new List<ActorInfo>();

        [SerializeField]
        public List<FunctionInfo> Functions = new List<FunctionInfo>();

        private Dictionary<string, FunctionInfo> FuncDic  = new Dictionary<string, FunctionInfo>();
        private Dictionary<string, ActorInfo>    ActorDic = new Dictionary<string, ActorInfo>();

        public void Init()
        {
            CleanUp();

            foreach (var actor in Actors.Where(a => a.Type == ActorType.This))
            {
                actor.Actor = GetComponentInChildren<DialogActor>() ?? GetComponentInParent<DialogActor>();
                if (!actor.Actor)
                    Debug.LogError("Unable to set dialog actor to this!", this);
            }

            foreach (ActorInfo actor in Actors)
            {
                ActorDic[actor.Name] = actor;
            }

            foreach (FunctionInfo function in Functions)
            {
                FuncDic[function.Name] = function;
            }
        }

        public void CleanUp()
        {
            foreach (var actor in Actors.Where(a => a.Type == ActorType.Dynamic))
            {
                actor.Actor = null;
            }

            ActorDic.Clear();
            FuncDic.Clear();
        }

        internal void Invoke(DialogFunctionSlot function)
        {
            var func = FuncDic[function.name];
            func?.Function.Invoke();
        }

        public string GetActorName(DialogActorSlot sayActor)
        {
            var actor = ActorDic[sayActor.name];
            return actor != null && actor.Actor != null ? actor.Actor.DisplayedName : sayActor.name;
        }

        public void StartThisDialog()
        {
            MainGame.Instance.StartDialog(this);
        }

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
