using System;
using System.Collections;
using System.Collections.Generic;
using Framework.Editor;
using UnityEditor;
using UnityEngine;

namespace Framework.AI.Editor
{
    public class BehaviourTreeEditorPresenter : EditorPresenter
    {
        protected BehaviourTreeEditorView View;
        private BehaviourTree TreeAsset;
        private string AssetPath;

        internal struct Model
        {
            public BehaviourTree TreeAsset;
            public string AssetPath;
        }

        public BehaviourTreeEditorPresenter(BehaviourTreeEditorView view)
        {
            View = view;
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

            Model model;
            model.TreeAsset = TreeAsset;
            model.AssetPath = AssetPath;

            View.RecreateNodes(ref model);
            View.Repaint();
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
            AssetDatabase.AddObjectToAsset(asset, TreeAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void AddNewNode(BehaviourTreeNode node)
        {
            AddToAsset(node);
            
            TreeAsset.Nodes.Add(node);

            View.OnNodeAdded(TreeAsset, node);
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
                    
                    AddNewNode(obj);
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
            menu.AddItem(new GUIContent("Refresh"), false, () =>
            {
                Model model;
                model.TreeAsset = TreeAsset;
                model.AssetPath = AssetPath;
                View.RecreateNodes(ref model);
            });

            menu.ShowAsContext();
        }

        public void OnEmptyDotClicked(BehaviourTreeEditorNode source, Vector2 position)
        {
            View.TryBeginConnection(source, position);
        }
    }
}
