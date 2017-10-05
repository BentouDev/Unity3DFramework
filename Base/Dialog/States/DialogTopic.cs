using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    [System.Serializable]
    public partial class DialogTopic : DialogState
    {
        public List<Item> Items = new List<Item>();
        
        public abstract class Item : ScriptableObject
        { }
        
        public override void DestroyChildren()
        {
            foreach (var item in Items)
            {
                DestroyImmediate(item, true);
            }

            Items.Clear();
        }
    }
}
