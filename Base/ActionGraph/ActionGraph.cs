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
        [Validate("OnValidateParameters")]
        public List<GenericParameter> Parameters = new List<GenericParameter>();

        [HideInInspector]
        public AnyEntry AnyEntryNode;

        [HideInInspector]
        public List<EventEntry> NamedEventEntries = new List<EventEntry>();

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

        public void SetToParameter(GenericParameter parameter)
        {
            
        }

        public void GetFromParameter(GenericParameter parameter)
        {
            
        }

        public void UpdateFromDataset()
        {
            foreach (var node in Nodes)
            {
                node.UpdateFromParameters();
            }
        }

        public void UploadToDataset()
        {
            foreach (var node in Nodes)
            {
                node.UploadToParameters();
            }
        }

#if UNITY_EDITOR
        public ValidationResult OnValidateParameters()
        {
            HashSet<string> _cache = new HashSet<string>();
            foreach (var parameter in Parameters)
            {
                if (_cache.Contains(parameter.Name))
                    return new ValidationResult(ValidationStatus.Error,
                        $"All parameters name must be unique! Repeated '{parameter.Name}'");
            }
            
            return ValidationResult.Ok;
        }
#endif
    }
}
