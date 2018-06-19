using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using Framework;

public class ParamListDrawer
{
    private static readonly int ReorderableListOffset = 14;

    private ReorderableList _drawerList;
    public  ReorderableList DrawerList => _drawerList;

    private List<GenericParameter> Parameters;

    public void Init(List<GenericParameter> paramList)
    {
        Parameters = paramList;
        Recreate();
    }

    public void Recreate()
    {
        GenericParamUtils.SetDrawersForKnownTypes();

        _drawerList = new ReorderableList
        (
            Parameters, typeof(GenericParameter),
            true, false, true, true
        );
        
        DrawerList.drawHeaderCallback += rect =>
        {
            rect.x      += ReorderableListOffset;
            rect.width  -= ReorderableListOffset;

            var original = rect;

            rect.width *= 0.5f;
            GUI.Label(rect, "Name");

                
            rect.x = original.x + GenericParamUtils.LabelWidth * 1.35f;
            rect.width = GenericParamUtils.FieldWidth;
            GUI.Label(rect, "Default value");
        };

        DrawerList.onAddDropdownCallback += OnAddParameter;
        DrawerList.drawElementCallback   += OnDrawParameter;
    }
    
    private void OnDrawParameter(Rect rect, int index, bool active, bool focused)
    {
        var parameter = Parameters[index];

        rect.height = GenericParamUtils.FieldHeight;
        rect.y     += 2;

        GenericParamUtils.DrawParameter(rect, parameter);
    }
    
    private void OnAddParameter(Rect buttonrect, ReorderableList list)
    {
        ShowAddParameterMenu();
    }

    private void ShowAddParameterMenu()
    {
        GenericMenu menu = new GenericMenu();

        var types = KnownType.GetKnownTypes();
        for (int i = 0; i < types.Count; i++)
        {
            var type = types[i];
            menu.AddItem(new GUIContent(type.GenericName), false, CreateNewParameterCallback(type.HoldType));
        }

        menu.ShowAsContext();
    }
    
    UnityEditor.GenericMenu.MenuFunction CreateNewParameterCallback(Type type)
    {
        return () =>
        {
            OnAddNewParameter(type);
        };
    }
    
    private void AddNewParam(Type type)
    {
        string typename  = GenericParameter.GetDisplayedName(type);
        string paramName = StringUtils.MakeUnique(string.Format("New {0}", typename), Parameters.Select(p => p.Name));

        Parameters.Add
        (
            new GenericParameter(type)
            {
                Name = paramName
            }
        );
    }

    private void OnObjectReferenceTypeSelected(System.Type type)
    {
        AddNewParam(type);
    }

    private void OnAddNewParameter(Type type)
    {
        if (type.IsSubclassOf(typeof(UnityEngine.Object)) && type != typeof(GameObject))
        {
            ReferenceTypePicker.ShowWindow(type, OnObjectReferenceTypeSelected, t => t.IsSubclassOf(type));
        }
        else
        {
            AddNewParam(type);
        }
    }
}
