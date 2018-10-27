using UnityEditor;

namespace Framework
{
    [CustomEditor(typeof(DataSet))]
    public class DataSetEditor : UnityEditor.Editor
    {
        private DataSet Target => target as DataSet;
        
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