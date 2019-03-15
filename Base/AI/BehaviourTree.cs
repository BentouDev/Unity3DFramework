using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Framework.AI
{
#if UNITY_EDITOR
    [CreateAssetMenu(menuName = "Data/Behaviour Tree", fileName = "New Behaviour Tree")]
#endif
    public class BehaviourTree : BaseScriptableObject
    {
#if UNITY_EDITOR
        [SerializeField] public Vector2 EditorPos = Vector2.zero;
#endif

        [SerializeField]
        public List<Parameter> Parameters = new List<Parameter>();

        [SerializeField]
        public List<BehaviourTreeNode> Nodes = new List<BehaviourTreeNode>();

        [HideInInspector]
        [SerializeField]
        public BehaviourTreeNode RootNode;
        
        public void BuildEmptyBlackboard(Blackboard blackboard)
        {
            Variant.BuildKnownTypeList();
            
            foreach (Parameter parameter in Parameters)
            {
                blackboard.InsertFromParameter(parameter);
            }
        }
        
        public void GetAllEnabledNodes(List<BehaviourTreeNode> nodes)
        {
            ProcessNodeRecurrent(RootNode, nodes);
        }

        public List<BehaviourTreeNode> GetAllEnabledNodes()
        {
            List<BehaviourTreeNode> nodes = new List<BehaviourTreeNode>();

            GetAllEnabledNodes(nodes);

            return nodes;
        }

        private void ProcessNodeRecurrent(BehaviourTreeNode node, List<BehaviourTreeNode> nodes)
        {
            nodes.Add(node);

            if (!node.IsParentNode())
                return;

            var children = node.AsParentNode().GetChildNodes();
            foreach (BehaviourTreeNode child in children)
            {
                ProcessNodeRecurrent(child, nodes);
            }
        }

        public bool Contains(BehaviourTreeNode node)
        {
            return FindNodeRecursive(RootNode, node);
        }

        private bool FindNodeRecursive(BehaviourTreeNode node, BehaviourTreeNode toFind)
        {
            if (node == null)
                return false;

            if (node == toFind)
                return true;

            if (node.IsParentNode())
            {
                var childNodes = node.AsParentNode().GetChildNodes();
                for (int i = 0; i < childNodes.Count; i++)
                {
                    if (FindNodeRecursive(childNodes[i], toFind))
                        return true;
                }
            }

            return false;
        }

#if UNITY_EDITOR        
        public static void TryRepairAsset(string path, BehaviourTree asset)
        {
            bool assetChanged = false;
            foreach (var obj in AssetDatabase.LoadAllAssetsAtPath(path))
            {
                var asTree = obj as BehaviourTree;
                if (asTree)
                    continue;
                
                var asNode = obj as BehaviourTreeNode;
                if (asNode)
                {
                    if (asset.Nodes.Contains(asNode))
                        continue;
                    
                    foreach (var node in asset.Nodes.Where(n => n.IsParentNode()))
                    {
                        node.AsParentNode().GetChildNodes().Remove(asNode);
                    }
                }

                assetChanged = true;
                Object.DestroyImmediate(asNode, true);
            }

            if (!assetChanged) 
                return;
            
            EditorUtility.SetDirty(asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        [MenuItem("CONTEXT/BehaviourTree/Optimize")]
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
                    command.context as BehaviourTree
                );
            }
        }
#endif
    }
}
