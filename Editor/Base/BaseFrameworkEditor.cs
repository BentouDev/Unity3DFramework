using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using UnityEditor;
using UnityEditor.VersionControl;
using UnityEngine;

namespace Framework.Editor
{
    public class BaseFrameworkEditor : UnityEditor.Editor
    {
        enum MemberType
        {
            Field,
            Property
        }

        class ReflectionInfo
        {
            internal delegate bool ValidateDelegate(out string msg);

            internal ReflectionInfo(MemberInfo info, MemberType memberType)
            {
                Info           = info;
                MemberType     = memberType;
                UnderlyingType = info.GetUnderlyingType();

                foreach (var attrib in info.GetCustomAttributesData())
                {
                    foreach (Type attributeType in AttributeTypes)
                    {
                        if (attrib.AttributeType.IsSubclassOf(attributeType))
                            Attributes.Add(attrib);
                    }

                    if (attrib.AttributeType == typeof(Validate))
                    {
                        ValidateMethodName = attrib.ConstructorArguments[0].Value as string;
                    }
                    else if (attrib.AttributeType == typeof(RequireValue))
                    {
                        RequireValue = true;
                    }
                }
            }

            internal ValidationResult CheckValidate(SerializedProperty property)
            {
                ValidationResult result = ValidationResult.Ok;

                if (Validator != null)
                    result = Validator.Invoke();

                else if (RequireValue)
                {
                    if (property.objectReferenceValue == null && !property.hasVisibleChildren)
                        result = new ValidationResult(ValidationStatus.Error, $"{Info.Name} is required!");
                }

                if (PreviousResult != null && !PreviousResult.Equals(result))
                    ValidatorWindow.GetInstance().RemoveValidation(PreviousResult);

                PreviousResult = result;

                return result;
            }

            internal ValidationResult PreviousResult;
            internal bool RequireValue = false;
            internal string ValidateMethodName;
            internal System.Func<ValidationResult> Validator;
            internal Type UnderlyingType;
            internal MemberType MemberType;
            internal MemberInfo Info;
            internal List<CustomAttributeData> Attributes = new List<CustomAttributeData>();
        }
        
        static List<Type> AttributeTypes = new List<Type>()
        {
            typeof(TooltipAttribute),
            typeof(HeaderAttribute),
            typeof(SpaceAttribute),
            typeof(HideInInspector),
            typeof(BaseEditorAttribute)
        };

        private static Dictionary<System.Type, List<ReflectionInfo>> ReflectionCache = new Dictionary<Type, List<ReflectionInfo>>();
        private WeakReference<List<ReflectionInfo>> Cache;
        
        private void Initialize()
        {
            List<ReflectionInfo> cache;
            if (Cache == null || !Cache.TryGetTarget(out cache))
            {
                var targetType = serializedObject.targetObject.GetType();
                if (!ReflectionCache.TryGetValue(targetType, out cache))
                {
                    cache = new List<ReflectionInfo>();

                    foreach (var member in targetType.GetMembers())
                    {
                        ReflectionInfo info = null;

                        if (member is FieldInfo)
                            info = new ReflectionInfo(member, MemberType.Field);
                        else if (member is PropertyInfo)
                            info = new ReflectionInfo(member, MemberType.Property);

                        if (info != null)
                        {
                            if (!string.IsNullOrEmpty(info.ValidateMethodName))
                            {
                                info.Validator = Delegate.CreateDelegate(typeof(System.Func<ValidationResult>),
                                    serializedObject.targetObject,
                                    info.ValidateMethodName, false) as System.Func<ValidationResult>;
                            }

                            cache.Add(info);                            
                        }
                    }

                    ReflectionCache[serializedObject.targetObject.GetType()] = cache;
                }

                if (Cache != null)
                    Cache.SetTarget(cache);
                else
                    Cache = new WeakReference<List<ReflectionInfo>>(cache);
            }
        }

        [UnityEditor.Callbacks.DidReloadScripts]
        private static void OnScriptsReloaded()
        {
            ReflectionCache.Clear();
        }
        
        public override void OnInspectorGUI()
        {
            Initialize();

            EditorGUI.BeginChangeCheck();
            serializedObject.Update();

            DrawType();

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }

        private void DrawType()
        {
            List<ReflectionInfo> cache;
            Cache.TryGetTarget(out cache);
            
            InspectorUtils.DrawDefaultScriptField(serializedObject);

            foreach (ReflectionInfo info in cache)
            {
                var property = serializedObject.FindProperty(info.Info.Name);                
                if (property != null)
                {
                    DrawField(info, property);
                }
                else
                {
                    DrawProperty(info);
                }
            }
        }

        private MessageType ToMessageType(ValidationStatus status)
        {
            switch (status)
            {
                case ValidationStatus.Info:
                    return MessageType.Info;
                case ValidationStatus.Warning:
                    return MessageType.Warning;
                case ValidationStatus.Error:
                    return MessageType.Error;
            }
            
            return MessageType.None;
        }

        private void DrawField(ReflectionInfo info, SerializedProperty property)
        {
            var result = info.CheckValidate(property);
            if (!result)
            {
                ValidatorWindow.GetInstance().RegisterValidation(result, target);
                GUILayout.BeginVertical(EditorStyles.helpBox);
            }
            
            if (!property.isArray || property.type == "string")
            {
                // Somehow check if this type can be drawn other way
                // Is it defined with attribute?
                EditorGUILayout.PropertyField(property, true);
            }
            else
            {
                ReorderableList list = ReorderableDrawer.GetList(property);
                list.DoLayoutList();
            }

            if (!result)
            {
                GUILayout.BeginVertical();
                using (var layout = new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
                {
                    GUILayout.Box(SpaceEditorStyles.GetValidationIcon(result.Status), GUIStyle.none, GUILayout.Width(24));
                    GUILayout.Label(result.Message, EditorStyles.wordWrappedMiniLabel);
                }
                GUILayout.EndVertical();
                GUILayout.EndVertical();
            }
        }

        private void DrawProperty(ReflectionInfo info)
        {
            // EditorGUILayout.HelpBox($"{info.MemberType.ToString()} : {info.Info.Name}", MessageType.Info);
        }
    }
}