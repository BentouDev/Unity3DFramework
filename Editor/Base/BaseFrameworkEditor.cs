using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Framework.Utils;
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
            internal ReflectionInfo(MemberInfo info, MemberType memberType, bool visible)
            {
                Info           = info;
                IsVisible      = visible;
                MemberType     = memberType;
                //UnderlyingType = info.GetUnderlyingType();

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
                    else if (attrib.AttributeType == typeof(VisibleInInspector))
                    {
                        IsVisible = true;
                    }
                    else if (attrib.AttributeType == typeof(HideInInspector))
                    {
                        IsVisible = false;   
                    }
                }
            }

            private void UpdateResultCache(Framework.IBaseObject instance, ValidationResult result)
            {
                var previous = instance.PreviousResult(Info.Name);
                if (previous != null && !previous.Equals(result))
                    ValidatorWindow.GetInstance().RemoveValidation(previous);

                instance.UpdateValidation(Info.Name, result);                
            }

            public static bool HasPropertyValue(SerializedProperty property)
            {
                if (property.isArray)
                    return property.arraySize > 0;

                return !(property.objectReferenceValue == null && !property.hasVisibleChildren);
            }

            // Move storage of last result to actual instance
            internal ValidationResult CheckValidate(UnityEngine.Object instance, SerializedProperty property)
            {
                ValidationResult result = ValidationResult.Ok;

                if (Validator != null)
                    result = Validator.Invoke(instance);

                else if (RequireValue)
                {
                    if (!HasPropertyValue(property))
                        result = new ValidationResult(ValidationStatus.Error, $"{Info.Name} is required!");
                }

                UpdateResultCache(instance as IBaseObject, result);

                return result;
            }

            internal bool IsVisible = true;
            internal bool RequireValue = false;
            internal string ValidateMethodName;
            internal Func<UnityEngine.Object, ValidationResult> Validator;
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
            typeof(VisibleInInspector),
            typeof(BaseEditorAttribute)
        };

        private static Dictionary<System.Type, List<ReflectionInfo>> ReflectionCache = new Dictionary<Type, List<ReflectionInfo>>();
        private WeakReference<List<ReflectionInfo>> Cache;
        
        public static Func<UnityEngine.Object, ValidationResult> BuildValidationCaller(Type targetType, MethodInfo method)
        {
            var obj  = Expression.Parameter(typeof(UnityEngine.Object), "instance");
            var cast = Expression.Convert(obj, targetType);
            var call = Expression.Call(cast, method);

            return Expression.Lambda<Func<UnityEngine.Object, ValidationResult>>(call, obj).Compile();
        }

        private void ProcessClassMember(List<ReflectionInfo> cache, System.Type targetType, MemberInfo member)
        {
            ReflectionInfo info = null;

            if (member is FieldInfo)
                info = new ReflectionInfo(member, MemberType.Field, ((FieldInfo)member).IsPublic);
            else if (member is PropertyInfo)
                info = new ReflectionInfo(member, MemberType.Property, false);

            if (info != null)
            {
                if (!string.IsNullOrEmpty(info.ValidateMethodName))
                {
                    var methodInfo = targetType.GetMethod(info.ValidateMethodName);
                    if (methodInfo == null)
                        Debug.LogErrorFormat
                        (
                            "Validate: Not method called '{0}' in class '{1}'",
                            info.ValidateMethodName, targetType.Name
                        );
                    else
                        info.Validator = BuildValidationCaller(targetType, methodInfo);
                }

                cache.Add(info);
            }            
        }

        private void Initialize()
        {
            List<ReflectionInfo> cache;
            if (Cache == null || !Cache.TryGetTarget(out cache))
            {
                var targetType = serializedObject.targetObject.GetType();
                if (!ReflectionCache.TryGetValue(targetType, out cache))
                {
                    cache = new List<ReflectionInfo>();

                    foreach (var member in targetType.GetMembers().OrderBy(m => m.MetadataToken))
                    {
                        ProcessClassMember(cache, targetType, member);
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

            (target as IBaseObject)?.OnPreValidate();

            EditorGUI.BeginChangeCheck();
            {
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
            if (EditorGUI.EndChangeCheck())
            {
                (target as IBaseObject)?.OnPostValidate();
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
            if (!info.IsVisible)
                return;
            
            var result = info.CheckValidate(target, property);
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
                var (initialize, list) = ReorderableDrawer.GetList(property);
                if (initialize)
                {
                    if (property.arrayElementType.Equals(typeof(Variant).Name))
                    {
                        list.drawElementCallback += GenericParameterDrawer;
                        list.onAddDropdownCallback += GenericParameterAdd;
                        list.getElementHeightCallback += (e,i) => 16;
                    }
                    else if (property.arrayElementType.Equals(typeof(Framework.Parameter).Name))
                    {
                        list.drawElementCallback += ParameterDraw;
                        list.onAddDropdownCallback += ParameterAdd;
                        list.getElementHeightCallback += (e,i) => 16;                        
                    }
                }

                list.DoLayoutList();
            }

            if (!result)
            {
                GUILayout.BeginVertical();
                using (new EditorGUILayout.HorizontalScope(EditorStyles.helpBox))
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
            if (!info.IsVisible)
                return;

            // EditorGUILayout.HelpBox($"{info.MemberType.ToString()} : {info.Info.Name}", MessageType.Info);
        }

        private void ParameterAdd(Rect buttonrect, ReorderableList list)
        {
            var generic_list = list.ListUnsafe.GetAs<List<Framework.Parameter>>();
            if (generic_list == null)
                Debug.LogError("Unable to get List<Framework.Parameter>!");
            else
                KnownTypeUtils.ShowAddParameterMenu((t) => { AddNewParam(t, generic_list); });
        }

        private void ParameterDraw(Rect rect, SerializedProperty element, GUIContent label, int index, bool selected, bool focused)
        {
            var param = element.GetAs<Framework.Parameter>();
            if (param != null)
            {
                VariantUtils.DrawParameter(rect, param.Value);
            }
            else
            {
                GUI.Label(rect, "Unable to render as Parameter!");
            }
        }

        private void AddNewParam(SerializedType type, List<Framework.Parameter> list)
        {
            string typename = KnownType.GetDisplayedName(type.Type);
            string paramName = StringUtils.MakeUnique($"New {typename}", list.Select(p => p.Name));

            list.Add
            (
                new Framework.Parameter(type, paramName)
            );
        }

        private void GenericParameterAdd(Rect buttonrect, ReorderableList list)
        {
            var generic_list = list.ListUnsafe.GetAs<List<Variant>>();
            if (generic_list == null)
                Debug.LogError("Unable to get List<Variant>!");
            else
                KnownTypeUtils.ShowAddParameterMenu((t) => { AddNewVariant(t, generic_list); });
        }
        
        private void GenericParameterDrawer(Rect rect, SerializedProperty element, GUIContent label, int index, bool selected, bool focused)
        {
            var param = element.GetAs<Variant>();
            if (param != null)
            {
                VariantUtils.DrawParameter(rect, param);
            }
            else
            {
                GUI.Label(rect, "Unable to render as Variant!");
            }
        }
        
        private void AddNewVariant(SerializedType type, List<Variant> list)
        {
            string typename = KnownType.GetDisplayedName(type.Type);
            string paramName = StringUtils.MakeUnique($"New {typename}", list.Select(p => p.Name));

            list.Add
            (
                new Variant(type)
                {
                    Name = paramName
                }
            );
        }

    }
}