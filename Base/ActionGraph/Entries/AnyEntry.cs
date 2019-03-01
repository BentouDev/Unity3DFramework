using System.Collections.Generic;
using System.ComponentModel;
using Malee;
using UnityEngine;

namespace Framework
{
    public class AnyEntry : GraphEntry
    {
        [System.Serializable]
        public struct EntryInfo
        {
            [SerializeField]
            public string Name;

            [SerializeField]
            [ReorderableAttribute]
            public TConditionList Conditions;

            [SerializeField] 
            [HideInInspector] 
            public ActionGraphNode Child;
        }

        [SerializeField]
        [ReorderableAttribute]
        public TEntryList Entries = new TEntryList();
        
        [System.Serializable]
        public class TEntryList : ReorderableArray<EntryInfo>
        { }
        
        [System.Serializable]
        public class TConditionList : ReorderableArray<Condition>
        { }

        private void OnEnable()
        {
            name = "any";
        }

        void OnValidate()
        {
            name = "any";
        }
    }
}