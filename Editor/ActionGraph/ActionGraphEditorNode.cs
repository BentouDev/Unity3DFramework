using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    public class ActionGraphEditorNode : GraphNode
    {
        public ActionGraphNode ActionNode;

        public ActionGraph Graph;

        private ActionGraphPresenter Presenter;

        private List<GraphNode> Parents = new List<GraphNode>();

        public static readonly float ContentMargin = 14; 

        public override string Name => ActionNode.name;
        
        public ActionGraphEditorNode(ActionGraph graph, ActionGraphNode node, ActionGraphPresenter presenter)
        {
            Graph = graph;
            ActionNode = node;
            Presenter = presenter;
            
            Size = new Vector2(164, 64);
            Position = ActionNode.EditorPosition;
        }

        public override bool IsAcceptingConnection(GraphNode parent, Vector2 physicalPos)
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
        }

        public override Vector2 GetParentConnectPosition(GraphNode parent)
        {
            return drawRect.CenterLeft();
        }

        public override void GetChildConnectPositions(GraphNode child, IList<Vector2> positions)
        {
            positions.Add(drawRect.CenterRight());
        }

        protected override void OnParentDisconnected(GraphNode node)
        {
            Parents.Remove(node);
        }

        protected override void OnConnectToParent(GraphNode parent)
        {
            Parents.Add(parent);
        }

        protected override void OnConnectToChild(GraphNode node)
        {
            var asAction = node as ActionGraphEditorNode;
            if (asAction != null)
            {
                Presenter.OnConnectChildNode(this, asAction);
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
            Selection.activeObject = ActionNode;
        }

        void DrawLabel()
        {
            // GUI.Label(drawRect, ActionNode.name, EditorStyles.whiteLabel);
            HandleLabel(drawRect, ActionNode.name, EditorStyles.whiteLabel);
        }

        protected override void OnGUI()
        {
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
            DrawConnectDots(DrawRect);
        }

        protected override void OnDrawContent()
        {
            EditorGUI.HelpBox(drawRect, "No designated editor found! Check code for [CustomActionEditor]", MessageType.Error);
        }

        private Rect ParentDotRect = new Rect();
        private Rect ChildDotRect = new Rect();

        void DrawParentConnector(Rect dotRect)
        {
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
                GUI.color = GetParentConnectColor(Parents.FirstOrDefault(p => p.Selected));
                GUI.Box 
                (
                    ParentDotRect,
                    GUIContent.none, SpaceEditorStyles.DotFlowTargetFill
                );                        
            }            
        }

        void DrawChildConnector(Rect dotRect)
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
            
            if (ActionNode.Connections.Any())
            {
                GUI.color = Selected ? SpaceEditorStyles.ActiveColor : Color.white;
                GUI.Box 
                (
                    ChildDotRect,
                    GUIContent.none, SpaceEditorStyles.DotFlowTargetFill
                );
            }
            
            if (ChildDotRect.Contains(Event.current.mousePosition) 
                && Event.current.type == EventType.MouseDown 
                && Event.current.button == 0)
            {
                Presenter.OnNodeConnectorClicked(this, ChildDotRect.center);
            }

            GUI.color = Color.white;
        }

        void DrawConnectDots(Rect dotRect)
        {
            DrawParentConnector(dotRect);
            DrawChildConnector (dotRect);
        }
    }
}