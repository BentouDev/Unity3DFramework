#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework
{
    public class DialogImporter : AssetPostprocessor
    {
        public static readonly string DialogFileType = ".dialog";

        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets,
            string[] movedAssets, string[] movedFromPath)
        {
            Assert.AreEqual(movedAssets.Length, movedFromPath.Length);
            int assetCount = movedAssets.Length;
            for (uint i = 0; i < assetCount; i++)
            {
                bool isDialog = movedAssets[i].EndsWith(DialogFileType);
                Assert.AreEqual(isDialog, movedFromPath[i].EndsWith(DialogFileType), 
                    string.Format("Unity fucked up something while moving file from:\n'{0}' \nto \n'{1}'", 
                    movedFromPath[i], movedAssets[i]));

                if (isDialog)
                {
                    ImportDialog(movedAssets[i]);
                }
            }

            foreach (var path in importedAssets.Where(s => s.EndsWith(DialogFileType)))
            {
                ImportDialog(path);
            }
        }

        private static void ImportDialog(string path)
        {
            int    index   = path.LastIndexOf(DialogFileType, StringComparison.Ordinal);
            string newPath = path.Substring(0, index) + ".asset";

            Dialog dialogAsset = AssetDatabase.LoadAssetAtPath<Dialog>(newPath);
            if (!dialogAsset)
            {
                dialogAsset = ScriptableObject.CreateInstance<Dialog>();
                AssetDatabase.CreateAsset(dialogAsset, newPath);
            }

            dialogAsset.DialogScriptFile = path;
            dialogAsset.RebuildFromScript();
        }
    }
}
#endif