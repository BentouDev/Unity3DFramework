using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework.Editor;
using UnityEditor;
using UnityEditor.Graphs;
using UnityEngine;

namespace Framework.AI.Editor
{
    [InitializeOnLoad]
    public class BehaviourTreeEditorView : EditorView<BehaviourTreeEditorView, BehaviourTreeEditorPresenter>, IAssetEditor<BehaviourTree>
    {
        protected bool bShowParameters;
        private readonly GraphNodeEditor Nodes = new GraphNodeEditor();
        private Dictionary<BehaviourTreeNode, BehaviourTreeEditorNode> NodeLookup;

        #region Creation

        [MenuItem("Gameplay/Deprecated/Behaviour Tree Editor")]
        public static void MenuShowEditor()
        {
            FocusOrCreate();
        }

        protected BehaviourTreeEditorView()
        {
            Presenter = new BehaviourTreeEditorPresenter(this);
            Nodes.OnRightClick.Reassign(data =>
            {
                var node = Nodes.AllNodes.FirstOrDefault
                (
                    n => n.PhysicalRect.Contains(
                        (data.MousePos * Nodes.ZoomLevel) 
                )) as BehaviourTreeEditorNode;
                
                if (node != null)
                    Presenter.OnNodeRightClick(node, data.MousePos);
                else
                    Presenter.OnRightClick(data.MousePos);
                
                return true;
            });
            
            Nodes.OnConnection.AddPost(data =>
            {
                if (data.Target == null)
                {
                    Presenter.OnDropFailed(data.Source, data.MousePos);
                }
            });
            
            Nodes.OnDeleteNode.AddPost(data =>
            {
                Presenter.OnNodeDeleted(data.Node as BehaviourTreeEditorNode);
                Nodes.AllNodes.Remove(data.Node);
            });
        }

        void OnEnable()
        {
            name = " Behaviour   ";
            titleContent.image = SpaceEditorStyles.BehaviourTreeIcon;
            titleContent.text = name;

            Presenter.OnEnable();
        }
        
        internal GraphNode OnNodeAdded(BehaviourTree asset, BehaviourTreeNode node)
        {
            var newNode = new BehaviourTreeEditorNode(asset, node, Presenter);
            Nodes.AddNode(newNode);

            return newNode;
        }

        internal void RecreateNodes(ref BehaviourTreeEditorPresenter.Model model)
        {
            if (NodeLookup != null)
                NodeLookup.Clear();
            else
                NodeLookup = new Dictionary<BehaviourTreeNode, BehaviourTreeEditorNode>();
            
            Nodes.ClearNodes();
            Nodes.ScrollPos = model.TreeAsset.EditorPos;

            foreach (var node in model.TreeAsset.Nodes)
            {
                var editorNode = new BehaviourTreeEditorNode(model.TreeAsset, node, Presenter);
                NodeLookup[node] = editorNode;
                Nodes.AddNode(editorNode);
            }

            foreach (var node in Nodes.AllNodes.Select(n => n as BehaviourTreeEditorNode))
            {
                /*if (node != null && node.TreeNode.IsParentNode())
                {
                    var childNodes = node.TreeNode.AsParentNode().GetChildNodes();
                    if (childNodes != null)
                    {
                        foreach (var childNode in childNodes)
                        {
                            var foundNode = NodeLookup[childNode];
                            if (GraphNode.CanMakeConnection(node, foundNode))
                                GraphNode.MakeConnection(node, foundNode);
                        }
                    }
                }*/
                throw new NotImplementedException("TODO!");
            }
        }

        #endregion

        #region IAssetEditor

        public void OnLoadAsset(BehaviourTree asset)
        {
            Presenter.OnLoadAsset(asset);
        }

        public void ReloadAssetFromSelection()
        {
            Presenter.OnReloadAssetFromSelection();
        }

        #endregion

        #region GUI

        public Vector2 GetScrollPos()
        {
            return Nodes.ScrollPos;
        }

        internal void DrawWorkspace(ref BehaviourTreeEditorPresenter.Model model)
        {
            GUILayout.BeginVertical();
            {
                DrawToolbar(ref model);

                GUILayout.BeginHorizontal();
                {
                    DrawNodeGraph();

                    if (bShowParameters)
                    {
                        EditorGUILayout.BeginVertical(GUILayout.Width(300));
                        {
                            DrawParameters();
                        }
                        EditorGUILayout.EndVertical();
                    }
                }
                GUILayout.EndHorizontal();

                Nodes.HandleEvents(this);

                DrawFooter(ref model);
            }
            EditorGUILayout.EndVertical();

            if (Nodes.WantsRepaint)
                Repaint();
        }

        private void DrawNodeGraph()
        {
            GUILayout.BeginVertical(SpaceEditorStyles.GraphNodeEditorBackground);
            {
                // Reserve space for graph
                var targetRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));
                var adjustedRect = new Rect(0, 16 + 21, targetRect.width, position.height - 21 - 16);

                Nodes.Draw(this, adjustedRect);
            }
            GUILayout.EndVertical();
        }

        private void DrawToolbar(ref BehaviourTreeEditorPresenter.Model model)
        {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
            {
                GUILayout.Label(model.TreeAsset.name);

                /*if (ExecuteInRuntime())
                {
                    GUI.color = Color.yellow;

                    EditorGUILayout.Separator();

                    GUILayout.Label("PLAYING : " + RuntimeController.name);

                    GUI.color = Color.white;

                    if (GUILayout.Button("select", EditorStyles.toolbarButton))
                    {
                        Selection.activeGameObject = RuntimeController.gameObject;
                    }
                }*/

                if (Presenter.NeedsRepair(model.AssetPath, model.TreeAsset))
                {
                    using (new EditorAreaUtils.GUIColor(Color.red))
                    {
                        EditorGUILayout.Separator();
                        GUILayout.Label("Optimizeable");
                    }
                }

                GUILayout.FlexibleSpace();

                bShowParameters = GUILayout.Toggle(bShowParameters, "Parameters", EditorStyles.toolbarButton);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawParameters()
        {
            
        }

        private void DrawFooter(ref BehaviourTreeEditorPresenter.Model model)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true)); // GUI.skin.box, 
            {
                if (GUILayout.Button(model.AssetPath, EditorStyles.label))
                {
                    Selection.activeObject = model.TreeAsset;
                    EditorGUIUtility.PingObject(model.TreeAsset);
                }

                GUILayout.FlexibleSpace();
                GUILayout.Label($"<<{(Nodes.CurrentMouseMode != null ? Nodes.CurrentMouseMode.GetType().Name : "null")}>>");
                //GUILayout.Label($"{Nodes.ScrollPos} :: {Event.current.mousePosition} :: ");
                //GUILayout.Label($"{Nodes.ZoomLevel * 100:##.##}%");
                Nodes.ZoomLevel = GUILayout.HorizontalSlider(Nodes.ZoomLevel, 0.25f, 1, GUILayout.Width(64));
            }
            EditorGUILayout.EndHorizontal();
        }

        internal void DrawCreationButton()
        {
            GUI.Label(new Rect(EditorSize.x * 0.5f - 175, EditorSize.y * 0.5f - 15, 350, 30), "Select Behaviour Tree in project tab to edit, or create new ");
            if (GUI.Button(new Rect(EditorSize.x * 0.5f - 50, EditorSize.y * 0.5f + 15, 100, 20), "Create"))
            {
                Presenter.OnCreateNewAsset();
            }
        }

        #endregion

        public void TryBeginConnection(BehaviourTreeEditorNode source, Vector2 position)
        {
//            if (source.TreeNode.IsParentNode())
//                Nodes.StartConnection(source, position);
            throw new NotImplementedException("TODO!");
        }

        public void DeleteNode(BehaviourTreeEditorNode node)
        {
            Nodes.OnDeleteNode.Post().Node = node;
        }

        public void DisconnectNodes(BehaviourTreeEditorNode parent, BehaviourTreeEditorNode toRemove)
        {
//            parent.RemoveConnection(toRemove);
            Presenter.OnNodeDisconnected(parent, toRemove);
        }
    }
}
