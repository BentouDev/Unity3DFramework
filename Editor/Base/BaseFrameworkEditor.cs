using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
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
            internal ReflectionInfo(MemberInfo info, MemberType memberType)
            {
                Info           = info;
                MemberType     = memberType;
                UnderlyingType = info.GetUnderlyingType();

                foreach (var attrib in UnderlyingType.GetCustomAttributesData())
                {
                    foreach (Type attributeType in AttributeTypes)
                    {
                        if (attrib.AttributeType.IsSubclassOf(attributeType))
                            Attributes.Add(attrib);
                    }   
                }
            }

            internal Type                      UnderlyingType;
            internal MemberType                MemberType;
            internal MemberInfo                Info;
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
                if (!ReflectionCache.TryGetValue(serializedObject.targetObject.GetType(), out cache))
                {
                    cache = new List<ReflectionInfo>();
                
                    foreach (var member in serializedObject.targetObject.GetType().GetMembers())
                    {
                        if (member is FieldInfo)
                            cache.Add(new ReflectionInfo(member, MemberType.Field));
                        else if (member is PropertyInfo)
                            cache.Add(new ReflectionInfo(member, MemberType.Property));
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

        private void DrawField(ReflectionInfo info, SerializedProperty property)
        {
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
        }

        private void DrawProperty(ReflectionInfo info)
        {
            // EditorGUILayout.HelpBox($"{info.MemberType.ToString()} : {info.Info.Name}", MessageType.Info);
        }
    }
}