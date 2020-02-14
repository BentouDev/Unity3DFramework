using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using Framework.Editor;
using Framework.Utils;
using UnityEditor;
using UnityEngine;

public static class SerializedPropertyExtension
{
    public static T GetAs<T>(this SerializedProperty prop) where T : class
    {
        var targetObject = prop.serializedObject.targetObject;
        var targetObjectClassType = targetObject.GetType();
        var (_, propObject) = ProcessPropertyPath(targetObjectClassType, targetObject, prop.propertyPath);
        return propObject as T;
    }

    public static PropertyPath GetPath(this SerializedProperty prop)
    {
        PropertyPath result = new PropertyPath();
        GetPath(prop, in result);
        return result;
    }

    public static void GetPath(this SerializedProperty prop, in PropertyPath path)
    {
        path.Clear();

        var targetObject = prop.serializedObject.targetObject;
        var targetObjectClassType = targetObject.GetType();
        ProcessPropertyPath(targetObjectClassType, targetObject, prop.propertyPath, null, path);
    }

    private static string ArrayPrefix = "Array.data[";
    private static string ArraySuffix = "]";
    private static string ArrayFieldName = "array";

    private static Pair<string, object> ProcessPropertyPath(
        System.Type type, object target, string path, 
        string label = "", PropertyPath propertyPath = null)
    {
        if (string.IsNullOrEmpty(path))
            return PairUtils.MakePair(label, target);
        
        if (path.Equals(ArrayFieldName))
            return PairUtils.MakePair(label, target);

        string baseMember = string.Empty;
        object newTarget = null;
        Type targetType = null;
        if (path.StartsWith(ArrayFieldName))
        {
            // skip 'array' fields
            return ProcessPropertyPath(
                type, target, 
                path.Substring(ArrayFieldName.Length + 1), 
                baseMember, propertyPath
            );
        }
        
        if (path.StartsWith(ArrayPrefix))
        {
            int index = -1;
            int startIndex = ArrayPrefix.Length;
            int endIndex = path.IndexOf(ArraySuffix);
            int endLenght = endIndex - startIndex;
            int.TryParse(path.Substring(startIndex, endLenght), out index);

            // What if there's a null in the list?
            var genericList = target as IList;
            newTarget = genericList?[index];

            if (newTarget == null)
                throw new Exception($"Unable to materialize indexer from path '{path}'!");

            targetType = newTarget.GetType();
            path = path.Substring(endIndex + ArraySuffix.Length);
        }
        else
        {
            baseMember = path;
            int index = path.IndexOf('.');
            if (index != -1)
            {
                baseMember = path.Substring(0, index);
            }

            if (type == null || target == null)
                throw new Exception($"Unable to materialize field from path '{path}'!");

            var field = type.GetField(baseMember);
            if (field == null)
                throw new Exception($"Unable to process path fragment '{baseMember}' in path '{path}'!");
            
            newTarget = field.GetValue(target);
            targetType = field.FieldType;
        }

        if (propertyPath != null && !string.IsNullOrEmpty(baseMember))
        {
            propertyPath.Append(baseMember, targetType);
        }

        var dotIndex = path.IndexOf('.');
        if (dotIndex != -1)
        {
            return ProcessPropertyPath(
                targetType, newTarget, 
                path.Substring(dotIndex + 1), 
                baseMember, propertyPath
            );  
        }
        else
        {
            return PairUtils.MakePair(baseMember, newTarget);
        }
    }
}
