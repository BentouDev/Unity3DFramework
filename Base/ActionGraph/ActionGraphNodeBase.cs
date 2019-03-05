using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework
{
    public abstract class ActionGraphNodeBase : ParametrizedScriptableObject
    {
        [SerializeField]
        [HideInInspector]
        public ActionGraph Graph;

#if UNITY_EDITOR
        [SerializeField]
        [HideInInspector]
        public Vector2 EditorPosition;
#endif

        public override IDataSetProvider GetProvider()
        {
            return Graph;
        }
    }
}