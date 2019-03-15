using System;
using System.Collections.Generic;
using System.Linq;
using Framework.AI;
using UnityEngine;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif

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
        public List<Parameter> Parameters = new List<Parameter>();

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

        public List<Parameter> GetParameters()
        {
            return Parameters;
        }

        public List<Parameter> GetParameters(Predicate<Parameter> predicate)
        {
            return Parameters.Where(t => predicate(t)).ToList();
        }

        public bool TryGetParameter(ParameterReference reference, out Parameter param)
        {
            param = Parameters.FirstOrDefault(p => p.Id == reference.ParameterId);
            return param != null;
        }

        public void SetToVariant(ParameterReference parameter, Variant localValue)
        {
            if (TryGetParameter(parameter, out var param))
            {
                localValue.Set(param.Value.Get());
            }
        }

        public void SetFromVariant(ParameterReference parameter, Variant localValue)
        {
            if (TryGetParameter(parameter, out var param))
            {
                param.Value.Set(localValue.Get());
            }            
        }

        public bool CanEditObject(Object obj)
        {
            return HasObject(obj);
        }

        public bool HasObject(Object obj)
        {
            switch (obj)
            {
                case ActionGraphNode asNode:
                {
                    return Nodes.Contains(asNode);
                }
                case AnyEntry asAny:
                {
                    return AnyEntryNode == asAny;
                }
                case EventEntry asEvent:
                {
                    return NamedEventEntries.Contains(asEvent);
                }
                case Condition asCondition:
                {
                    return asCondition.Graph == this;
                }
            }

            return false;            
        }

        public void SetToParameter(Variant parameter)
        {
            
        }

        public void GetFromParameter(Variant parameter)
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
                else
                    _cache.Add(parameter.Name);
            }
            
            return ValidationResult.Ok;
        }

        public static void TryRepairAsset(string path, ActionGraph asset)
        {
            bool assetChanged = false;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var asGraph = obj as ActionGraph;
                if (asGraph)
                    continue;
                
                if (asset.HasObject(obj))
                    continue;

                assetChanged = true;
                Object.DestroyImmediate(obj, true);
            }

            if (!assetChanged)
                return;

            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("CONTEXT/ActionGraph/Optimize")]
        static void RebuildCommand(MenuCommand command)
        {
            if (EditorUtility.DisplayDialog
            (
                "Optimize Asset",
                "This will remove unused nodes inside asset, but will break redo command stack. Are you sure?",
                "Optimize",
                "Cancel"
            ))
            {
                TryRepairAsset
                (
                    AssetDatabase.GetAssetPath(command.context), 
                    command.context as ActionGraph
                );
            }
        }
#endif
    }
}
