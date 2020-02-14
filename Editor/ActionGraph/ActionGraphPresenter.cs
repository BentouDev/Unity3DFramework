using System;
using System.Linq;
using Framework.AI.Editor;
using Framework.Editor.Base;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;

namespace Framework.Editor
{
    public class ActionGraphPresenter : EditorPresenter, IReorderableNotify
    {
        private ActionGraphView View;
        private ActionGraph Asset;
        private string AssetPath;
        private bool _duringRecreate;

        private PropertyPath ParameterPath;
        
        public class ConnectEvent
        {
            public Connection Connection;
        }

        public readonly EventQueue<ConnectEvent> OnConnectNodesQueue = new EventQueue<ConnectEvent>();

        internal override void OnDestroy()
        {
            ActionGraph.TryRepairAsset(AssetPath, Asset);
        }

        public ActionGraphPresenter(ActionGraphView view)
        {
            View = view;

            OnConnectNodesQueue.Reassign((data =>
            {
                // if (!data.Connection.From.Owner.connectedTo.Any(c => c.Target.Equals(data.Connection.Target)))
                {
                    var asParent = data.Connection.From.Owner as ActionGraphEditorNode;
                    var asChild = data.Connection.Target.Owner as ActionGraphEditorNode;

                    if (asParent != null 
                    &&  asChild != null 
                    &&  asChild.ActionNode is ActionGraphNode asActionNode)
                    {
                        Undo.RecordObject(asParent.ActionNode,
                            $"Connect nodes {asActionNode.name} to {asParent.ActionNode.name}");
                        
                        GraphNode.FinishConnection(data.Connection);
                        return true;                        
                    }
                }

                return false;
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
            bool isFromAsset(ActionGraphNodeBase node)
            {
                return Asset.AnyEntryNode == node 
                    || Asset.Nodes.Contains(node)
                    || Asset.NamedEventEntries.Contains(node);
            }

            var asAct = Selection.activeObject as ActionGraphNodeBase;
            if (Selection.activeObject != Asset)
            {
                if (asAct == null || !isFromAsset(asAct))
                    ReloadAssetFromSelection();
            }
            else
            {
                View.OnSelectionChanged();
            }
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

            switch (node.ActionNode)
            {
                case ActionGraphNode asAction:
                {
                    foreach (ActionGraphNode graphNode in Asset.Nodes)
                    {
                        graphNode.Connections.Remove(asAction);
                    }

                    foreach (var entry in Asset.NamedEventEntries)
                    {
                        if (entry.Child == asAction)
                            entry.Child = null;
                    }

                    if (Asset.AnyEntryNode)
                    {
                        for (int i = 0; i < Asset.AnyEntryNode.Entries.Count; i++)
                        {
                            var entry = Asset.AnyEntryNode.Entries[i];
                            if (entry.Child == asAction)
                            {
                                entry.Child = null;
                                Asset.AnyEntryNode.Entries[i] = entry;
                            }
                        }
                    }

                    Asset.Nodes.Remove(asAction);
                    break;
                }
                case AnyEntry asAny:
                {
                    Asset.AnyEntryNode = null;
                    break;
                }
                case EventEntry asEvent:
                {
                    Asset.NamedEventEntries.Remove(asEvent);
                    break;
                }
            }
            
            /*foreach (ActionGraph.EntryPoint input in Asset.Inputs)
            {
                input.Nodes.Remove(node.ActionNode);
            }*/
        }

        internal void OnLostAsset()
        {
            Asset.EditorPos = View.GetScrollPos();
            Asset.Notifiers.Remove(this);
        }
        
        internal void OnLoadAsset(ActionGraph treeAsset)
        {
            if (Asset == treeAsset)
                return;

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

                Asset.Notifiers.Add(this);
            }
            
            PathBuilder<ActionGraph> builder = new PathBuilder<ActionGraph>();
            ParameterPath = builder
                .ByListOf<Framework.Parameter>(a => nameof(a.Parameters))
                .Path();

            RecreateNodes();
        }
        
        private GraphNode AddNewNode(ActionGraphNodeBase node)
        {
            AssetDatabaseUtils.AddToAsset(Asset, node);

            switch (node)
            {
                case ActionGraphNode asAction:
                {
                    Asset.Nodes.Add(asAction);
                    break;
                }
                case AnyEntry asAnyEntry:
                {
                    Asset.AnyEntryNode = asAnyEntry;
                    break;
                }
                case EventEntry asEntry:
                {
                    Asset.NamedEventEntries.Add(asEntry);
                    break;
                }
                default:
                {
                    Debug.LogError($"Unknown node {node}!");
                    break;
                }
            }

            return View.OnNodeAdded(Asset, node);
        }

        private void ShowNodeContextMenu(ActionGraphEditorNode node)
        {
            GenericMenu menu = new GenericMenu();

            // ToDo: disconnection per row
            // ToDo: special entries
            if (node.connectedTo.Any())
            {
                foreach (var info in node.connectedTo)
                {
                    menu.AddItem
                    (
                        new GUIContent($"Disconnect.../{info.Target.Owner.UniqueName}"), 
                        false, 
                        () => View.DisconnectNodes(info)
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
        
        private void ShowMainContextMenu(Vector2 mousePos, System.Action<Type> createByType)
        {
            GenericMenu menu = new GenericMenu();

            menu.AddItem(new GUIContent("New..."), false, () =>
            {
                ShowNodeCreationPopup(mousePos, createByType);
            });
            
            if (Asset.AnyEntryNode == null)
            {
                menu.AddItem(new GUIContent("Add 'Any' Entry"),false, 
                    () => { createByType(typeof(AnyEntry)); });
            }
            else
            {
                menu.AddDisabledItem(new GUIContent("Add 'Any' Entry"));
            }
            
            menu.AddItem(new GUIContent("Add Named Event Entry"), false, 
                () => { createByType(typeof(EventEntry)); });

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

            picker.Data = ReferenceTypePicker.BuildTypeList(string.Empty,
                type => type.IsSubclassOf(typeof(ActionGraphNode))).ToList();

            // picker.position = rect;
            picker.OnElementPicked = callback;
            // picker.CloseOnLostFocus = false;
            picker.ShowAsDropDown(rect, size);
        }

        internal override void OnDraw()
        {
            if (Asset)
            {
                try
                {
                    Asset.UpdateFromDataset();

                    View.OnDraw(Asset, AssetPath);
                    OnConnectNodesQueue.Process();

                    Asset.UploadToDataset();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
            else
            {
                View.DrawCreationButton();
            }
        }

        internal override void OnUpdate()
        {
            View.OnUpdate();
        }

        public void OnNodeRightClick(ActionGraphEditorNode node, Vector2 eventMousePos)
        {
            ShowNodeContextMenu(node);
        }

        public void OnRightClick(Vector2 mousePos)
        {
            ShowMainContextMenu(mousePos, (type) =>
            {
                var obj = ScriptableObject.CreateInstance(type) as ActionGraphNodeBase;
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

        public void OnDropFailed(Slot eventSource, Vector2 mousePos)
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

                    GraphNode.ConnectNodes(eventSource, AddNewNode(obj));

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

        /*public void OnInputDotClicked(Slot slot, Vector2 pos)
        {
            View.TryBeginConnection(slot, pos);
        }*/

        public void OnNodeConnectorClicked(Slot slot, Vector2 center)
        {
            View.TryBeginConnection(slot, center);
        }

        /*public void OnConnectInputNode(ActionGraphEditorNode node)
        {
            if (_duringRecreate)
                return;

            var data = OnConnectNodesQueue.Post();
            data.Parent = null;
            data.Child  = node.ActionNode;
            data.InputIndex = CurrentInputTarget;

            CurrentInputTarget = -1;
        }*/

        public void OnConnectChildNode(Connection connection)
        {
            if (_duringRecreate)
                return;

            var data = OnConnectNodesQueue.Post();
            data.Connection = connection;     
        }
        
        public void OnRemoveInputAtIndex(int index)
        {
            /*var input = Asset.Inputs[index];
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

            Asset.Inputs.RemoveAt(index);*/
        }

        public void OnReorderInputAtIndex(int oldIndex, int newIndex)
        {
            Undo.RecordObject(Asset, "Input reordered");

            //var oldInput = Asset.Inputs[oldIndex];
            
            //Asset.Inputs.RemoveAt(oldIndex);
            // Asset._inputNodes.RemoveAt(oldIndex);

            //Asset.Inputs.Insert(newIndex, oldInput);
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

        /*public void OnNodeDisconnected(ActionGraphEditorNode parent, ActionGraphEditorNode toRemove)
        {
            parent.ActionNode.Connections.Remove(toRemove.ActionNode);
        }*/

        public void OnInputDisconnected(ActionGraphEditorNode child)
        {
            /*Undo.RecordObject(Asset, "Disconnect from input");
            for (int i = 0; i < Asset.Inputs.Count; i++)
            {
                int index = Asset.Inputs[i].Nodes.IndexOf(child.ActionNode);
                if (index != -1)
                {
                    Asset.Inputs[i].Nodes.RemoveAt(index);
                }
            }*/
        }

        public void DisconnectNodes(ActionGraphNode parent, ActionGraphNode node)
        {
            View.DisconnectNodes(parent, node);
        }

        public void OnLeftClick(Vector2 mousePos)
        {
            Selection.activeObject = Asset;
        }

        public bool IsFromPath(PropertyPath path)
        {
            return ParameterPath.Equals(path);
        }

        public void OnReordered(PropertyPath path, int oldIndex, int newIndex)
        {
            EnsureSaved();
        }

        public void OnAdded(PropertyPath path)
        {
            EnsureSaved();
        }

        public void OnRemoved(PropertyPath path, int index)
        {
            EnsureSaved();
        }

        private void EnsureSaved()
        {
            EditorUtility.SetDirty(Asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}