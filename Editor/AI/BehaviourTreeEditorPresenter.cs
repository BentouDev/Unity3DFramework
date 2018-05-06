using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Framework.Editor;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.AI.Editor
{
    public class BehaviourTreeEditorPresenter : EditorPresenter
    {
        protected BehaviourTreeEditorView View;
        private BehaviourTree TreeAsset;
        private string AssetPath;

        private bool _duringRecreate;
        private bool _doRepair;

        public class ConnectEvent
        {
            public BehaviourTreeNode Parent;
            public BehaviourTreeNode Child;
        }
        
        public readonly EventQueue<ConnectEvent> OnConnectNodesQueue = new EventQueue<ConnectEvent>();

        internal struct Model
        {
            public BehaviourTree TreeAsset;
            public string AssetPath;
        }

        public BehaviourTreeEditorPresenter(BehaviourTreeEditorView view)
        {
            View = view;
            
            OnConnectNodesQueue.Reassign(data =>
            {
                if (!data.Parent || !data.Child)
                    return false;
                
                Undo.RecordObject(data.Parent, $"Connect nodes {data.Child.Name} to {data.Parent.Name}");
                data.Parent.AsParentNode().AddOrSetChild(data.Child);

                return true;
            });
        }

        private readonly Stack<int> _indexStack = new Stack<int>();
        internal override void OnUndoRedo()
        {
            _indexStack.Clear();
            for (int i = 0; i < TreeAsset.Nodes.Count; i++)
            {
                if (TreeAsset.Nodes[i] == null)
                    _indexStack.Push(i);
            }
            
            while (_indexStack.Count != 0)
                TreeAsset.Nodes.RemoveAt(_indexStack.Pop());
            
            RecreateNodes();
        }

        internal bool NeedsRepair(string path, BehaviourTree asset)
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
                }

                assetChanged = true;
            }

            return assetChanged;
        }

        internal override void OnEnable()
        {
            ReloadAssetFromSelection();
        }

        internal override void OnFocus()
        {
            ReloadAssetFromSelection();
        }

        internal override void OnProjectChange()
        {
            ReloadAssetFromSelection();
        }

        internal override void OnSelectionChange()
        {
            var node = Selection.activeObject as BehaviourTreeNode;
            if (Selection.activeObject != TreeAsset && (node == null || !TreeAsset.Nodes.Contains(node)))
                ReloadAssetFromSelection();
        }

        internal override void OnDraw()
        {
            if (_doRepair)
            {
                _doRepair = false;

                BehaviourTree.TryRepairAsset(AssetPath, TreeAsset);
                RecreateNodes();

                return;
            }
            
            if (TreeAsset != null)
            {
                Model model;
                model.TreeAsset = TreeAsset;
                model.AssetPath = AssetPath;

                View.DrawWorkspace(ref model);
            }
            else
            {
                View.DrawCreationButton();
            }
            
            OnConnectNodesQueue.Process();
        }

        internal void OnLostAsset()
        {
            TreeAsset.EditorPos = View.GetScrollPos();
        }

        internal void OnLoadAsset(BehaviourTree treeAsset)
        {
            if (TreeAsset)
                OnLostAsset();

            if (treeAsset == null)
            {
                TreeAsset = null;
                AssetPath = string.Empty;
            }
            else
            {
                TreeAsset = treeAsset;
                AssetPath = AssetDatabase.GetAssetPath(TreeAsset);
            }

            //View.RecreateParameterList();

            RecreateNodes();
        }

        internal void OnReloadAssetFromSelection()
        {
            ReloadAssetFromSelection();
        }

        internal void OnCreateNewAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Behaviour Tree", "New Behaviour Tree", "asset", string.Empty);
            var behaviourTree = ScriptableObject.CreateInstance<BehaviourTree>();

            try
            {
                AssetDatabase.CreateAsset(behaviourTree, AssetDatabase.GenerateUniqueAssetPath(path));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(behaviourTree);

                OnLoadAsset(behaviourTree);
            }
            catch (System.Exception)
            {
                // ignored
            }
        }

        private bool ShouldReloadFromSelection()
        {
            var filtered = Selection.GetFiltered(typeof(BehaviourTree), SelectionMode.Assets);
            if (filtered.Length == 1 && filtered[0] is BehaviourTree)
                return true;
            return Selection.activeObject != null && Selection.activeObject == TreeAsset;
        }

        private void ReloadAssetFromSelection()
        {
            if (!ShouldReloadFromSelection())
            {
                //CheckForRuntimeController();
                return;
            }

            BehaviourTree asset = null;

            var filtered = Selection.GetFiltered(typeof(BehaviourTree), SelectionMode.Assets);
            if (filtered.Length == 1)
            {
                asset = (BehaviourTree)filtered[0];
            }

            OnLoadAsset(asset);
        }

        private void AddToAsset(UnityEngine.Object asset)
        {
//            asset.hideFlags |= HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(asset, TreeAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private GraphNode AddNewNode(BehaviourTreeNode node)
        {
            AddToAsset(node);
            
            TreeAsset.Nodes.Add(node);

            return View.OnNodeAdded(TreeAsset, node);
        }

        private void RecreateNodes()
        {
            _duringRecreate = true;
            {
                OnConnectNodesQueue.Clear();
            
                Model model;
                model.TreeAsset = TreeAsset;
                model.AssetPath = AssetPath;
            
                View.RecreateNodes(ref model);
                View.Repaint();
            }
            _duringRecreate = false;
        }

        public void OnRightClick(Vector2 mousePosition)
        {
            ShowMainContextMenu((type) =>
            {
                var obj = ScriptableObject.CreateInstance(type) as BehaviourTreeNode;
                if (obj)
                {
                    obj.EditorPosition = mousePosition;
                    obj.name = obj.Name;

                    //Undo.IncrementCurrentGroup();
                    //
                    //int    index     = Undo.GetCurrentGroup();
                    string undoName = $"Created {obj.name} node";
                    
                    Undo.SetCurrentGroupName(undoName);
                    Undo.RecordObject(TreeAsset, undoName);
                    
                    AddNewNode(obj);

//                    if (index == Undo.GetCurrentGroup())
//                    {
//                        Undo.CollapseUndoOperations(index);
//                    }
                }
                else
                {
                    Debug.LogErrorFormat("Unable to create node '{0}'!", type.Name);
                }
            });
        }
 
        UnityEditor.GenericMenu.MenuFunction CreateNewNodeCallback(System.Action<Type> callback, Type type)
        {
            return () =>
            {
                callback(type);
            };
        }

        private void ShowMainContextMenu(System.Action<Type> callback)
        {
            GenericMenu menu = new GenericMenu();

            foreach (Type type in Reflection.GetSubclasses<BehaviourTreeNode>())
            {
                string title = string.Format("{0}/{1}", BehaviourTreeNode.GetNodeTypeName(type), type.Name);
                menu.AddItem(new GUIContent(title), false, CreateNewNodeCallback(callback, type));
            }

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Refresh"), false, RecreateNodes);

            menu.ShowAsContext();
        }

        public void OnEmptyDotClicked(BehaviourTreeEditorNode source, Vector2 position)
        {
            View.TryBeginConnection(source, position);
        }

        public void OnConnectNodes(BehaviourTreeNode parent, BehaviourTreeNode child)
        {
            if (_duringRecreate)
                return;

            var data = OnConnectNodesQueue.Post();
            data.Parent = parent;
            data.Child  = child;
        }

        public void ScheduleRepair()
        {
            _doRepair = true;
        }

        public void OnDropFailed(GraphNode node, Vector2 mousePos)
        {
            ShowMainContextMenu((type) =>
            {
                var obj = ScriptableObject.CreateInstance(type) as BehaviourTreeNode;
                if (obj)
                {
                    obj.EditorPosition = mousePos;
                    obj.name = obj.Name;

                    string undoName = $"Created {obj.name} node";
                    
                    Undo.SetCurrentGroupName(undoName);
                    Undo.RecordObject(TreeAsset, undoName);

                    GraphNode.MakeConnection(node, AddNewNode(obj));
                    
                    View.Repaint();
                }
                else
                {
                    Debug.LogErrorFormat("Unable to create node '{0}'!", type.Name);
                }
            });
        }
    }
}
