using UnityEditor;
using UnityEngine;

namespace Framework
{
    public abstract class DataBankEditor : UnityEditor.Editor
    {
        protected abstract DataBank Target { get; }

        private ParamListDrawer _drawer;
        private ParamListDrawer Drawer
        {
            get
            {
                if (_drawer == null)
                {
                    _drawer = new ParamListDrawer();
                    _drawer.Init(Target.GetSerialized());
                    _drawer.Recreate();
                }

                return _drawer;
            }
        }
        
        public override void OnInspectorGUI()
        {
            EditorGUILayout.LabelField("DataSet");

            EditorGUILayout.BeginHorizontal();
            {
                if (GUILayout.Button("Force Save"))
                {
                    Target.SetValues(Drawer.GetParameters());
                    serializedObject.ApplyModifiedProperties();
                    EditorUtility.SetDirty(target);
                    AssetDatabase.SaveAssets();
                }
            }
            EditorGUILayout.EndHorizontal();

            EditorGUI.BeginChangeCheck();
            {
                Drawer.DrawerList.DoLayoutList();
            }
            if (EditorGUI.EndChangeCheck())
            {
                Target.SetValues(Drawer.GetParameters());
                AssetDatabase.SaveAssets();
            }
        }
    }
}