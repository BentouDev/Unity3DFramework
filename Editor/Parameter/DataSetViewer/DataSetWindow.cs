using System.Collections;
using System.Collections.Generic;
using Framework;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
namespace Framework.Editor
{
    public class DataSetWindow : EditorWindow
    {
        // We are using SerializeField here to make sure view state is written to the window 
        // layout file. This means that the state survives restarting Unity as long as the window
        // is not closed. If omitting the attribute then the state just survives assembly reloading 
        // (i.e. it still gets serialized/deserialized)
        [SerializeField] TreeViewState m_TreeViewState;

        // The TreeView is not serializable it should be reconstructed from the tree data.
        DataSetView m_TreeView;
        MultiColumnHeaderState m_TreeHeader;
        SearchField m_SearchField;
        IDataSet m_CurrentDataSet;
        bool m_Initialized;

        Rect multiColumnTreeViewRect => new Rect(20, 30, position.width-40, position.height-60);

        Rect toolbarRect => new Rect (20f, 10f, position.width-40f, 20f);

        Rect bottomToolbarRect => new Rect(20f, position.height - 18f, position.width - 40f, 16f);

        [MenuItem("Framework/DataSet Viewer")]
        public static DataSetWindow GetWindow ()
        {
            var window = GetWindow<DataSetWindow>();
            window.titleContent = new GUIContent("DataSet Viewer");
            window.Focus();
            window.Repaint();
            return window;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset (int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject (instanceID) is IDataSet myDataSet)
            {
                var window = GetWindow ();
                window.OnSetAsset(myDataSet);
                return true;
            }

            return false;
        }

        void InitIfNeeded ()
        {
            if (!m_Initialized || m_TreeView == null)
            {
                // Check if it already exists (deserialized from window layout file or scriptable object)
                if (m_TreeViewState == null)
                    m_TreeViewState = new TreeViewState();

                bool firstInit = m_TreeHeader == null;
                var headerState = DataSetView.CreateDefaultMultiColumnHeaderState(multiColumnTreeViewRect.width);
                if (MultiColumnHeaderState.CanOverwriteSerializedFields(m_TreeHeader, headerState))
                    MultiColumnHeaderState.OverwriteSerializedFields(m_TreeHeader, headerState);
                m_TreeHeader = headerState;

                var multiColumnHeader = new DataSetHeader(headerState);
                if (firstInit)
                    multiColumnHeader.ResizeToFit ();

                m_TreeView = new DataSetView(m_TreeViewState, multiColumnHeader, m_CurrentDataSet);

                m_SearchField = new SearchField();
                m_SearchField.downOrUpArrowKeyPressed += m_TreeView.SetFocusAndEnsureSelectedItem;

                m_Initialized = true;
            }
        }

        void OnSelectionChange ()
        {
            if (!m_Initialized)
                return;

            if (!OnSetAsset(Selection.activeObject as IDataSet))
            {
                if (Selection.activeObject is IDataSetProvider asProvider)
                {
                    var providedSet = asProvider.GetDataSet();
                    OnSetAsset(providedSet);
                }                
            }
        }

        private bool OnSetAsset(IDataSet set)
        {
            if (set != null && set != m_CurrentDataSet)
            {
                m_CurrentDataSet = set;
                m_TreeView.SetDataSet(m_CurrentDataSet);
                m_TreeView.Reload();
                m_Initialized = false;
                return true;
            }

            return false;
        }

        void OnGUI ()
        {
            InitIfNeeded();

            DoToolbar();
            DoTreeView (multiColumnTreeViewRect);
            BottomToolBar (bottomToolbarRect);
        }
        
        void DoToolbar()
        {
            GUILayout.BeginHorizontal (EditorStyles.toolbar);
            GUILayout.Space (100);
            GUILayout.FlexibleSpace();
            m_TreeView.searchString = m_SearchField.OnToolbarGUI (m_TreeView.searchString);
            GUILayout.EndHorizontal();
        }

        void DoTreeView (Rect rect)
        {
            m_TreeView.OnGUI(rect);
        }

        void BottomToolBar(Rect rect)
        {
            GUI.Box(rect, GUIContent.none);
        }
    }
}