using UnityEditor;
using UnityEngine;

namespace Framework
{
    public static class AssetDatabaseUtils
    {
        public static void AddToParentAsset(Object asset, Object obj)
        {
            var path = AssetDatabase.GetAssetPath(asset);
            if (!string.IsNullOrEmpty(path))
            {
                var parent = AssetDatabase.LoadAssetAtPath(path, AssetDatabase.GetMainAssetTypeAtPath(path));
                obj.hideFlags |= HideFlags.HideInHierarchy;
                AssetDatabase.AddObjectToAsset(obj, parent);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
        }

        public static void AddToAsset(Object asset, Object obj)
        {
            obj.hideFlags |= HideFlags.HideInHierarchy;
            AssetDatabase.AddObjectToAsset(obj, asset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();            
        }
    }
}