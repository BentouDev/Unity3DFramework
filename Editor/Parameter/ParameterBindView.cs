using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Object = System.Object;

namespace Framework.Editor
{
    public class ParameterBindView
    {
        private static float BIND_ENUM_WIDTH = 64;
        private float BIND_ENTRY_HEIGHT => EditorGUIUtility.singleLineHeight + 4;

        private int DATASET_ASSET_PICKER_WINDOW_ID;
        private int DATASET_RUNTIME_PICKER_WINDOW_ID;
        
        private UnityEditorInternal.ReorderableList ListDrawer;

        private List<Framework.Parameter> Parameters;
        private IParameterBinder Binder;
        private IDataSetProvider Provider;
        private UnityEngine.Object EditorTarget;

        public UnityEditorInternal.ReorderableList GetList()
        {
            return ListDrawer;
        }

        public float GetHeight()
        {
            return IsValid() ? ListDrawer.GetHeight() + 1 : 0;
        }

        public void DoList(Rect position)
        {
            if (IsValid()) ListDrawer.DoList(position);
        }

        public bool IsValid()
        {
            bool isOk = true;
            isOk &= Parameters != null;
            isOk &= Provider != null;
            isOk &= Binder != null;
            isOk &= EditorTarget != null;
            return isOk;
        }

        public bool Initialize(List<Framework.Parameter> parameters, BindingContext context, UnityEngine.Object target)
        {
            Parameters = parameters;
            Provider = context.Provider;
            Binder = context.Binder;
            EditorTarget = target;

            var isOk = IsValid();

            if (isOk && ListDrawer == null)
            {
                ListDrawer = new UnityEditorInternal.ReorderableList
                (
                    Parameters, 
                    typeof(Framework.Parameter),
                    false, 
                    true, 
                    false, 
                    false
                );

                ListDrawer.drawHeaderCallback += rect =>
                {
                    var oldColor = GUI.color; 
                    GUI.color = Color.white;
                    GUI.Label(rect, "Parameter Bindings", EditorStyles.whiteLabel);
                    GUI.color = oldColor;
                };

                ListDrawer.drawElementBackgroundCallback += (rect, index, active, focused) => { };
                ListDrawer.elementHeight = BIND_ENTRY_HEIGHT;
                ListDrawer.showDefaultBackground = false;
                ListDrawer.drawElementCallback += OnDrawBinding;
            }

            return isOk;
        }

        private void OnDrawBinding(Rect rect, int index, bool active, bool focused)
        {
            EditorGUI.BeginDisabledGroup(Application.isPlaying);
            {
                var oldColor = GUI.color;
                GUI.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), 0.65f);
                GUI.Box(rect, GUIContent.none);
                GUI.color = Color.white;
    
                var param = Parameters[index];
                var oldRect = rect;
                rect = EditorGUI.PrefixLabel(rect, new GUIContent(param.Name), SpaceEditorStyles.InvisibleText);
    
                var labelRect = oldRect;
                labelRect.width = rect.x - oldRect.x;
                EditorGUI.SelectableLabel(labelRect, param.Name, EditorStyles.whiteMiniLabel);
    
                {
                    rect.width -= BIND_ENUM_WIDTH;
    
                    var binding = Binder.GetSyncList(Parameters)?.FirstOrDefault(b => b.Param.ParameterId == param.Id);
                    if (binding != null)
                    {
                        switch (binding.Type)
                        {
                            case BindingType.Unbound:
                                EditorGUI.HelpBox(rect, "UNBOUND", MessageType.Warning);
                                break;
                            case BindingType.Value:
                                HandleBindValue(rect, binding);
                                break;
                            case BindingType.LocalProperty:
                                HandleBindProperty(rect, binding);
                                break;
                            case BindingType.DataSetVariable:
                                HandleBindDataSet(rect, binding);
                                break;
                        }
    
                        rect.x += rect.width;
                        rect.y += 3;
                        rect.width = BIND_ENUM_WIDTH;

                        var newType = (BindingType) EditorGUI.EnumPopup(rect, GUIContent.none, binding.Type);
                        if (binding.Type != newType)
                        {
                            OnChangeBindingType(binding, Binder, newType);
                        }
                    }
                }
    
                GUI.color = oldColor;
            }
            EditorGUI.EndDisabledGroup();
        }

        void OnChangeBindingType(ParamBinding binding, IParameterBinder binder, BindingType type)
        {
            binding.SetBindingType(type, Provider);
        }

        void HandleBindValue(Rect rect, ParamBinding binding)
        {
            if (binding == null)
                return;

            var fieldRect = rect;
            
            
            fieldRect.y += 2;
            fieldRect.height = EditorGUIUtility.singleLineHeight;
            
            VariantUtils.DrawParameter(fieldRect, binding.DefaultValue, false);
        }

        void HandleBindDataSet(Rect rect, ParamBinding binding)
        {
            if (binding == null)
                return;
            
            var buttonRect = rect;
            buttonRect.y += 2;
            buttonRect.height = EditorGUIUtility.singleLineHeight;
            buttonRect.width *= 0.5f;

            
            if (Event.current.commandName == "ObjectSelectorUpdated")
            {
                if (EditorGUIUtility.GetObjectPickerControlID() == DATASET_RUNTIME_PICKER_WINDOW_ID)
                {
                    GUI.changed = true;

                    var go = EditorGUIUtility.GetObjectPickerObject() as GameObject;
                    if (go != null)
                        binding.Context = go.GetComponent<DataSetInstance>();

                    DATASET_RUNTIME_PICKER_WINDOW_ID = -1;
                }
                else if (EditorGUIUtility.GetObjectPickerControlID() == DATASET_ASSET_PICKER_WINDOW_ID)
                {
                    GUI.changed = true;
                    
                    binding.Context = EditorGUIUtility.GetObjectPickerObject();
                    DATASET_ASSET_PICKER_WINDOW_ID = -1;
                }
            }

            
            // Button to invoke menu 
            // local
            // --
            // Pick runtime...
            // Pick asset...
            if (EditorGUI.DropdownButton(buttonRect, new GUIContent(binding?.ContextAsDataSet?.ToString()), FocusType.Passive, EditorStyles.objectField))
            {
                GenericMenu pickDataset = new GenericMenu();
                pickDataset.AddItem(new GUIContent("local (from parametrized object IProvider)"), false, () =>
                {
                    GUI.changed = true;
                });

                pickDataset.AddSeparator(string.Empty);

                pickDataset.AddItem(new GUIContent("Pick runtime..."), false, () =>
                {
                    DATASET_RUNTIME_PICKER_WINDOW_ID = GUIUtility.GetControlID(FocusType.Passive) + 200;

                    EditorGUIUtility.ShowObjectPicker<DataSetInstance>
                    (
                        null, true, string.Empty,
                        DATASET_RUNTIME_PICKER_WINDOW_ID
                    );
                });

                pickDataset.AddItem(new GUIContent("Pick asset..."), false, () =>
                {
                    DATASET_ASSET_PICKER_WINDOW_ID = GUIUtility.GetControlID(FocusType.Passive) + 100;

                    EditorGUIUtility.ShowObjectPicker<DataSet>
                    (
                        null, false, string.Empty,
                        DATASET_ASSET_PICKER_WINDOW_ID

                    );
                });

                pickDataset.DropDown(buttonRect);
            }

            buttonRect.x += buttonRect.width;

            EditorGUI.BeginDisabledGroup(binding.ContextAsDataSet == null);
            {
                // Button to invoke menu
                // go to DataSet...
                // --
                // list of vars of matching type
                EditorGUI.Popup(buttonRect, 0, new[]
                {
                    "SomeVar",
                }, GUI.skin.button);
            }
    
            EditorGUI.EndDisabledGroup();
        }

        void HandleBindProperty(Rect rect, ParamBinding binding)
        {
            if (binding == null)
                return;
            
            var objectRect = rect;
            objectRect.y += 2;
            objectRect.height = EditorGUIUtility.singleLineHeight;
            objectRect.width *= 0.5f;

            EditorGUI.ObjectField(objectRect, null, typeof(UnityEngine.Object), true);

            objectRect.x += objectRect.width;

            // Button to invoke menu 
            // local
            // --
            // Pick runtime...
            // Pick asset...
            objectRect.y += 2;
            EditorGUI.Popup(objectRect, 0, new[]
            {
                "Component >",
                "Some Var",
                "Some Prop",
            }, SpaceEditorStyles.LightObjectField);
        }
    }
}