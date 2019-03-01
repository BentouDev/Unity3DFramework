using System;
using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public class EventEntry : GraphEntry
    {
        [SerializeField]
        public List<Condition> Conditions;

        [SerializeField] 
        [HideInInspector] 
        public ActionGraphNode Child;
    }
}