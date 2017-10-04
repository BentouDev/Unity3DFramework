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
    public class Dialog : ScriptableObject
    {
        [System.Serializable] public class SerializedStates : SerializedDictionary<string, DialogState> { }

        [SerializeField]
        public DialogState FirstState;

        [SerializeField]
        public SerializedStates States = new SerializedStates();

        [SerializeField]
        public List<DialogFunctionSlot> Functions = new List<DialogFunctionSlot>();

        [SerializeField]
        public List<DialogActorSlot> Actors = new List<DialogActorSlot>();

        [SerializeField]
        [HideInInspector]
        public string DialogScriptFile;
        
#if UNITY_EDITOR
        public bool RebuildFromScript(bool clearOnEmpty = false)
        {
            if (string.IsNullOrEmpty(DialogScriptFile) && !clearOnEmpty)
                return true;

            bool isEmpty = string.IsNullOrEmpty(DialogScriptFile);
            if (isEmpty)
            {
                Clear();
                return true;
            }

            Clear(false);
            
            DialogParser parser = new DialogParser(this);
            return parser.ProcessFile(DialogScriptFile);
        }

        public void Clear(bool immediateSave = true)
        {
            foreach (var state in States.Dictionary.Values)
            {
                DestroyImmediate(state, true);
            }

            foreach (var actor in Actors)
            {
                DestroyImmediate(actor, true);
            }

            foreach (var func in Functions)
            {
                DestroyImmediate(func, true);
            }

            States.Dictionary.Clear();
            Functions.Clear();
            Actors.Clear();

            if (immediateSave)
                AssetDatabase.SaveAssets();
        }

        [MenuItem("CONTEXT/Dialog/Rebuild")]
        static void RebuildCommand(MenuCommand command)
        {
            Dialog dlg = (Dialog)command.context;
            if (dlg.RebuildFromScript())
                Debug.Log("Rebuilded dialog " + dlg.name);
            else
                Debug.LogError("Failed to build dialog " + dlg.name);
        }
#endif
    }
}
