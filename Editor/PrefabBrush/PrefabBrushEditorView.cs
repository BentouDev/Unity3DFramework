using UnityEngine;
using UnityEditor;

namespace Framework.Editor
{
    public class PrefabBrushEditorView : EditorView<PrefabBrushEditorView, PrefabBrushEditorPresenter>
    {
        private readonly float HandleSize = 1.5f;
        private readonly Color HandleColor = new Color(0.75f, 0.25f, 1, 0.25f);
        private RaycastHit LastHit;

        private GameObject Prefab;
        private Transform Parent;

        [MenuItem("Gameplay/Prefab Brush")]
        public static void MenuShowEditor()
        {
            FocusOrCreate();
        }

        protected PrefabBrushEditorView()
        {
            Presenter = new PrefabBrushEditorPresenter(this);
        }

        private bool IsPainting;

        public void DrawHandle()
        {
            // If something overrides us, disable automatically
            if (IsPainting && SceneView.onSceneGUIDelegate != OnSceneGUI)
                IsPainting = false;

            IsPainting = GUILayout.Toggle(IsPainting, new GUIContent("Paint"), EditorStyles.miniButton);
            if (IsPainting)
            {
                SceneView.onSceneGUIDelegate = OnSceneGUI;
            }
            else
            {
                if (SceneView.onSceneGUIDelegate == OnSceneGUI)
                    SceneView.onSceneGUIDelegate = null;
            }

            Prefab = EditorGUILayout.ObjectField("Prefab", Prefab, typeof(GameObject), false) as GameObject;
            Parent = EditorGUILayout.ObjectField("Parent", Parent, typeof(Transform), true) as Transform;
            
            if (Event.current == null)
                return;

            if (Event.current.type == EventType.MouseMove)
            {
                SceneView.RepaintAll();
            }
        }

        void OnSceneGUI(SceneView sceneView)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition); // Camera.current.ScreenPointToRay(Event.current.mousePosition); // 

            RaycastHit newHit;
            if (Physics.Raycast(ray, out newHit))
            {
                var view = Camera.current.WorldToViewportPoint(newHit.point);
                if (view.x < 1 && view.x > 0 && view.y < 1 && view.y > 0 && view.z > 0)
                {
                    LastHit = newHit;
                    Handles.color = HandleColor;
                    Handles.ConeHandleCap(0, LastHit.point, Quaternion.LookRotation(LastHit.normal), HandleSize, EventType.Repaint);
                }

                sceneView.Repaint();
            }
            
            if (!IsPainting)
                return;

            if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
            {
                if (Prefab)
                {
                    GameObject go = Instantiate(Prefab, LastHit.point, Quaternion.LookRotation(Vector3.forward, LastHit.normal));
                    if (Parent)
                        go.transform.SetParent(Parent, true);

                    Undo.RegisterCreatedObjectUndo(go, "Instantiaded prefab");
                    Event.current.Use();
                }
            }
        }
    }
}