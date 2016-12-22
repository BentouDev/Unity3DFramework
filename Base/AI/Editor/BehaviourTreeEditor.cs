using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.EditorUtils;
using Framework;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Framework.AI
{
    public class BehaviourTreeEditor : EditorWindow
    {
        private static readonly int ReorderableListOffset = 14;

        private static BehaviourTreeEditor instance;

        private Vector2 editorSize = Vector2.zero;
        private Vector2 paramScroll = Vector2.zero;

        private readonly GraphNodeEditor Nodes = new GraphNodeEditor();
        private ReorderableList ParameterList;

        private BehaviourTree TreeAsset;
        private string AssetPath;

        private AIController RuntimeController;

        private bool showParameters;
        
        #region Window

        [MenuItem("Gameplay/Behaviour Tree")]
        public static void ShowEditor()
        {
            instance = EditorWindow.GetWindow<BehaviourTreeEditor>();
        }

        public BehaviourTreeEditor()
        {
            SetupEvents();
        }

        private void SetupEvents()
        {
            Nodes.OnLeftClick                     += OnEmptyGraphLeftClick;
            Nodes.OnRightClick                    += OnEmptyGraphRightClick;
            Nodes.OnDelete                        += OnGraphNodeDeleted;
            Nodes.OnConnectionEmptyDrop           += OnGraphNewConnection;
            BehaviourTreeGraphNode.OnNewChildNode += OnGraphNodeEmptyTarget; 
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
                instance.ReloadFromSelection();
            }
        }

        void OnEnable()
        {
            name = " Behaviour   ";
            titleContent.image = SpaceEditorStyles.BehaviourTreeIcon;
            titleContent.text = name;

            RecreateParameterList();
            RecreateNodes();
        }

        void OnFocus()
        {
            ReloadFromSelection();
        }

        //void OnInspectorUpdate()
        //{
        //    OnLoadAsset(ActionAsset);
        //}

        void OnProjectChange()
        {
            ReloadFromSelection();
        }

        void OnSelectionChange()
        {
            ReloadFromSelection();
            //    CheckForRuntimeController();
        }

        #endregion

        #region Events

        void OnEmptyGraphLeftClick(Vector2 mousePosition)
        {
            SelectTreeAsset();
        }

        void OnEmptyGraphRightClick(Vector2 mousePosition)
        {
            CreateRootNode(mousePosition);
        }

        void OnGraphNodeDeleted()
        {
            HandleDelete();
        }

        void OnGraphNewConnection(GraphNode node, Vector2 mousePosition)
        {
            CreateChildNode(node, mousePosition);
        }

        void OnGraphNodeEmptyTarget(BehaviourTreeGraphNode node, Vector2 offset)
        {
            Nodes.StartConnection(node, offset);
        }

        #endregion

        #region Runtime

        public bool ExecuteInRuntime()
        {
            return EditorApplication.isPlaying && HasRuntimeController();
        }

        public bool HasRuntimeController()
        {
            return RuntimeController != null;
        }

        public NodeResult CheckNodeStatus(BehaviourTreeNode node)
        {
            return RuntimeController.CheckNodeStatus(node);
        }

        private void CheckForRuntimeController()
        {
            if (!EditorApplication.isPlaying || EditorApplication.isPaused)
                return;

            if (Selection.activeGameObject)
            {
                var controller = Selection.activeGameObject.GetComponent<AIController>();
                if (controller && controller != RuntimeController)
                {
                    RuntimeController = controller;
                    OnLoadAsset(RuntimeController.BehaviourTree);
                }
            }
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

        #endregion

        #region Editor

        private void SelectTreeAsset()
        {
            if (Selection.activeObject != TreeAsset)
            {
                Selection.activeObject = TreeAsset;
            }
        }

        public static BehaviourTreeEditor GetInstance()
        {
            return instance;
        }

        public BehaviourTree GetCurrentAsset()
        {
            return TreeAsset;
        }

        public bool CanEditNode(BehaviourTreeNode node)
        {
            return TreeAsset.Contains(node);
        }

        public bool IsNodeCurrent(BehaviourTreeNode treeNode)
        {
            if (HasRuntimeController())
            {
                return RuntimeController.CurrentActingNode == treeNode;
            }

            return false;
        }

        #endregion

        #region NodeDeletion

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

        #endregion

        #region NodeCreation

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
                string title = string.Format("{0}/{1}", BehaviourTreeNode.GetNodeTypeName(type), type.Name);
                menu.AddItem(new GUIContent(title), false, CreateNewNodeCallback(callback, type));
            }

            menu.ShowAsContext();
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
                    obj.name = obj.Name;

                    behNode.TreeNode.AsParentNode().AddOrSetChild(obj);

                    AddToAsset(obj);

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
                        obj.name = obj.Name + " (Root)";

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

        #endregion

        #region Asset

        bool ShouldReloadFromSelection()
        {
            var filtered = Selection.GetFiltered(typeof(BehaviourTree), SelectionMode.Assets);
            if (filtered.Length == 1 && filtered[0] is BehaviourTree)
                return true;
            return Selection.activeObject != null && Selection.activeObject == TreeAsset;
        }

        private void ReloadFromSelection()
        {
            if (!ShouldReloadFromSelection())
                return;

            AssetPath = string.Empty;
            TreeAsset = null;

            var filtered = Selection.GetFiltered(typeof(BehaviourTree), SelectionMode.Assets);
            if (filtered.Length == 1)
            {
                TreeAsset = (BehaviourTree) filtered[0];
            }

            OnLoadAsset(TreeAsset);
        }

        private void OnLoadAsset(BehaviourTree asset)
        {
            if (asset == null)
            {
                AssetPath = string.Empty;
            }
            else
            {
                TreeAsset = asset;
                AssetPath = AssetDatabase.GetAssetPath(TreeAsset);
            }

            RecreateParameterList();
            RecreateNodes();
            Repaint();
        }

        void CreateNewBehaviourTree()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Behaviour Tree", "New Behaviour Tree", "asset", string.Empty);
            var behaviourTree = CreateInstance<BehaviourTree>();

            AssetDatabase.CreateAsset(behaviourTree, AssetDatabase.GenerateUniqueAssetPath(path));
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            EditorGUIUtility.PingObject(behaviourTree);

            OnLoadAsset(behaviourTree);
        }

        #endregion

        #region Parameters

        private void RecreateParameterList()
        {
            if (!TreeAsset)
                return;

            GenericParamUtils.SetDrawersForKnownTypes();

            ParameterList = new ReorderableList
            (
                TreeAsset.Parameters, typeof(GenericParameter),
                true, false, true, true
            );

            ParameterList.drawHeaderCallback += rect =>
            {
                rect.x      += ReorderableListOffset;
                rect.width  -= ReorderableListOffset;

                var original = rect;

                rect.width *= 0.5f;
                GUI.Label(rect, "Name");

                
                rect.x = original.x + GenericParamUtils.LabelWidth * 1.35f;
                rect.width = GenericParamUtils.FieldWidth;
                GUI.Label(rect, "Default value");
            };

            ParameterList.onAddDropdownCallback += OnAddParameter;
            ParameterList.drawElementCallback += OnDrawParameter;
            ParameterList.onChangedCallback   += list =>
            {
                Undo.RecordObject(TreeAsset, "Modified Parameters");
                EditorUtility.SetDirty(TreeAsset);
            };
        }

        private void OnAddParameter(Rect buttonrect, ReorderableList list)
        {
            ShowAddParameterMenu();
        }

        private void ShowAddParameterMenu()
        {
            GenericMenu menu = new GenericMenu();

            var types = KnownType.GetKnownTypes();
            for (int i = 0; i < types.Count; i++)
            {
                var type = types[i];
                menu.AddItem(new GUIContent(type.GenericName), false, CreateNewParameterCallback(type.HoldType));
            }

            menu.ShowAsContext();
        }

        private void OnDrawParameter(Rect rect, int index, bool active, bool focused)
        {
            var parameter = TreeAsset.Parameters[index];

            rect.height = GenericParamUtils.FieldHeight;
            rect.y     += 2;

            GenericParamUtils.DrawParameter(rect, parameter);
        }

        UnityEditor.GenericMenu.MenuFunction CreateNewParameterCallback(Type type)
        {
            return () =>
            {
                OnAddNewParameter(type);
            };
        }

        private void AddNewParam(Type type)
        {
            string paramName = StringUtils.MakeUnique(string.Format("New {0}", type.Name), TreeAsset.Parameters.Select(p => p.Name));

            TreeAsset.Parameters.Add(
               new GenericParameter(type)
               {
                   Name = paramName
               }
           );
        }

        private void OnObjectReferenceTypeSelected(System.Type type)
        {
            AddNewParam(type);
        }

        private void OnAddNewParameter(Type type)
        {
            if (type.IsSubclassOf(typeof(UnityEngine.Object)) && type != typeof(GameObject))
            {
                ReferenceTypePicker.ShowWindow(type, OnObjectReferenceTypeSelected, t => t.IsSubclassOf(type));
            }
            else
            {
                AddNewParam(type);
            }
        }

        #endregion

        #region GUI

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
                    CreateNewBehaviourTree();
                }
            }
        }

        private void DrawParameterList()
        {
            paramScroll = GUILayout.BeginScrollView(paramScroll);

            /*EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                // GUILayout.TextField("Name", (GUIStyle)"ToolbarSeachTextFieldPopup");
                if (GUILayout.Button("+", EditorStyles.toolbarButton))
                {
                    GenericMenu menu = new GenericMenu();

                    var names = Enum.GetNames(typeof(GenericParameter.ParameterType));

                    for (int i = 0; i < names.Length; i++)
                    {
                        menu.AddItem(new GUIContent(names[i]), false, CreateNewParameterCallback(i));
                    }

                    menu.ShowAsContext();
                }
            }
            EditorGUILayout.EndHorizontal();*/

            if (ParameterList != null && TreeAsset != null && TreeAsset.Parameters != null)
            {
                ParameterList.DoLayoutList();
            }

            /*foreach (GenericParameter parameter in TreeAsset.Parameters)
            {
                GenericParamUtils.LayoutParameter(parameter);
            }*/

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

                    EditorGUILayout.Separator();

                    GUILayout.Label("PLAYING : " + RuntimeController.name);

                    GUI.color = Color.white;

                    if (GUILayout.Button("select", EditorStyles.toolbarButton))
                    {
                        Selection.activeGameObject = RuntimeController.gameObject;
                    }
                }

                GUILayout.FlexibleSpace();

                showParameters = GUILayout.Toggle(showParameters, "Parameters", EditorStyles.toolbarButton);
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

        #endregion
    }
}
