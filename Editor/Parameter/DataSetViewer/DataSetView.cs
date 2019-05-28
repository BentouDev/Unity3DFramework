using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;

namespace Framework.Editor
{
    public class DataSetView : TreeView
    {
        enum DataSetColumns
        {
            Name,
            Type,
            Value,
        }

        public DataSetView(TreeViewState state) : base(state)
        {
        }

        public DataSetView(TreeViewState state, MultiColumnHeader multiColumnHeader, IDataSet treeModel) : base(state, multiColumnHeader)
        {
        }

        protected override TreeViewItem BuildRoot()
        {
            return null;
        }

        public void SetDataSet(IDataSet current)
        {
            
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float width)
        {
            var columns = new[]
            {
                new MultiColumnHeaderState.Column
                {
                    headerContent = new GUIContent("Name"),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = 150,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column 
                {
                    headerContent = new GUIContent
                    (
                        EditorGUIUtility.FindTexture("FilterByType"), 
                        "Sort by parameter type"
                    ),
                    contextMenuText = "Type",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Right,
                    width = 30, 
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column 
                {
                    headerContent = new GUIContent("Value"),
                    headerTextAlignment = TextAlignment.Right,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = 110,
                    minWidth = 60,
                    autoResize = true
                },
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(DataSetColumns)).Length, 
                "Number of columns should match number of enum values: You probably forgot to update one of them.");

            var state =  new MultiColumnHeaderState(columns);
            return state;
        }
    }
}