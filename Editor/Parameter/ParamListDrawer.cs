using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEditor;
using Framework;
using Framework.Editor;
using ReorderableList = UnityEditorInternal.ReorderableList;

public class ParamListDrawer
{
    public static readonly int ReorderableListOffset = 14;

    private ReorderableList _drawerList;
    public  ReorderableList DrawerList => _drawerList;

    private float FirstPos;
    private List<float> Positions = new List<float>();
    private List<GenericParameter> Parameters;
    private bool CustomAdd;

    public List<GenericParameter> GetParameters()
    {
        return Parameters;
    }

    public void Init(List<GenericParameter> paramList, bool customAdd = false)
    {
        CustomAdd = customAdd;
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

                
            rect.x = original.x + rect.width;
            rect.width = GenericParamUtils.FieldWidth;
            GUI.Label(rect, "Value");
        };

        if (!CustomAdd)
            DrawerList.onAddDropdownCallback += OnAddParameter;

        DrawerList.drawElementCallback += OnDrawParameter;
    }
    
    private void OnDrawParameter(Rect rect, int index, bool active, bool focused)
    {
        var parameter = Parameters[index];

        if (index == 0)
            FirstPos = rect.y;

        Positions.Resize(index + 1);
        Positions[index] = rect.y - FirstPos;

        rect.height = GenericParamUtils.FieldHeight;
        rect.y     += 2;

        GenericParamUtils.DrawParameter(rect, parameter);
    }
    
    private void OnAddParameter(Rect buttonrect, ReorderableList list)
    {
        Positions.Resize(Parameters.Count);
        KnownTypeUtils.ShowAddParameterMenu(AddNewParam);
    }

    public float GetFirstOffset()
    {
        return FirstPos;
    }

    public float GetPos(int index)
    {
        return index >= 0 && index < Positions.Count ? Positions[index] : 0;
    }

    private void AddNewParam(SerializedType type)
    {
        string typename = KnownType.GetDisplayedName(type.Type);
        string paramName = StringUtils.MakeUnique($"New {typename}", Parameters.Select(p => p.Name));

        Parameters.Add
        (
            new GenericParameter(type)
            {
                Name = paramName
            }
        );
    }
}
