using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    public class PrefabBrushEditorView : EditorView<PrefabBrushEditorView, PrefabBrushEditorPresenter>
    {
        [MenuItem("Gameplay/Prefab Brush")]
        public static void MenuShowEditor()
        {
            FocusOrCreate();
        }

        protected PrefabBrushEditorView()
        {
            Presenter = new PrefabBrushEditorPresenter(this);
            SceneView.onSceneGUIDelegate -= OnSceneGUI;
        }
        
        public void DrawHandle()
        {
            if (Event.current == null)
                return;

            if (Event.current.type == EventType.MouseMove)
                SceneView.RepaintAll();

            //Ray ray = Camera.current.ScreenPointToRay(Event.current.mousePosition); // HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            //RaycastHit hit;
            //if (Physics.Raycast(ray, out hit))
            //{
            //    Handles.PositionHandle(hit.point, Quaternion.identity);
            //}
        }
        
        void OnSceneGUI(SceneView sceneView)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); // Camera.current.ScreenPointToRay(Event.current.mousePosition); // 

            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                Handles.PositionHandle(hit.point, Quaternion.identity);
            }
        }
    }
}