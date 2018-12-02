using System.Collections.Generic;
using UnityEngine;

namespace Framework
{
    public abstract class ActionGraphNode : BaseScriptableObject
    {
        [SerializeField]
        public List<ActionGraphNode> Connections = new List<ActionGraphNode>();

        public ActionGraph Graph;

        [SerializeField]
        [HideInInspector]
        public Vector2 EditorPosition;

        public override IDataSetProvider GetProvider()
        {
            return Graph;
        }
    }
}