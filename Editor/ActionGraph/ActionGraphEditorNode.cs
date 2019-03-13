using System;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    public class ActionGraphEditorNode : GraphNode
    {
        protected static readonly float BASIC_HEIGHT = 24;
        protected static readonly float OUTPUT_HEIGHT = 20;
        
        public ActionGraphNodeBase ActionNode;

        protected ActionGraph Graph;

        protected ActionGraphPresenter Presenter;

        protected System.Collections.Generic.List<Connection> Parents = new System.Collections.Generic.List<Connection>();

        public static readonly float ContentMargin = 14; 

        public override string Name => ActionNode.name;

        public ActionGraphEditorNode(ActionGraph graph, ActionGraphNodeBase node, ActionGraphPresenter presenter)
        {
            Graph = graph;
            ActionNode = node;
            Presenter = presenter;
            
            Size = new Vector2(164, 64);
            Position = ActionNode.EditorPosition;

            if (!Inputs.Any())
                Inputs.Add(new Slot(this, SlotType.Input));
            
            ActionNode.ValidationSubscribe(OnTargetChanged);
        }

        public override void OnDestroy()
        {
            if (ActionNode != null)
                ActionNode.ValidationUnsubscribe(OnTargetChanged);
        }

        public virtual void RebuildConnections(Func<object, GraphNode> lookup)
        {
            if (ActionNode is ActionGraphNode asAction)
            {
                foreach (var node in asAction.Connections)
                {
                    var slot = new Slot(this, SlotType.Output);
                    MakeConnection(slot, lookup.Invoke(node)?.GetDefaultInputSlot());
                    Outputs.Add(slot);
                }    
            }
        }

        public override Vector2 GetSlotPosition(Slot slot)
        {
            if (slot.Type == SlotType.Input)
                return new Vector2(0, BASIC_HEIGHT + 0.5f * ConnectorSize.y);
            
            int index = Outputs.IndexOf(slot);
            return new Vector2(Size.x,BASIC_HEIGHT + (ConnectorSize.y * 0.5f) + (index * OUTPUT_HEIGHT));
        }

        protected internal override Slot GetDefaultInputSlot()
        {
            return Inputs.Any() ? Inputs[0] : null;
        }

        protected virtual void OnTargetChanged()
        {
            ActionNode.OnSetupParametrizedProperties();
            Editor.WantsRepaint = true;
        }

        /*public override bool IsAcceptingConnection(GraphNode parent, Vector2 physicalPos)
        {
            if (CanMakeConnection(parent, this))
            {
                var parentPos = PhysicalRect.CenterLeft();
                var dropArea = new Rect
                (
                    parentPos.x - ConnectorSize.x * 0.5f, 
                    parentPos.y - ConnectorSize.y * 0.5f, 
                    ConnectorSize.x, ConnectorSize.y
                );

                return dropArea.Contains(physicalPos);
            }

            return false;
        }*/

        protected override void OnParentDisconnected(Connection node)
        {
            Parents.Remove(node);
        }

        protected override void OnConnectToParent(Connection eventSource)
        {
            Parents.Add(eventSource);
        }

        protected override void OnChildDisconnected(Connection connection)
        {
            connectedTo.Remove(connection);
            if (ActionNode is ActionGraphNode asNode)
            {
                if (connection.Target.Owner is ActionGraphEditorNode asAction) 
                    asNode.Connections.Remove((ActionGraphNode) asAction.ActionNode);
            }
        }

        protected override void OnConnectToChild(Connection eventSource)
        {
            if (ActionNode is ActionGraphNode Self)
            {
                var asAction = eventSource.Target.Owner as ActionGraphEditorNode;
                if (asAction != null && asAction.ActionNode is ActionGraphNode asNode)
                {
                    connectedTo.Add(eventSource);
                    Self.Connections.Add(asNode);
                }                
            }
        }

        void DrawEditableLabel()
        {
            var newName = EditorGUI.TextField(drawRect, ActionNode.name, EditorStyles.whiteLabel);
            if (newName != ActionNode.name)
            {
                Undo.RecordObject(ActionNode, "Rename");
                ActionNode.name = newName;
            }
        }

        protected override void OnRenamed(string newName)
        {
            Undo.RecordObject(ActionNode, "Renamed");
            ActionNode.name = newName;
        }

        protected override void OnSelected(bool value)
        {
            if (value)
                Selection.activeObject = ActionNode;
        }

        protected virtual bool CanRename()
        {
            return true;
        }

        void DrawLabel()
        {
            if (CanRename())
                HandleLabel(drawRect, ActionNode.name, EditorStyles.whiteLabel);
            else
                GUI.Label(drawRect, ActionNode.name, EditorStyles.whiteLabel);
        }

        protected override void OnGUI()
        {
            ActionNode.OnSetupParametrizedProperties();

            if (Selected)
                GUI.color = SpaceEditorStyles.ActiveColor;

            GUI.Box(drawRect, GUIContent.none, WindowStyle);

            ActionNode.EditorPosition = Position;

            var old_rect = drawRect;
            
            drawRect.height = 24;
            
            GUI.Box(drawRect, GUIContent.none, Selected ? SpaceEditorStyles.GraphNodeBackgroundSelected : WindowStyle);

            GUI.color = Color.white;

            drawRect.height = GenericParamUtils.FieldHeight;
            drawRect.y += 4;
            drawRect.x += 4;
            drawRect.width -= 8;

            DrawLabel();

            var old_y  = drawRect.y + drawRect.height + 2;
            var y_margin = old_y - old_rect.y + 2;
            drawRect   = old_rect;
            drawRect.y = old_y;
            drawRect.height -= y_margin;
            drawRect.x += 2;
            drawRect.width -= 4;
        }

        protected override void PostGUI()
        {
            // DrawConnectionSlots(DrawRect);
        }

        protected override void DrawSlot(Slot slot)
        {
            base.DrawSlot(slot);

            if (Event.current.type == EventType.MouseDown
            &&  Event.current.button == 0)
            {
                var rect = GetDrawSlotRect(slot);
                if (rect.Contains(Event.current.mousePosition))
                {
                    // TODO pass actual slot
                    Presenter.OnNodeConnectorClicked(slot, rect.center);
                }                
            }
        }

        protected override void OnDrawContent()
        {
            EditorGUI.HelpBox(drawRect, "No designated editor found! Check code for [CustomActionEditor]", MessageType.Error);
        }

        private Rect ParentDotRect = new Rect();
        private Rect ChildDotRect = new Rect();

        void DrawInputSlots(Rect dotRect)
        {
            foreach (var slots in Inputs)
            {
                
            }

            return;
            
            ParentDotRect.Set(
                dotRect.xMin - ConnectorSize.x * 0.5f, 
                dotRect.center.y - ConnectorSize.y * 0.5f, 
                ConnectorSize.x, 
                ConnectorSize.y
            );
            
            GUI.Box
            (
                ParentDotRect,
                GUIContent.none, SpaceEditorStyles.DotFlowTarget
            );

            if (Parents.Any())
            {
                GUI.color = GetParentConnectColor(Parents.FirstOrDefault(p => p.From.Owner.Selected)?.From.Owner);
                GUI.Box 
                (
                    ParentDotRect,
                    GUIContent.none, SpaceEditorStyles.DotFlowTargetFill
                );                        
            }            
        }

        void DrawOutputSlots(Rect dotRect)
        {
            ChildDotRect.Set(
                dotRect.xMax - ConnectorSize.x * 0.5f, 
                dotRect.center.y - ConnectorSize.y * 0.5f, 
                ConnectorSize.x, 
                ConnectorSize.y
            );

            GUI.Box
            (
                ChildDotRect,
                GUIContent.none, SpaceEditorStyles.DotFlowTarget
            );

            var asAction = ActionNode as ActionGraphNode;
            if (asAction)
            {
                if (asAction.Connections.Any())
                {
                    GUI.color = Selected ? SpaceEditorStyles.ActiveColor : Color.white;
                    GUI.Box 
                    (
                        ChildDotRect,
                        GUIContent.none, SpaceEditorStyles.DotFlowTargetFill
                    );
                }                
            }

            if (ChildDotRect.Contains(Event.current.mousePosition) 
                && Event.current.type == EventType.MouseDown 
                && Event.current.button == 0)
            {
                // TODO pass actual slot
                Presenter.OnNodeConnectorClicked(null, ChildDotRect.center);
            }

            GUI.color = Color.white;
        }

        void DrawConnectionSlots(Rect dotRect)
        {
            DrawInputSlots (dotRect);
            DrawOutputSlots(dotRect);
        }
    }
}