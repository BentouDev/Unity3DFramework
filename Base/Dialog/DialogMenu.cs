using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class DialogMenu : DialogState
    {
        [SerializeField]
        public List<MenuItem> Items = new List<MenuItem>();

        [System.Serializable]
        public class MenuItem
        {
            [SerializeField]
            public DialogState State;

            [SerializeField]
            public string Text;
        }
    }
}
