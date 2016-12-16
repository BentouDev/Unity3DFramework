using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.EditorUtils;
using MyNamespace;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.AI
{
    public class BehaviourTreeEditor : EditorWindow
    {
        private static BehaviourTreeEditor instance;

        private Vector2 editorSize = Vector2.zero;
        private Vector2 paramScroll = Vector2.zero;

        private BehaviourTree TreeAsset;
        private string AssetPath;
        
        private readonly GraphNodeEditor Nodes = new GraphNodeEditor();
        
        private bool showParameters;

        private AIController RuntimeController;
        
        public static BehaviourTreeEditor GetInstance()
        {
            return instance;
        }
        
        public BehaviourTree GetCurrentAsset()
        {
            return TreeAsset;
        }

        [MenuItem("Gameplay/Behaviour Tree")]
        public static void ShowEditor()
        {
            instance = EditorWindow.GetWindow<BehaviourTreeEditor>();
        }

        public BehaviourTreeEditor()
        {
            Nodes.OnRightClick += CreateRootNode;
            Nodes.OnLeftClick += SelectTreeAsset;
            Nodes.OnDelete += HandleDelete;
            Nodes.OnConnectionEmptyDrop += CreateChildNode;
            BehaviourTreeGraphNode.OnNewChildNode += Nodes.StartConnection; // CreateChildNode;
        }
        
        private void HandleDelete()
        {
            var node = GraphNode.toDelete as BehaviourTreeGraphNode;
            if (node != null)
            {
                DeleteFromAsset(node.TreeNode);
            }
        }

        private void DestroyChildrenNodes(BehaviourTreeNode node)
        {
            if (!node.IsParentNode())
                return;

            var asParent = node.AsParentNode();
            var children = asParent.GetChildNodes();
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
                    asParent.RemoveChild(treeNode);
                    DestroyImmediate(treeNode, true);
                }
            }
        }
        
        private bool RemoveNodeAssetRecursive(BehaviourTreeNode node, BehaviourTreeNode toRemove)
        {
            bool removed = false;

            if (node.IsParentNode())
            {
                var asParent = node.AsParentNode();
                var childNodes = asParent.GetChildNodes();
                if (childNodes.Contains(toRemove))
                {
                    asParent.RemoveChild(toRemove);
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

        private void SelectTreeAsset(Vector2 mousePosition)
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

        private void CreateChildNode(GraphNode node, Vector2 position)
        {
            var behNode = node as BehaviourTreeGraphNode;
            if (!TreeAsset || behNode == null || !behNode.TreeNode.IsParentNode())
                return;

            ShowNodeCreationMenu((type) =>
            {
                var obj = ScriptableObject.CreateInstance(type) as BehaviourTreeNode;
                if (obj)
                {
                    obj.EditorPosition = position;

                    AddToAsset(obj);

                    behNode.TreeNode.AsParentNode().AddOrSetChild(obj);

                    RecreateNodes();
                }
                else
                {
                    Debug.LogErrorFormat("Unable to create type {0} as Behaviour Tree Node!", type.Name);
                }
            });
        }

        private void CreateRootNode(Vector2 position)
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
                        obj.EditorPosition = position;

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
                CreateNodesRecursive(new BehaviourTreeGraphNode(TreeAsset.RootNode));
            }
        }
        
        private void CreateNodesRecursive(BehaviourTreeGraphNode graphNode)
        {
            Nodes.AddNode(graphNode);

            if (graphNode.TreeNode.IsParentNode())
            {
                var childNodes = graphNode.TreeNode.AsParentNode().GetChildNodes();
                if (childNodes != null)
                {
                    foreach (BehaviourTreeNode childNode in childNodes)
                    {
                        var newNode = new BehaviourTreeGraphNode(childNode);
                        GraphNode.MakeConnection(graphNode, newNode);
                        CreateNodesRecursive(newNode);
                    }
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

        private static string GetTypePrefix(System.Type type)
        {
            if (type.IsSubclassOf(typeof(CompositeNode)))
                return "Composite";
            if (type.IsSubclassOf(typeof(DecoratorNode)))
                return "Decorator";
            if (type.IsSubclassOf(typeof(TaskNode)))
                return "Task";

            return "Unknown";
        }
        
        private void ShowNodeCreationMenu(System.Action<Type> callback)
        {
            GenericMenu menu = new GenericMenu();
            
            foreach (Type type in Reflection.GetSubclasses<BehaviourTreeNode>())
            {
                string title = string.Format("{0}/{1}", GetTypePrefix(type), type.Name);
                menu.AddItem(new GUIContent(title), false, CreateNewNodeCallback(callback, type));
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
        //    CheckForRuntimeController();
        }

        public bool ExecuteInRuntime()
        {
            return EditorApplication.isPlaying && HasRuntimeController();
        }

        public bool HasRuntimeController()
        {
            return RuntimeController != null;
        }

        private void CheckForRuntimeController()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isPaused)
                return;

            if (Selection.activeGameObject)
            {
                var controller = Selection.activeGameObject.GetComponent<AIController>();
                if (controller)
                {
                    RuntimeController = controller;
                }
            }
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

        void Update()
        {
            if (EditorApplication.isPlaying && !EditorApplication.isPaused)
            {
                CheckForRuntimeController();

                if (HasRuntimeController())
                {
                    Repaint();
                }
            }
        }

        void OnGUI()
        {
            if (instance == null)
                instance = this;

            editorSize = new Vector2(position.width, position.height);

            if (TreeAsset != null)
            {

                GUILayout.BeginVertical();
                {
                    DrawToolbar();

                    GUILayout.BeginHorizontal();
                    {
                        DrawNodeGraph();

                        if (showParameters)
                        {
                            EditorGUILayout.BeginVertical(GUILayout.Width(300));
                            {
                                DrawParameterList();
                            }
                            EditorGUILayout.EndVertical();
                        }
                    }
                    GUILayout.EndHorizontal();

                    DrawFooter();
                }
                EditorGUILayout.EndVertical();

                Nodes.HandleEvents();
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

        UnityEditor.GenericMenu.MenuFunction CreateNewParameterCallback(int index)
        {
            return () =>
            {
                EditorParameter.ParameterType type = (EditorParameter.ParameterType) Enum.GetValues(typeof(EditorParameter.ParameterType)).GetValue(index);
                AddNewParameter(type);
            };
        }

        private void AddNewParameter(EditorParameter.ParameterType type, UnityEngine.Object obj = null)
        {
            string paramName = string.Format("New {0}", type.ToString());

            int i = 0;
            while (TreeAsset.Parameters.Any(p => p.Name.Equals(paramName)))
            {
                paramName = string.Format("New {0} {1}", type.ToString(), i);
                i++;
            }

            TreeAsset.Parameters.Add (
                new EditorParameter(type)
                {
                    Name = paramName,
                    ObjectValue = obj,
                    Type = obj ? obj.GetType() : null
                }
            );
        }
        
        private void DrawParameterList()
        {
            paramScroll = GUILayout.BeginScrollView(paramScroll);

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                // GUILayout.TextField("Name", (GUIStyle)"ToolbarSeachTextFieldPopup");
                if (GUILayout.Button("+", EditorStyles.toolbarButton))
                {
                    GenericMenu menu = new GenericMenu();

                    var names = Enum.GetNames(typeof(EditorParameter.ParameterType));

                    for (int i = 0; i < names.Length; i++)
                    {
                        menu.AddItem(new GUIContent(names[i]), false, CreateNewParameterCallback(i));
                    }
                    
                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();

            foreach (EditorParameter parameter in TreeAsset.Parameters)
            {
                EditorParamDrawer.DrawParameter(parameter);
            }

            GUILayout.EndScrollView();
        }

        private void DrawToolbar()
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(TreeAsset.name);
                
                if (ExecuteInRuntime())
                {
                    GUI.color = Color.yellow;

                    GUILayout.Label("PLAYING : " + RuntimeController.name);

                    GUI.color = Color.white;

                    if (GUILayout.Button("select", EditorStyles.toolbarButton))
                    {
                        Selection.activeGameObject = RuntimeController.gameObject;
                    }
                }

                GUILayout.FlexibleSpace();

                showParameters = GUILayout.Toggle(showParameters, "Blackboard Parameters", EditorStyles.toolbarButton);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawNodeGraph()
        {
            GUILayout.BeginVertical(SpaceEditorStyles.GraphNodeEditorBackground);
            {
                var rect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                editorSize = Nodes.GetMaxCoordinates();

                Nodes.Draw(this, rect, new Rect(0, 0, editorSize.x, editorSize.y));
            }
            GUILayout.EndVertical();
        }

        private void DrawFooter()
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true)); // GUI.skin.box, 
            {
                GUILayout.Label(AssetPath);
                GUILayout.FlexibleSpace();
            }
            EditorGUILayout.EndHorizontal();
        }

        public bool CanEditNode(BehaviourTreeNode node)
        {
            return TreeAsset.Contains(node);
        }

        public bool IsCurrentNode(BehaviourTreeNode treeNode)
        {
            if (HasRuntimeController())
            {
                return RuntimeController.CurrentActingNode == treeNode;
            }

            return false;
        }
    }
}
