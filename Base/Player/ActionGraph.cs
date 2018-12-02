using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework
{
#if UNITY_EDITOR
    [CreateAssetMenu(fileName = "New Action Graph", menuName = "Data/Action Graph")]
#endif
    public class ActionGraph : BaseScriptableObject, IDataSetProvider
    {
        [SerializeField]
        public List<ActionGraphNode> Nodes = new List<ActionGraphNode>();

        [SerializeField]
        public List<GenericParameter> Parameters = new List<GenericParameter>();

        [HideInInspector]
        [SerializeField]
        public SerializedType InputType;

        [System.Serializable]
        public class EntryPoint
        {
            [SerializeField]
            public GenericParameter Input;

            [SerializeField]
            public List<ActionGraphNode> Nodes = new List<ActionGraphNode>();
        }

        [SerializeField]
        public List<EntryPoint> Inputs = new List<EntryPoint>();

#if UNITY_EDITOR
        [HideInInspector]
        public Vector2 EditorPos;
        
        [HideInInspector]
        public Vector2 BeginEditorPos;
#endif

        public override IDataSetProvider GetProvider()
        {
            return this;
        }

        public List<GenericParameter> GetParameters()
        {
            return Parameters;
        }

        public List<GenericParameter> GetParameters(Predicate<GenericParameter> predicate)
        {
            return Parameters.Where(t => predicate(t)).ToList();
        }

        public bool CanEditObject(Object obj)
        {
            return HasObject(obj);
        }

        public bool HasObject(Object obj)
        {
            var asNode = obj as ActionGraphNode;
            if (asNode)
            {
                return Nodes.Contains(asNode);
            }

            return false;            
        }
    }
}
