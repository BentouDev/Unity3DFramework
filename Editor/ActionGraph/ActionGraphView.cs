using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine.Assertions;

namespace Framework.Editor
{
    #region SYF
    public class BeginNode : GraphNode
    {
        private UnityEditorInternal.ReorderableList DrawerList;
        private ActionGraph Graph;
        private float PreferredWidth;
        private ActionGraphPresenter Presenter;
        private List<int> ProxyList;
        
        private List<float> _positions = new List<float>();
        private List<float> Positions
        {
            get
            {
                //_positions.Resize(Graph.Inputs.Count + 1);
                return _positions;
            }
        }

        private float FirstPos;

        void RecreateDrawer()
        {
            VariantUtils.SetDrawersForKnownTypes();

            /*ProxyList = new List<int>(Graph.Inputs.Count);

            DrawerList = new UnityEditorInternal.ReorderableList
            (
                ProxyList, typeof(ActionGraph.EntryPoint),
                true, false, true, true
            );

            DrawerList.drawHeaderCallback += rect =>
            {
                rect.x      += ParamListDrawer.ReorderableListOffset;
                rect.width  -= ParamListDrawer.ReorderableListOffset;

                var original = rect;

                rect.width *= 0.5f;
                GUI.Label(rect, "Name");

                
                rect.x = original.x + VariantUtils.LabelWidth * 1.35f;
                rect.width = VariantUtils.FieldWidth;
                GUI.Label(rect, "Input value");
            };

            DrawerList.onAddCallback += OnAddInput;

            DrawerList.drawElementCallback += OnDrawParameter;

            DrawerList.onCanAddCallback += list => Graph.InputType != null && Graph.InputType.Type != null;
            DrawerList.onRemoveCallback += list =>
            {
                Presenter.OnRemoveInputAtIndex(list.index);
            };

            DrawerList.onReorderCallbackWithDetails += (list, index, newIndex) =>
            {
                float pos = Positions[index];

                _positions.RemoveAt(index);
                _positions.Insert(newIndex, pos);

                Presenter.OnReorderInputAtIndex(index, newIndex);
            };*/
        }

        public BeginNode(ActionGraph graph, ActionGraphPresenter presenter)
        {
            Graph = graph;

            RecreateDrawer();
            
            Size = new Vector2(400,300);

            WindowTitle = GUIContent.none;
            WindowStyle = SpaceEditorStyles.GraphNodeBackground;

            PreferredWidth = drawRect.width;

            Presenter = presenter;

            Position = Graph.BeginEditorPos;
        }

        private void OnAddInput(UnityEditorInternal.ReorderableList list)
        {
            /*string typename = Variant.GetDisplayedName(Graph.InputType.Type);
            string paramName = StringUtils.MakeUnique($"New {typename}", Graph.Inputs.Select(p => p.Input.Name));

            ProxyList.Add(1);
            Graph.Inputs.Add
            (
                new ActionGraph.EntryPoint()
                {
                    Input = new Variant(Graph.InputType)
                    {
                        Name = paramName
                    } 
                }
            );*/
        }

        private void OnDrawParameter(Rect rect, int index, bool active, bool focused)
        {
            /*var parameter = Graph.Inputs[index];

            if (index == 0)
                FirstPos = rect.y;

            Positions[index] = rect.y - FirstPos;

            rect.height = VariantUtils.FieldHeight;
            rect.y     += 2;

            VariantUtils.DrawParameter(rect, parameter.Input);*/
        }

        protected override void OnSelected(bool value)
        {
            Selection.activeObject = Graph;
        }

        private void OnTypeChanged(SerializedType type)
        {
            // ToDo: Move to presenter
            /*if (Graph.InputType != null && Graph.InputType == type)
                return;

            if (Graph.Inputs != null && Graph.Inputs.Any() 
            && !EditorUtility.DisplayDialog("Warning!", 
                "This will erase existing input entries. Are you sure?", 
                "Ok",
                "Cancel"))
            {
                return;
            }

            Graph.Inputs?.Clear();
            Graph.InputType = type;*/
        }

        protected override void OnGUI()
        {
            drawRect.width = PreferredWidth;

            if (Selected)
                GUI.color = SpaceEditorStyles.ActiveColor;

            GUI.Box(drawRect, GUIContent.none, WindowStyle);
            DrawConnectDots(drawRect);

            GUI.color = Color.white;

            Graph.BeginEditorPos = Position;
        }

        Rect DrawNodeHeader()
        {
            drawRect.height = VariantUtils.FieldHeight * 2 + 6;
            GUI.color = Selected ? SpaceEditorStyles.ActiveColor : Color.white;
            GUI.Box(drawRect, GUIContent.none, Selected
                ? SpaceEditorStyles.GraphNodeBackgroundSelected
                : SpaceEditorStyles.GraphNodeBackground);

            GUI.color = Color.white;

            // Padding for content
            drawRect.y     += 4;
            drawRect.x     += 4;
            drawRect.width -= 8;

            var old_rect = drawRect;

            drawRect.x += 4;
            drawRect.height = VariantUtils.FieldHeight * 1.5f;
            GUI.Label(drawRect, "Inputs", EditorStyles.whiteLargeLabel);

            return old_rect;
        }

        public override void GetChildConnectPositions(GraphNode child, IList<Vector2> pos)
        {
            /*float yPos = drawRect.yMin + ConnectorSize.y + 74;

            var node = child as ActionGraphEditorNode;
            if (node != null)
            {
                for (int i = 0; i < Graph.Inputs.Count; i++)
                {
                    var input = Graph.Inputs[i];
                    if (input.Nodes.Contains(node.ActionNode))
                    {
                        pos.Add(new Vector2(drawRect.xMax, yPos + Positions[i]));
                    }
                }
            }*/
        }

        protected override bool CanConnectTo(Slot targetSlot, Slot childSlot)
        {
            return base.CanConnectTo(targetSlot, childSlot);
        }

        protected override void OnConnectToParent(Connection eventSource)
        {
            Assert.IsTrue(false, "Not supported!");
        }

        protected override void OnConnectToChild(Connection eventTarget)
        {
            /*if (eventTarget.From.Owner is ActionGraphEditorNode asGraph)
            {
                Presenter.OnConnectInputNode(asGraph);
            }*/
        }

        protected override void OnParentDisconnected(Connection node)
        {

        }

        void DrawConnectDots(Rect dotRect)
        {
            /*var inputsCount = Graph.Inputs.Count;
            var yPos = FirstPos;
            var xPos = drawRect.xMax - ConnectorSize.x * 0.5f;

            GUI.color = Color.white;

            if (inputsCount > 0)
            {
                for (int i = 0; i < inputsCount; i++)
                {
                    var input = Graph.Inputs[i];
                    var rect = new Rect(xPos, yPos + Positions[i], ConnectorSize.x, ConnectorSize.y);
                    
                    GUI.Box
                    (
                        rect, GUIContent.none, SpaceEditorStyles.DotFlowTarget
                    );
                    
                    if (input.Nodes.Any())
                    {
                        GUI.color = Selected ? SpaceEditorStyles.ActiveColor : Color.white;
                        GUI.Box
                        (
                            rect, GUIContent.none, SpaceEditorStyles.DotFlowTargetFill
                        );
                        GUI.color = Color.white;
                    }

                    if (rect.Contains(Event.current.mousePosition) 
                        && Event.current.type == EventType.MouseDown 
                        && Event.current.button == 0)
                    {
                        // TODO pass actual slot
                        Presenter.OnInputDotClicked(null, i, rect.center);
                    }
                }
            }*/
        }
        
        protected override void OnDrawContent()
        {
            /*ProxyList.Resize(Graph.Inputs.Count);

            EditorGUI.BeginChangeCheck();
            {
                var old_rect = DrawNodeHeader();

                drawRect.y += VariantUtils.FieldHeight + 2;
                drawRect.width = drawRect.width - 20;

                drawRect.y += 20;
                drawRect.height = VariantUtils.FieldHeight;
                string content = Graph.InputType.ToString();
                if (GUI.Button(drawRect, content, EditorStyles.objectField))
                {
                    KnownTypeUtils.ShowAddParameterMenu(OnTypeChanged);
                }

                drawRect.x = old_rect.x;
                drawRect.width = old_rect.width;

                drawRect.y += VariantUtils.FieldHeight + 4;
                drawRect.height = old_rect.height - drawRect.y;

                var base_y = drawRect.y - old_rect.y;
                if (DrawerList != null)
                {
                    DrawerList.DoList(drawRect);
                    Size = new Vector2(Size.x, base_y + DrawerList.GetHeight() + 8);
                }    
            }
            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }*/
        }
    }
    #endregion SYF
    
    public class ActionGraphView : EditorView<ActionGraphView, ActionGraphPresenter>
    {
        private readonly GraphNodeEditor Nodes = new GraphNodeEditor();

        private Dictionary<ActionGraphNodeBase, ActionGraphEditorNode> NodeLookup;

        private List<System.Type> NodeEditors = new List<System.Type>();

        [MenuItem("Framework/Action Graph Editor", false, 1)]
        public static void MenuShowEditor()
        {
            FocusOrCreate();
        }

        public ActionGraphView()
        {
            Presenter = new ActionGraphPresenter(this);
            Nodes.OnRightClick.Reassign(data =>
            {
                var node = Nodes.AllNodes.FirstOrDefault
                (
                    n => n.PhysicalRect.Contains(
                        (data.MousePos * Nodes.ZoomLevel) 
                    )) as ActionGraphEditorNode;

                if (node != null)
                    Presenter.OnNodeRightClick(node, data.MousePos);
                else
                    Presenter.OnRightClick(data.MousePos);
                
                return true;
            });
            
            Nodes.OnLeftClick.Reassign(data =>
            {
                Presenter.OnLeftClick(data.MousePos);
                return true;
            });

            Nodes.OnConnection.Reassign((data =>
            {
                if (data.Target != null)
                {
                    if (GraphNode.CanMakeConnection(data.Source, data.Target))
                    {
                        Presenter.OnConnectChildNode(GraphNode.MakeConnection(data.Source, data.Target));
                        return true;
                    }
                }

                return false;
            }));

            Nodes.OnConnection.AddPost(data =>
            {
                if (data.Target == null)
                {
                    Presenter.OnDropFailed(data.Source, data.MousePos);
                }
            });
            
            Nodes.OnDeleteNode.AddPost(data =>
            {
                Presenter.OnNodeDeleted(data.Node as ActionGraphEditorNode);
                Nodes.AllNodes.Remove(data.Node);
            });
            
            Nodes.OnSlotClicked.Reassign(data =>
            {
                Presenter.OnNodeConnectorClicked(data.Source, data.Source.Owner.GetSlotPosition(data.Source));
                return true;
            });
        }

        protected override void OnCreated()
        {
            
        }

        void OnEnable()
        {
            name = " Action Graph ";
            titleContent.image = SpaceEditorStyles.DotFlowTarget.normal.background;
            titleContent.text = name;

            Presenter.OnEnable();
        }

        private System.Type FindNodeEditor(ActionGraphNodeBase node)
        {
            return NodeEditors.FirstOrDefault(t => t.CustomAttributes.Any(
                a =>
                {
                    if (a.AttributeType == typeof(CustomActionEditor)
                        && a.ConstructorArguments.Count >= 1)
                    {
                        return a.ConstructorArguments[0].Value == node.GetType();
                    }

                    return false;
                }));
        }

        private ActionGraphEditorNode CreateNodeEditor(ActionGraphNodeBase node, ActionGraph graph)
        {
            var editorNodeType = FindNodeEditor(node);

            ActionGraphEditorNode editorNode = null;
            if (editorNodeType != null)
            {
                editorNode = Activator.CreateInstance(editorNodeType, graph, node, Presenter) 
                    as ActionGraphEditorNode;
            }
            else
            {
                editorNode = new ActionGraphEditorNode(graph, node, Presenter);                        
            }

            NodeLookup[node] = editorNode;
            Nodes.AddNode(editorNode);

            return editorNode;
        }

        public void RecreateNodes(ActionGraph asset)
        {
            using (new GraphNode.NodeRecreationContext())
            {
                if (NodeLookup != null)
                    NodeLookup.Clear();
                else
                    NodeLookup = new Dictionary<ActionGraphNodeBase, ActionGraphEditorNode>();

                ActionGraphEditorNode.WindowStyle = SpaceEditorStyles.GraphNodeBackground;

                Nodes.ClearNodes();

                if (asset)
                {
                    Nodes.ScrollPos = asset.EditorPos;

                    NodeEditors = ReferenceTypePicker.BuildTypeList(string.Empty, 
                        t => t.IsSubclassOf(typeof(ActionGraphEditorNode))
                    ).ToList();

                    if (asset.AnyEntryNode)
                    {
                        CreateNodeEditor(asset.AnyEntryNode, asset);
                    }

                    foreach (var entry in asset.NamedEventEntries)
                    {
                        CreateNodeEditor(entry, asset);
                    }

                    foreach (ActionGraphNode node in asset.Nodes)
                    {
                        CreateNodeEditor(node, asset);
                    }

                    foreach (GraphNode graphNode in Nodes.AllNodes.Where(t => t is ActionGraphEditorNode))
                    {
                        var asGraph = (ActionGraphEditorNode) graphNode;

                        asGraph.RebuildConnections(LookupEditorNode);
                    }
                }   
            }
        }

        private bool showParams;
        private float ParamsWidth = 300;
        private Splitter Split = new Splitter();
        
        public void OnDraw(ActionGraph graph, string assetPath)
        {
            GUILayout.BeginVertical();
            {
                GUILayout.BeginHorizontal();
                {
                    DrawNodeGraph();

                    if (showParams)
                    {
                        Split.Layout();
                        DrawParameters(graph);
                    }
                }
                GUILayout.EndHorizontal();
                
                DrawFooter(graph, assetPath);
                
                HandleEvents();
            }
            GUILayout.EndVertical();
        }

        private void HandleEvents()
        {
            var (wantsRepaint, paramWidthDelta) = Split.HandleWidth();

            ParamsWidth -= paramWidthDelta;
            ParamsWidth = Mathf.Clamp(ParamsWidth, 100, EditorSize.x * 0.9f);
            
            Nodes.WantsRepaint = wantsRepaint;
            
            Nodes.HandleEvents(this);
        }

        public void OnUpdate()
        {
            if (Nodes.WantsRepaint)
                Repaint();
        }
        
        private Rect NodeGraphRect = new Rect();

        private void DrawNodeGraph()
        {
            GUILayout.BeginVertical(SpaceEditorStyles.GraphNodeEditorBackground);
            {
                // Reserve space for graph
                var targetRect = GUILayoutUtility.GetRect(GUIContent.none, GUIStyle.none, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

                NodeGraphRect.Set(0, 16 + 21, targetRect.width, position.height - 21 - 16);

                Nodes.Draw(this, NodeGraphRect);
            }
            GUILayout.EndVertical();
        }
        
        private ParamListDrawer _drawer;
        private ParamListDrawer GetDrawer(ActionGraph graph)
        {
            if (_drawer == null)
            {
                _drawer = new ParamListDrawer();
                _drawer.Init(graph.Parameters);
                _drawer.Recreate();
            }

            return _drawer;
        }
        
        private void DrawParameters(ActionGraph graph)
        {
            GUILayout.BeginVertical(GUILayout.Width(ParamsWidth));
            {
                var drawer = GetDrawer(graph);
                EditorGUI.BeginChangeCheck();
                {
                    drawer.DrawerList.DoLayoutList();
                }
                if (EditorGUI.EndChangeCheck())
                {
                    graph.Parameters = drawer.GetParameters();
                    AssetDatabase.SaveAssets();
                }
            }
            GUILayout.EndVertical();
        }

        private void DrawFooter(ActionGraph graph, string assetPath)
        {
            EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(true)); // GUI.skin.box, 
            {
                if (GUILayout.Button(assetPath, EditorStyles.label))
                {
                    Selection.activeObject = graph;
                    EditorGUIUtility.PingObject(graph);
                }

                GUILayout.FlexibleSpace();
                //GUILayout.Label($"<<{(Nodes.CurrentMouseMode != null ? Nodes.CurrentMouseMode.GetType().Name : "null")}>>");
                
                //GUILayout.Label($"{Nodes.ScrollPos} :: {Event.current.mousePosition} :: ");
                GUILayout.Label($"{Nodes.ZoomLevel * 100:##.##}%");
                
                Nodes.ZoomLevel = GUILayout.HorizontalSlider(Nodes.ZoomLevel, Nodes.MinZoom, Nodes.MaxZoom, GUILayout.Width(64));

                showParams = GUILayout.Toggle(showParams, "Params", EditorStyles.toolbarButton);
            }
            EditorGUILayout.EndHorizontal();
        }
        
        internal void DrawCreationButton()
        {
            GUI.Label(new Rect(EditorSize.x * 0.5f - 175, EditorSize.y * 0.5f - 15, 350, 30), "Select Action Graph in project tab to edit, or create new ");
            if (GUI.Button(new Rect(EditorSize.x * 0.5f - 50, EditorSize.y * 0.5f + 15, 100, 20), "Create"))
            {
                Presenter.OnCreateNewAsset();
            }
        }

        public Vector2 GetScrollPos()
        {
            return Nodes.ScrollPos;
        }

        ActionGraphEditorNode LookupEditorNode(object obj)
        {
            return NodeLookup[obj as ActionGraphNodeBase 
                ?? throw new Exception($"Cannot cast {obj} to ActionGraphNode!")];
        }

        public GraphNode OnNodeAdded(ActionGraph asset, ActionGraphNodeBase node)
        {
            var editorNode = CreateNodeEditor(node, asset);
            editorNode.RebuildConnections(LookupEditorNode);
            return editorNode;
        }

        public void TryBeginConnection(Slot slot, Vector2 pos)
        {
            Nodes.StartConnection(slot, pos);
        }

//        public void OnRemoveInput(ActionGraph.EntryPoint input)
//        {
//            foreach (var node in input.Nodes)
//            {
//                Begin.RemoveConnection(NodeLookup[node]);   
//            }
//        }

        public void DisconnectNodes(Connection connection)
        {
            connection.From.Owner.RemoveConnection(connection);
        }

        [Obsolete("Do not use this! Remove nodes by connection!")]
        public void DisconnectNodes(ActionGraphNode parent, ActionGraphNode node)
        {
            throw new Exception("Removing connection by node itself is no longer supported!");

            var editorParent = NodeLookup[parent];
            var editorNode = NodeLookup[node];
            
            //editorParent.RemoveConnection(editorNode);
            // Presenter.OnNodeDisconnected(editorParent, editorNode);
        }

        [Obsolete("Do not use this! Remove nodes by connection!")]
        public void DisconnectNodes(GraphNode node, ActionGraphEditorNode child)
        {
            throw new Exception("Removing connection by node itself is no longer supported!");
            
            /*node.RemoveConnection(child);
            if (node == Begin)
            {
                Presenter.OnInputDisconnected(child);
            }
            else
            {
                Presenter.OnNodeDisconnected(node as ActionGraphEditorNode, child);
            }*/
        }

        public void DeleteNode(ActionGraphEditorNode node)
        {
            Nodes.OnDeleteNode.Post().Node = node;
        }

        public void OnSelectionChanged()
        {
            Nodes.DeselectNodes();
        }
    }
}