using System.Collections.Generic;
using System.ComponentModel;
using Malee;
using UnityEngine;

namespace Framework
{
    public class AnyEntry : GraphEntry
    {
        [System.Serializable]
        public class EntryInfo
        {
            [SerializeField]
            public string Name;

            [SerializeField]
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

        private void OnEnable()
        {
            EnsureConditionData();
        }

        void OnValidate()
        {
            EnsureConditionData();
        }

        private void EnsureConditionData()
        {
            name = "any";

            foreach (var entry in Entries)
            {
                foreach (var condition in entry.Conditions)
                {
                    if (condition)
                        condition.Graph = Graph;
                }
            }            
        }
    }
}