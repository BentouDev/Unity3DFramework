using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Framework.Editor.Base;
using Framework.Utils;
using UnityEditor;
using UnityEditor.Experimental.Networking.PlayerConnection;
using UnityEngine;
using EditorGUI = UnityEditor.EditorGUI;
using EditorGUILayout = UnityEditor.Experimental.Networking.PlayerConnection.EditorGUILayout;

namespace Framework.Editor
{
    [CustomActionEditor(typeof(AnyEntry))]
    [CustomActionEditor(typeof(EventEntry))]
    public class ActionGraphEditorEntryBase : ActionGraphEditorNode, IReorderableNotify
    {
        private static readonly float BASIC_HEIGHT = 24;
        private static readonly float OUTPUT_HEIGHT = 20;

        private readonly PropertyPath ConditionPropertyPath;

        protected AnyEntry AsAny => ActionNode as AnyEntry;
        protected EventEntry AsEvent => ActionNode as EventEntry;

        public ActionGraphEditorEntryBase(ActionGraph graph, ActionGraphNodeBase node, ActionGraphPresenter presenter) 
            : base(graph, node, presenter)
        {
            var builder = new PathBuilder<AnyEntry>();
            ConditionPropertyPath = builder
                .ByListOf<AnyEntry.EntryInfo>(t => nameof(t.Entries))
                .Path();

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
            Inputs.Clear();

            switch (ActionNode)
            {
                case AnyEntry asAny:
                {
                    foreach (var entry in asAny.Entries)
                    {
                        var slot = new Slot(this, SlotType.Output);

                        if (entry.Child)
                            connectedTo.Add(MakeConnection(slot, 
                            lookup.Invoke(entry.Child)?.GetDefaultInputSlot()));

                        Outputs.Add(slot);                        
                    }

                    break;
                }
                case EventEntry asEvent:
                {
                    var slot = new Slot(this, SlotType.Output);
                    if (asEvent.Child)
                        connectedTo.Add(MakeConnection(slot,
                            lookup.Invoke(asEvent.Child)?.GetDefaultInputSlot()));
                    
                    Outputs.Add(slot);
                    break;
                }
            }
        }

        protected override void OnConnectToChild(Connection eventSource)
        {
            var asAction = eventSource.Target.Owner as ActionGraphEditorNode;
            var asNode = asAction.ActionNode as ActionGraphNode;

            if (!asNode)
                return;

            switch (ActionNode)
            {
                case AnyEntry asAny:
                {
                    var index = Outputs.IndexOf(eventSource.From);
                    if (index >= 0)
                    {
                        var entry = asAny.Entries[index];
                        if (entry.Child)
                        {
                            RemoveConnection(connectedTo.FirstOrDefault(c => c.From.Equals(Outputs[index])));
                        }

                        entry.Child = asNode;
                        asAny.Entries[index] = entry;
                        connectedTo.Add(eventSource);
                    }
                    break;
                }
                case EventEntry asEvent:
                {
                    RemoveConnection(connectedTo.FirstOrDefault(c => c.From.Equals(Outputs[0])));
                    asEvent.Child = asNode;
                    connectedTo.Add(eventSource);
                    break;
                }
            }
        }

        protected override bool CanRename()
        {
            return ActionNode is EventEntry;
        }

        public override Vector2 GetSlotPosition(Slot slot)
        {
            Debug.Assert(slot.Type == SlotType.Output);

            int index = Outputs.IndexOf(slot);

            return new Vector2(Size.x,BASIC_HEIGHT + 7 + (index * OUTPUT_HEIGHT));
        }

        protected override void DrawContent()
        {
            drawRect.height = BASIC_HEIGHT;

            List<string> outNames = new List<string>();
            switch (ActionNode)
            {
                case AnyEntry asAny:
                {
                    foreach (var entry in asAny.Entries)
                    {
                        outNames.Add(entry.Name);                        
                    }
                    
                    break;
                }
                case EventEntry asEvent:
                {
                    outNames.Add("out");                        

                    break;
                }
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

        public void OnReordered(PropertyPath path, int oldIndex, int newIndex)
        {
            if (!AsAny || !ConditionPropertyPath.Equals(path))
                return;
            
            var oldInput = AsAny.Entries[oldIndex];
            AsAny.Entries.RemoveAt(oldIndex);
            AsAny.Entries.Insert(newIndex, oldInput);

            var oldSocket = Outputs[oldIndex];
            Outputs.RemoveAt(oldIndex);
            Outputs.Insert(newIndex, oldSocket);

            Editor.WantsRepaint = true;
        }

        public void OnAdded(PropertyPath path)
        {
            if (!AsAny || !ConditionPropertyPath.Equals(path))
                return;

            // All fresh entries shall be clear!
            if (AsAny.Entries.Any())
            {
                var last = AsAny.Entries.Last();
                last.Name = "out";
                last.Child = null;
                last.Conditions.Clear();
                AsAny.Entries[AsAny.Entries.Count - 1] = last;    
            }

            Outputs.Add(new Slot(this, SlotType.Output));
            Editor.WantsRepaint = true;
        }

        public void OnRemoved(PropertyPath path, int index)
        {
            if (!AsAny || !ConditionPropertyPath.Equals(path))
                return;
            
            RemoveAllConnectionFrom(Outputs[index]);

            Outputs.RemoveAt(index);

            Editor.WantsRepaint = true;
        }

        public bool IsFromPath(PropertyPath path)
        {
            return ConditionPropertyPath.Equals(path);
        }
    }
}