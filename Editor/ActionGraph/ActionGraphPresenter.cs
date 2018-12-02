using System;
using System.Linq;
using Framework.AI.Editor;
using Framework.Editor.Base;
using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    public class ActionGraphPresenter : EditorPresenter
    {
        private ActionGraphView View;
        private ActionGraph Asset;
        private string AssetPath;
        private bool _duringRecreate;
        private int CurrentInputTarget;
        
        public class ConnectEvent
        {
            public ActionGraphNode Parent;
            public ActionGraphNode Child;
            public int InputIndex;
        }
        
        public readonly EventQueue<ConnectEvent> OnConnectNodesQueue = new EventQueue<ConnectEvent>();
        
        public ActionGraphPresenter(ActionGraphView view)
        {
            View = view;
            
            OnConnectNodesQueue.Reassign((data =>
            {
                if (!data.Child || (!data.Parent && data.InputIndex == -1))
                    return false;

                if (data.InputIndex == -1)
                {
                    if (!data.Parent.Connections.Contains(data.Child))
                    {
                        Undo.RecordObject(data.Parent, $"Connect nodes {data.Child.name} to {data.Parent.name}");
                        data.Parent.Connections.Add(data.Child);                        
                    }
                }
                else
                {
                    if (!Asset.Inputs[data.InputIndex].Nodes.Contains(data.Child))
                    {
                        Undo.RecordObject(Asset, $"Connect node {data.Child.name} to input {data.InputIndex}");
                        Asset.Inputs[data.InputIndex].Nodes.Add(data.Child);                        
                    }
                }

                return true;
            }));
        }
        
        private void RecreateNodes()
        {
            _duringRecreate = true;
            {
                // OnConnectNodesQueue.Clear();

                View.RecreateNodes(Asset);
                View.Repaint();
            }
            _duringRecreate = false;
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
            var asAct = Selection.activeObject as ActionGraphNode;
            if (Selection.activeObject != Asset && (asAct == null || !Asset.Nodes.Contains(asAct)))
                ReloadAssetFromSelection();
        }

        bool ShouldReloadFromSelection()
        {
            var filtered = Selection.GetFiltered(typeof(ActionGraph), SelectionMode.Assets);
            if (filtered.Length == 1 && filtered[0] is ActionGraph)
                return true;
            return Selection.activeObject != null && Selection.activeObject == Asset;
        }

        void ReloadAssetFromSelection()
        {
            if (!ShouldReloadFromSelection())
                return;
            
            ActionGraph asset = null;

            var filtered = Selection.GetFiltered(typeof(ActionGraph), SelectionMode.Assets);
            if (filtered.Length == 1)
            {
                asset = (ActionGraph)filtered[0];
            }

            OnLoadAsset(asset);
        }
        
        private void DeleteFromAsset(ActionGraphEditorNode node)
        {
            Undo.RecordObject(Asset, $"Removed {node.ActionNode.name}");
            
            foreach (ActionGraphNode graphNode in Asset.Nodes)
            {
                graphNode.Connections.Remove(node.ActionNode);
            }

            foreach (ActionGraph.EntryPoint input in Asset.Inputs)
            {
                input.Nodes.Remove(node.ActionNode);
            }

            Asset.Nodes.Remove(node.ActionNode);
        }

        private void AddToAsset(UnityEngine.Object asset)
        {
//            asset.hideFlags |= HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(asset, Asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        internal void OnLostAsset()
        {
            Asset.EditorPos = View.GetScrollPos();
        }
        
        internal void OnLoadAsset(ActionGraph treeAsset)
        {
            if (Asset)
                OnLostAsset();

            if (treeAsset == null)
            {
                Asset = null;
                AssetPath = string.Empty;
            }
            else
            {
                Asset = treeAsset;
                AssetPath = AssetDatabase.GetAssetPath(Asset);
            }

            //View.RecreateParameterList();

            RecreateNodes();
        }
        
        private GraphNode AddNewNode(ActionGraphNode node)
        {
            AddToAsset(node);
            
            Asset.Nodes.Add(node);

            return View.OnNodeAdded(Asset, node);
        }

        private void ShowNodeContextMenu(ActionGraphEditorNode node)
        {
            GenericMenu menu = new GenericMenu();

            if (node.connectedTo.Any())
            {
                foreach (var info in node.connectedTo)
                {
                    menu.AddItem
                    (
                        new GUIContent($"Disconnect.../{info.Node.UniqueName}"), 
                        false, 
                        () => View.DisconnectNodes(node, info.Node as ActionGraphEditorNode)
                    );
                }
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Disconnect..."));
            }
            
            menu.AddItem(new GUIContent("Delete"), false, () => View.DeleteNode(node));

            menu.ShowAsContext();
        }
        
        private void ShowMainContextMenu(Vector2 mousePos, System.Action<Type> callback)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("New..."), false, () =>
            {
                ShowNodeCreationPopup(mousePos, callback);
            });

            menu.AddSeparator(string.Empty);
            menu.AddItem(new GUIContent("Refresh"), false, RecreateNodes);

            menu.ShowAsContext();
        }
        
        class ActionNodePicker : PickerWindow<System.Type>
        { }

        private void ShowNodeCreationPopup(Vector2 mousePos, Action<Type> callback)
        {
            var picker = ScriptableObject.CreateInstance<ActionNodePicker>();
            var size = new Vector2(300, 400);
            var rect = new Rect(mousePos.x, mousePos.y, 100, 100);

            picker.Data = ReferenceTypePicker.BuildTypeList(string.Empty, type => type.IsSubclassOf(typeof(ActionGraphNode))).ToList();
            // picker.position = rect;
            picker.OnElementPicked = callback;
            // picker.CloseOnLostFocus = false;
            picker.ShowAsDropDown(rect, size);
        }

        internal override void OnDraw()
        {
            if (Asset)
            {
                View.OnDraw(Asset, AssetPath);
                OnConnectNodesQueue.Process();
            }
            else
            {
                View.DrawCreationButton();
            }
        }
        
        public void OnNodeRightClick(ActionGraphEditorNode node, Vector2 eventMousePos)
        {
            ShowNodeContextMenu(node);
        }

        public void OnRightClick(Vector2 mousePos)
        {
            ShowMainContextMenu(mousePos, (type) =>
            {
                var obj = ScriptableObject.CreateInstance(type) as ActionGraphNode;
                if (obj)
                {
                    obj.EditorPosition = View.GetScrollPos();
                    obj.Graph = Asset;
                    obj.name = $"New {type.Name}";

                    string undoName = $"Created {obj.name} node";
                    
                    Undo.SetCurrentGroupName(undoName);
                    Undo.RecordObject(Asset, undoName);

                    AddNewNode(obj);
                    
                    View.Repaint();
                }
                else
                {
                    Debug.LogErrorFormat("Unable to create node!");
                }
            });
        }

        public void OnDropFailed(GraphNode eventSource, Vector2 mousePos)
        {
            ShowNodeCreationPopup(mousePos, (type) =>
            {
                var obj = ScriptableObject.CreateInstance(type) as ActionGraphNode;
                if (obj)
                {
                    obj.EditorPosition = View.GetScrollPos();
                    obj.Graph = Asset;
                    obj.name = $"New {type.Name}";

                    string undoName = $"Created {obj.name} node";
                
                    Undo.SetCurrentGroupName(undoName);
                    Undo.RecordObject(Asset, undoName);

                    GraphNode.MakeConnection(eventSource, AddNewNode(obj));

                    View.Repaint();
                }
                else
                {
                    Debug.LogErrorFormat("Unable to create node!");
                }
            });
        }

        public void OnNodeDeleted(ActionGraphEditorNode editorNode)
        {
            DeleteFromAsset(editorNode);
        }

        public void OnInputDotClicked(GraphNode node, int index, Vector2 pos)
        {
            CurrentInputTarget = index;
            View.TryBeginConnection(node, pos);
        }

        public void OnNodeConnectorClicked(ActionGraphEditorNode node, Vector2 center)
        {
            View.TryBeginConnection(node, center);
        }

        public void OnConnectInputNode(ActionGraphEditorNode node)
        {
            if (_duringRecreate)
                return;

            var data = OnConnectNodesQueue.Post();
            data.Parent = null;
            data.Child  = node.ActionNode;
            data.InputIndex = CurrentInputTarget;

            CurrentInputTarget = -1;
        }

        public void OnConnectChildNode(ActionGraphEditorNode parent, ActionGraphEditorNode child)
        {
            if (_duringRecreate)
                return;

            var data = OnConnectNodesQueue.Post();
            data.Parent = parent.ActionNode;
            data.Child  = child.ActionNode;
            data.InputIndex = -1;            
        }
        
        public void OnRemoveInputAtIndex(int index)
        {
            var input = Asset.Inputs[index];
            if (input.Nodes.Any())
            {
                if (!EditorUtility.DisplayDialog("Warning!", 
                    "This will orphan existing input nodes. Are you sure?", 
                    "Ok",
                    "Cancel"))
                {
                    return;
                }
            }

            View.OnRemoveInput(input);

            Undo.RecordObject(Asset, "Input removed");

            Asset.Inputs.RemoveAt(index);
        }

        public void OnReorderInputAtIndex(int oldIndex, int newIndex)
        {
            Undo.RecordObject(Asset, "Input reordered");

            var oldInput = Asset.Inputs[oldIndex];
            
            Asset.Inputs.RemoveAt(oldIndex);
            // Asset._inputNodes.RemoveAt(oldIndex);

            Asset.Inputs.Insert(newIndex, oldInput);
            // Asset._inputNodes.Insert(newIndex, oldNode);
        }

        public void OnCreateNewAsset()
        {
            string path = EditorUtility.SaveFilePanelInProject("Create New Action Graph", "New Action Graph", "asset", string.Empty);
            var actionGraph = ScriptableObject.CreateInstance<ActionGraph>();

            try
            {
                AssetDatabase.CreateAsset(actionGraph, AssetDatabase.GenerateUniqueAssetPath(path));
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();

                EditorGUIUtility.PingObject(actionGraph);

                OnLoadAsset(actionGraph);
            }
            catch (System.Exception)
            {
                // ignored, thrown event when user closes the save window
            }
        }

        public void OnNodeDisconnected(ActionGraphEditorNode parent, ActionGraphEditorNode toRemove)
        {
            parent.ActionNode.Connections.Remove(toRemove.ActionNode);
        }

        public void OnInputDisconnected(ActionGraphEditorNode child)
        {
            Undo.RecordObject(Asset, "Disconnect from input");
            for (int i = 0; i < Asset.Inputs.Count; i++)
            {
                int index = Asset.Inputs[i].Nodes.IndexOf(child.ActionNode);
                if (index != -1)
                {
                    Asset.Inputs[i].Nodes.RemoveAt(index);
                }
            }
        }
    }
}