using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class EventEntry : GraphEntry
    {
        [SerializeField]
        public TConditionList Conditions = new TConditionList();

        [SerializeField] 
        [HideInInspector] 
        public ActionGraphNode Child;

        private void OnEnable()
        {
            EnsureConditionData();
        }

        private void OnValidate()
        {
            EnsureConditionData();
        }

        private void EnsureConditionData()
        {
            foreach (var condition in Conditions)
            {
                if (condition)
                    condition.Graph = Graph;
            }
        }
    }
}