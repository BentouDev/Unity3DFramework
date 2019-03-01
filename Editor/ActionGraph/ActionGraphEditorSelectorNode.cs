using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    [CustomActionEditor(typeof(ActionSelector))]
    public class ActionGraphEditorSelectorNode : ActionGraphEditorNode
    {
        private static readonly float BOTTOM_MARGIN = 18;
        protected ActionSelector Node => ActionNode as ActionSelector;
        
        private UnityEditorInternal.ReorderableList DrawerList;
        private float PreferredWidth;

        private float FirstPos;

        private List<int> ProxyList;
        
        private List<float> _positions = new List<float>();
        private List<float> Positions
        {
            get
            {
                _positions.Resize(Node.Inputs.Count + 1);
                return _positions;
            }
        }

        public ActionGraphEditorSelectorNode(ActionGraph graph, ActionGraphNode node, ActionGraphPresenter presenter)
            : base(graph, node, presenter)
        {
            RecreateDrawer();
            
            Size = new Vector2(400,300);

            PreferredWidth = drawRect.width;
        }

        protected override void DrawContent()
        {
            ProxyList.Resize(Node.Inputs.Count);

            drawRect.x += 2;
            drawRect.width -= 4;

            EditorGUI.BeginChangeCheck();
            {
                var old_rect = drawRect;

                drawRect.y += 2;
                drawRect.width = drawRect.width - 20;

                //drawRect.y += 20;
                drawRect.height = GenericParamUtils.FieldHeight;
                string content = Node.InputType.ToString();
                if (GUI.Button(drawRect, content, EditorStyles.objectField))
                {
                    KnownTypeUtils.ShowAddParameterMenu(OnTypeChanged);
                }

                drawRect.x = old_rect.x;
                drawRect.width = old_rect.width;

                drawRect.y += GenericParamUtils.FieldHeight + 4;
                drawRect.height = old_rect.height - drawRect.y;

                var base_y = drawRect.y - old_rect.y + BOTTOM_MARGIN;
                if (DrawerList != null)
                {
                    DrawerList.DoList(drawRect);
                    Size = new Vector2(Size.x, base_y + DrawerList.GetHeight() + 8);
                }    
            }
            if (EditorGUI.EndChangeCheck())
            {
                AssetDatabase.SaveAssets();
            }
        }
        
        void RecreateDrawer()
        {
            GenericParamUtils.SetDrawersForKnownTypes();

            ProxyList = new List<int>(Node.Inputs.Count);

            DrawerList = new UnityEditorInternal.ReorderableList
            (
                ProxyList, typeof(ActionSelector.EntryPoint),
                true, false, true, true
            );

            DrawerList.drawHeaderCallback += rect =>
            {
                rect.x      += ParamListDrawer.ReorderableListOffset;
                rect.width  -= ParamListDrawer.ReorderableListOffset;

                var original = rect;

                rect.width *= 0.5f;
                GUI.Label(rect, "Name");

                
                rect.x = original.x + GenericParamUtils.LabelWidth * 1.35f;
                rect.width = GenericParamUtils.FieldWidth;
                GUI.Label(rect, "Input value");
            };

            DrawerList.onAddCallback += OnAddInput;

            DrawerList.drawElementCallback += OnDrawParameter;

            DrawerList.onCanAddCallback += list => Node.InputType != null && Node.InputType.Type != null;
            DrawerList.onRemoveCallback += list =>
            {
                OnRemoveInputAtIndex(list.index);
            };

            DrawerList.onReorderCallbackWithDetails += (list, index, newIndex) =>
            {
                float pos = Positions[index];

                _positions.RemoveAt(index);
                _positions.Insert(newIndex, pos);

                OnReorderInputAtIndex(index, newIndex);
            };
        }

        private void OnRemoveInputAtIndex(int index)
        {
            var input = Node.Inputs[index];
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

            foreach (var node in input.Nodes)
            {
                Presenter.DisconnectNodes(Node, node);
            }

            Undo.RecordObject(Node, "Input removed");

            Node.Inputs.RemoveAt(index);
        }

        private void OnReorderInputAtIndex(int oldIndex, int newIndex)
        {
            Undo.RecordObject(Node, "Input reordered");

            var oldInput = Node.Inputs[oldIndex];
            
            Node.Inputs.RemoveAt(oldIndex);

            Node.Inputs.Insert(newIndex, oldInput);
        }
        
        private void OnTypeChanged(SerializedType type)
        {
            // ToDo: Move to presenter
            if (Node.InputType != null && Node.InputType == type)
                return;

            if (Node.Inputs != null && Node.Inputs.Any() 
                                     && !EditorUtility.DisplayDialog("Warning!", 
                                         "This will erase existing input entries. Are you sure?", 
                                         "Ok",
                                         "Cancel"))
            {
                return;
            }

            Node.Inputs?.Clear();
            Node.InputType = type;
        }
        
        private void OnAddInput(UnityEditorInternal.ReorderableList list)
        {
            string typename = KnownType.GetDisplayedName(Node.InputType.Type);
            string paramName = StringUtils.MakeUnique($"New {typename}", Node.Inputs.Select(p => p.Input.Name));

            ProxyList.Add(1);
            Node.Inputs.Add
            (
                new ActionSelector.EntryPoint()
                {
                    Input = new GenericParameter(Node.InputType)
                    {
                        Name = paramName
                    } 
                }
            );
        }

        private void OnDrawParameter(Rect rect, int index, bool active, bool focused)
        {
            var parameter = Node.Inputs[index];

            if (index == 0)
                FirstPos = rect.y;

            Positions[index] = rect.y - FirstPos;

            rect.height = GenericParamUtils.FieldHeight;
            rect.y     += 2;

            GenericParamUtils.DrawParameter(rect, parameter.Input);
        }
    }
}