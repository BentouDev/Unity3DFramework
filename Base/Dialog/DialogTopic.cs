using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    [System.Serializable]
    public class DialogTopic : DialogState
    {
        [SerializeField]
        public List<Item> Items = new List<Item>();

        [System.Serializable]
        public abstract class Item
        {
            
        }

        [System.Serializable]
        public class SayItem : Item
        {
            [SerializeField]
            public DialogActorSlot Actor;

            [SerializeField]
            public string Text;
        }

        [System.Serializable]
        public class InvokeItem : Item
        {
            [SerializeField]
            public DialogFunctionSlot Function;
        }

        [System.Serializable]
        public class ExitItem : Item
        {
            
        }

        [System.Serializable]
        public class GotoItem : Item
        {
            [SerializeField]
            public DialogState State;
        }
    }
}
