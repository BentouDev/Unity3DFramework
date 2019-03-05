using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Framework.Editor
{
    [CustomActionEditor(typeof(ActionState))]
    public class ActionGraphEditorState : ActionGraphEditorNode, IReorderableNotify
    {
        private static readonly float BASIC_HEIGHT = 24;
        private static readonly float OUTPUT_HEIGHT = 20;

        public ActionState State => (ActionState) ActionNode;

        private readonly PropertyPath ToConnectionInfos;
        private readonly PropertyPath ToChildren;

        public ActionGraphEditorState(ActionGraph graph, ActionGraphNodeBase node, ActionGraphPresenter presenter)
            : base(graph, node, presenter)
        {
            var connection_builder = new PathBuilder<ActionState>();
            connection_builder.ByListOf<ActionState.ConnectionInfo>(l => nameof(State.ConnectionInfos));
            ToConnectionInfos = connection_builder.Path();

            var child_builder = new PathBuilder<ActionState>();
            child_builder.ByListOf<ActionGraphNode>(l => nameof(State.Connections));
            ToChildren = child_builder.Path();

            if (ActionNode)
                ActionNode.Notifiers.Add(this);
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            if (ActionNode)
                ActionNode.Notifiers.Remove(this);
        }

        public override void RebuildConnections(Func<object, GraphNode> lookup)
        {
            for (int i = 0; i < State.ConnectionInfos.Count; i++)
            {
                var slot = new Slot(this, SlotType.Output);

                var index = State.ConnectionInfos[i].ChildId;
                if (index >= 0 && index < State.Connections.Count)
                {
                    connectedTo.Add(MakeConnection(slot, 
                        lookup.Invoke(State.Connections[index])?.GetDefaultInputSlot()));
                }

                Outputs.Add(slot);                        
            }
        }

        protected override void OnConnectToChild(Connection eventSource)
        {
            var asAction = eventSource.Target.Owner as ActionGraphEditorNode;
            var asNode = asAction.ActionNode as ActionGraphNode;

            if (!asNode)
                return;

            var index = Outputs.IndexOf(eventSource.From);
            if (index >= 0)
            {
                var entry = State.ConnectionInfos[index];
                if (entry.ChildId != -1)
                {
                    RemoveConnection(connectedTo.FirstOrDefault(c => c.From.Equals(Outputs[index])));
                }

                State.Connections.Add(asNode);
                entry.ChildId = State.Connections.IndexOf(asNode);
                State.ConnectionInfos[index] = entry;
                connectedTo.Add(eventSource);
            }
        }

        protected override void DrawContent()
        {
            drawRect.height = BASIC_HEIGHT;

            List<string> outNames = new List<string>();
            foreach (var entry in State.ConnectionInfos)
            {
                outNames.Add(entry.Name);                        
            }

            drawRect.y += 1;
            
            Rect oldRect = drawRect;
            drawRect.width -= 10;
            foreach (var outName in outNames)
            {
                GUI.Label(drawRect, outName, SpaceEditorStyles.AlignedRigthWhiteBoldText);
                drawRect.y += OUTPUT_HEIGHT;
            }

            Size = new Vector2(Size.x, BASIC_HEIGHT + drawRect.y - oldRect.y);
        }

        public override Vector2 GetSlotPosition(Slot slot)
        {
            if (SlotType.Output == slot.Type)
            {
                int index = Outputs.IndexOf(slot);

                return new Vector2(Size.x,BASIC_HEIGHT + 7 + (index * OUTPUT_HEIGHT));                
            }

            return base.GetSlotPosition(slot);
        }

        public void OnReordered(PropertyPath path, int oldIndex, int newIndex)
        {
            if (!ToConnectionInfos.Equals(path))
                return;

            var old_node = State.Connections[oldIndex];
            State.Connections.RemoveAt(oldIndex);
            State.Connections.Insert(newIndex, old_node);

            Editor.WantsRepaint = true;
        }

        public void OnAdded(PropertyPath path)
        {
            if (!ToConnectionInfos.Equals(path))
                return;

            Outputs.Add(new Slot(this, SlotType.Output));
            Editor.WantsRepaint = true;
        }

        public void OnRemoved(PropertyPath path, int index)
        {
            if (!ToConnectionInfos.Equals(path))
                return;

            RemoveAllConnectionFrom(Outputs[index]);

            Outputs.RemoveAt(index);
            
            Editor.WantsRepaint = true;
        }

        public bool IsFromPath(PropertyPath path)
        {
            return ToConnectionInfos.Equals(path) || ToChildren.Equals(path);
        }
    }
}