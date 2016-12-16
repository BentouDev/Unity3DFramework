using System;
using System.Collections;
using System.Collections.Generic;
using Framework.EditorUtils;
using UnityEditor;
using UnityEngine;

namespace Framework.AI
{
    public class BehaviourTreeEditor : EditorWindow
    {
        private static BehaviourTreeEditor instance;
        private Vector2 editorSize = Vector2.zero;
        private Vector2 errorScroll = Vector2.zero;

        private BehaviourTree TreeAsset;
        private string AssetPath;
        private UnityEditor.Editor AssetEditor;

        private BehaviourTreeGUINode RootGUINode;

        private readonly NodeGraph Nodes = new NodeGraph();

        private bool isLoaded;

        [MenuItem("Gameplay/Behaviour Tree")]
        public static void ShowEditor()
        {
            instance = EditorWindow.GetWindow<BehaviourTreeEditor>();
        }

        public BehaviourTreeEditor()
        {
            isLoaded = false;
            Nodes.OnRightClick += CreateRootNode;
            Nodes.OnLeftClick += SelectTreeAsset;
            Nodes.OnDelete += HandleDelete;
            BehaviourTreeGUINode.OnNewChildNode += CreateChildNode;
        }

        private void HandleDelete()
        {
            var node = BaseNode.toDelete as BehaviourTreeGUINode;
            if (node != null)
            {
                DeleteFromAsset(node.TreeNode);
            }
        }

        private void DestroyChildrenNodes(BehaviourTreeNode node)
        {
            var children = node.GetChildNodes();
            if (children != null && children.Count > 0)
            {
                List<BehaviourTreeNode> toRemove = new List<BehaviourTreeNode>();
                foreach (var child in children)
                {
                    DestroyChildrenNodes(child);
                    toRemove.Add(child);
                }

                foreach (BehaviourTreeNode treeNode in toRemove)
                {
                    node.RemoveChild(treeNode);
                    DestroyImmediate(treeNode, true);
                }
            }
        }
        
        private bool RemoveNodeAssetRecursive(BehaviourTreeNode node, BehaviourTreeNode toRemove)
        {
            bool removed = false;
            var childNodes = node.GetChildNodes();
            if (childNodes != null && childNodes.Count > 0)
            {
                if (childNodes.Contains(toRemove))
                {
                    node.RemoveChild(toRemove);
                    return true;
                }
                else
                {
                    foreach (BehaviourTreeNode childNode in childNodes)
                    {
                        if (RemoveNodeAssetRecursive(childNode, toRemove))
                        {
                            removed = true;
                            break;
                        }
                    }
                }
            }

            return removed;
        }

        private void DeleteFromAsset(UnityEngine.Object asset)
        {
            Undo.RecordObject(TreeAsset, "Deleted action");
            Undo.RecordObject(asset, "Deleted action");

            if (asset is BehaviourTreeNode)
            {
                BehaviourTreeNode node = (BehaviourTreeNode) asset;
                
                RemoveNodeAssetRecursive(TreeAsset.RootNode, node);
                DestroyChildrenNodes(node);

                if (node == TreeAsset.RootNode)
                    TreeAsset.RootNode = null;

                DestroyImmediate(node, true);
            }
            else
            {
                DestroyImmediate(asset, true);
            }

            EditorUtility.SetDirty(TreeAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            OnLoadAsset(TreeAsset);
        }

        private void SelectTreeAsset()
        {
            if (Selection.activeObject != TreeAsset)
            {
                Selection.activeObject = TreeAsset;
            }
        }

        private void AddToAsset(UnityEngine.Object asset)
        {
            AssetDatabase.AddObjectToAsset(asset, TreeAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private void CreateChildNode(BehaviourTreeNode node)
        {
            if (!TreeAsset)
                return;

            ShowNodeCreationMenu((type) =>
            {
                var obj = ScriptableObject.CreateInstance(type) as BehaviourTreeNode;
                if (obj)
                {
                    AddToAsset(obj);

                    node.AddOrSetChild(obj);

                    RecreateNodes();
                }
                else
                {
                    Debug.LogErrorFormat("Unable to create type {0} as Behaviour Tree Node!", type.Name);
                }
            });
        }

        private void CreateRootNode()
        {
            if (!TreeAsset)
                return;

            if (TreeAsset.RootNode == null)
            {
                ShowNodeCreationMenu((type) =>
                {
                    var obj = ScriptableObject.CreateInstance(type) as BehaviourTreeNode;
                    if (obj)
                    {
                        AddToAsset(obj);

                        TreeAsset.RootNode = obj;

                        RecreateNodes();
                    }
                    else
                    {
                        Debug.LogErrorFormat("Unable to set type {0} as Root Node!", type.Name);
                    }
                });
            }
        }

        private void RecreateNodes()
        {
            Nodes.ClearNodes();

            if (TreeAsset && TreeAsset.RootNode != null)
            {
                CreateNodesRecursive(new BehaviourTreeGUINode(TreeAsset.RootNode));
            }
        }
        
        private void CreateNodesRecursive(BehaviourTreeGUINode guiNode)
        {
            Nodes.AddNode(guiNode);

            var childNodes = guiNode.TreeNode.GetChildNodes();
            if (childNodes != null)
            {
                foreach (BehaviourTreeNode childNode in childNodes)
                {
                    var newNode = new BehaviourTreeGUINode(childNode);
                    BaseNode.MakeConnection(guiNode, newNode);
                    CreateNodesRecursive(newNode);
                }
            }
        }

        UnityEditor.GenericMenu.MenuFunction CreateNewNodeCallback(System.Action<Type> callback, Type type)
        {
            return () =>
            {
                callback(type);
            };
        }

        private void ShowNodeCreationMenu(System.Action<Type> callback)
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (Type type in Reflection.GetSubclasses<BehaviourTreeNode>())
            {
                menu.AddItem(new GUIContent(type.Name), false, CreateNewNodeCallback(callback, type));
            }

            menu.ShowAsContext();
        }

        public static void FocusOrCreate(BehaviourTree actionData)
        {
            if (instance)
            {
                instance.Focus();
            }
            else
            {
                ShowEditor();
            }

            if (actionData != null)
            {
                instance.OnLoadAsset(actionData);
            }
            else
            {
                instance.Reload();
            }
        }
        
        void OnEnable()
        {
            name = "Behaviour Tree";
            titleContent = new GUIContent(name);
            RecreateNodes();
        }

        void OnFocus()
        {
            Reload();
        }

        //void OnInspectorUpdate()
        //{
        //    OnLoadAsset(ActionAsset);
        //}

        void OnProjectChange()
        {
            Reload();
        }

        void OnSelectionChange()
        {
            Reload();
        }

        private void Reload()
        {
            if (!ShouldReload())
                return;

            AssetPath = string.Empty;
            TreeAsset = null;

            var filtered = Selection.GetFiltered(typeof(BehaviourTree), SelectionMode.Assets);
            if (filtered.Length == 1)
            {
                TreeAsset = (BehaviourTree) filtered[0];
                AssetPath = AssetDatabase.GetAssetPath(TreeAsset);
            }

            OnLoadAsset(TreeAsset);
        }

        private void OnLoadAsset(BehaviourTree asset)
        {
            if (asset == null)
            {
                AssetPath = string.Empty;
                // ActionAsset = null;
                AssetEditor = null;
            }
            else
            {
                // ActionAsset = asset;
                AssetEditor = Editor.CreateEditor(TreeAsset);

                // UpdateActionData(ActionAsset);
            }

            RecreateNodes();
            Repaint();
        }

        void CreateNewMoveset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Behaviour Tree", "New Behaviour Tree", "asset", string.Empty);
            var behaviourTree = CreateInstance<BehaviourTree>();

            AssetDatabase.CreateAsset(behaviourTree, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(behaviourTree);

            OnLoadAsset(behaviourTree);
        }

        bool ShouldReload()
        {
            var filtered = Selection.GetFiltered(typeof(BehaviourTree), SelectionMode.Assets);
            if (filtered.Length == 1 && filtered[0] is BehaviourTree)
                return true;
            return Selection.activeObject != null && Selection.activeObject == TreeAsset;
        }

        void OnGUI()
        {
            editorSize = new Vector2(position.width, position.height);

            if (TreeAsset != null)
            {

                GUILayout.BeginVertical();
                DrawToolbar();

                DrawContent();

                DrawFooter();
                EditorGUILayout.EndVertical();
            }
            else
            {
                GUI.Label(new Rect(editorSize.x * 0.5f - 175, editorSize.y * 0.5f - 15, 350, 30), "Select Behaviour Tree in project tab to edit, or create new ");
                if (GUI.Button(new Rect(editorSize.x * 0.5f - 50, editorSize.y * 0.5f + 15, 100, 20), "Create"))
                {
                    CreateNewMoveset();
                }
            }
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label(TreeAsset.name);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawContent()
        {
            var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
            editorSize = Nodes.GetMaxCoordinates();

            Nodes.Draw(this, rect, new Rect(0, 0, editorSize.x, editorSize.y));
            Nodes.HandleEvents();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(GUI.skin.box, GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(AssetPath);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
