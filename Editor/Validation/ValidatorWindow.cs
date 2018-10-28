using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor
{
    public class ValidatorWindow : EditorView<ValidatorWindow, ValidatorPresenter>
    {
        private Vector2 ListScroll;
        private bool ShowErrors = true;
        private bool ShowWarnings = true;
        
        [MenuItem("Gameplay/Validator")]
        public static void MenuShowEditor()
        {
            FocusOrCreate();
        }

        protected ValidatorWindow()
        {
            Presenter = new ValidatorPresenter(this);
        }
        
        void OnEnable()
        {
            name = " Validator   ";
            titleContent.image = SpaceEditorStyles.InfoIconSmall;
            titleContent.text = name;

            Presenter.OnEnable();
        }
        
        public void RemoveValidation(ValidationResult result)
        {
            Presenter.OnRemoveValidation(result);
            Repaint();
        }

        public void RegisterValidation(ValidationResult result, UnityEngine.Object target = null)
        {
            Presenter.OnRegisterValidation(result, target);
            Repaint();
        }
        
        public void DrawHeader(ICollection<ValidatorPresenter.ValidationEntry> entries)
        {
            using (new EditorGUILayout.HorizontalScope(EditorStyles.toolbar, GUILayout.ExpandWidth(true)))
            {
                GUILayout.Label("Validator");
                GUILayout.FlexibleSpace();

                var warningContent = new GUIContent(
                    $"{entries.Count(e => e.Result.Status == ValidationStatus.Warning)}",
                    SpaceEditorStyles.WarningIconSmall
                );

                var errorContent = new GUIContent(
                    $"{entries.Count(e => e.Result.Status == ValidationStatus.Error)}",
                    SpaceEditorStyles.ErrorIconSmall
                );
                
                ShowWarnings = GUILayout.Toggle(ShowWarnings, 
                    warningContent, 
                    EditorStyles.toolbarButton
                );
                
                ShowErrors = GUILayout.Toggle(ShowErrors, 
                    errorContent,
                    EditorStyles.toolbarButton
                );
            }
        }
        
        public void DrawList(ICollection<ValidatorPresenter.ValidationEntry> entries)
        {
            ListScroll = EditorGUILayout.BeginScrollView(ListScroll);
            foreach (var entry in entries)
            {
                if (entry.Result)
                    continue;
                
                if (!ShowErrors && entry.Result.Status == ValidationStatus.Error)
                    continue;
                
                if (!ShowWarnings && entry.Result.Status == ValidationStatus.Warning)
                    continue;

                GUILayout.BeginVertical();
                EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
                {
                    GUILayout.Box(SpaceEditorStyles.GetValidationIcon(entry.Result.Status), GUIStyle.none, GUILayout.Width(24));

                    string label = $"{(entry.Target ? entry.Target + " - " : string.Empty)} {entry.Result.Message}";
                    if (GUILayout.Button(label, EditorStyles.wordWrappedMiniLabel, GUILayout.ExpandWidth(true)))
                    {
                        if (entry.Target)
                        {
                            EditorGUIUtility.PingObject(entry.Target);
                            Selection.activeObject = entry.Target;
                        }
                    }
                }
                EditorGUILayout.EndHorizontal();
                GUILayout.EndVertical();
            }
            
            EditorGUILayout.EndScrollView();
        }
    }
}