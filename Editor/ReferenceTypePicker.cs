using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;

public class ReferenceTypePicker : EditorWindow
{
    private enum AbstractionLevel
    {
        All,
        Concrete,
        Abstract
    }

    private static ReferenceTypePicker _instance;

    private Vector2 scrollPosition = Vector2.zero;

    private Action<Type>    OnTypePicked;
    private Predicate<Type> TypePredicate;

    private string SearchString = string.Empty;
    private AbstractionLevel Abstraction = AbstractionLevel.All;

    private Type BaseType;
    private Type SelectedType;
    private List<Type> AllTypes = new List<Type>();

    #region Window

    public static void ShowWindow(Type baseType, Action<Type> onObjectPicked, Predicate<Type> predicate = null)
    {
        if (_instance == null)
        {
            _instance = CreateInstance<ReferenceTypePicker>();
            _instance.OnInit(baseType, onObjectPicked, predicate);
            _instance.ShowUtility();
        }
    }
    
    void OnInit(
        Type baseType,
        Action<Type> onTypePicked,
        Predicate<Type> predicate
    )
    {
        BaseType = baseType;
        OnTypePicked = onTypePicked;
        TypePredicate = predicate;
        AllTypes = BuildTypeList(string.Empty, TypePredicate).ToList();
    }

    #endregion

    #region Events
    
    void OnDestroy()
    {
        _instance = null;
    }
    private void OnLostFocus()
    {
        Close();
    }

    #endregion
    
    #region GUI

    void OnGUI()
    {
        titleContent = new GUIContent(string.Format("Pick {0} Reference Type", BaseType.Name));
        var typeList = FilterTypeList(SearchString, Abstraction, TypePredicate).ToList();

        GUILayout.BeginVertical();
        {
            DrawSearchField();
            DrawAbstractionSelector();

            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                foreach (Type type in typeList)
                {
                    var isSelected = type == SelectedType;
                    if (GUILayout.Toggle(isSelected, new GUIContent(type.FullName), isSelected ? SpaceEditorStyles.SelectedListItem : SpaceEditorStyles.ListItem,
                        GUILayout.ExpandWidth(true), GUILayout.Height(20)))
                    {
                        SelectedType = type;
                    }
                }
            }
            GUILayout.EndScrollView();

            DrawSelectionInfoPanel();
        }
        GUILayout.EndVertical();

        if (SelectedType != null &&
            Event.current.type == EventType.KeyDown)
        {
            int index = typeList.IndexOf(SelectedType);

            switch (Event.current.keyCode)
            {
                case KeyCode.UpArrow:
                    if (index > 0)
                    {
                        SelectedType = typeList[index - 1];
                        scrollPosition.y = GetScrollPosForIndex(index - 1);
                        Event.current.Use();
                    }
                    break;
                case KeyCode.DownArrow:
                    if (index < typeList.Count - 1)
                    {
                        SelectedType = typeList[index + 1];
                        scrollPosition.y = GetScrollPosForIndex(index + 1);
                        Event.current.Use();
                    }
                    break;
                case KeyCode.Home:
                    SelectedType = typeList.First();
                    scrollPosition.y = GetScrollPosForIndex(0);
                    Event.current.Use();
                    break;
                case KeyCode.End:
                    SelectedType = typeList.Last();
                    scrollPosition.y = GetScrollPosForIndex(typeList.Count - 1);
                    Event.current.Use();
                    break;
            }
        }
    }

    private int GetScrollPosForIndex(int index)
    {
        return index * 16;
    }

    private void DrawSearchField()
    {
        GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(20));
        {
            SearchString = GUILayout.TextField(SearchString, (GUIStyle)"SearchTextField", GUILayout.ExpandWidth(true));
            GUILayout.Box(GUIContent.none, (GUIStyle)"SearchCancelButtonEmpty");
        }
        GUILayout.EndHorizontal();
    }

    private void DrawAbstractionSelector()
    {
        GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(32));
        {
            GUILayout.BeginVertical();
            {
                GUILayout.FlexibleSpace();
                Abstraction = (AbstractionLevel)EditorGUILayout.EnumPopup("Abstraction", Abstraction);
                GUILayout.FlexibleSpace();
            }
            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
    }

    private void DrawSelectionInfoPanel()
    {
        GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(100));
        {
            EditorStyles.whiteBoldLabel.normal.textColor = Color.white;
            EditorStyles.whiteBoldLabel.onNormal.textColor = Color.white;
            
            if (SelectedType != null)
            {
                GUILayout.Label(GetInheritanceLine(SelectedType), EditorStyles.wordWrappedMiniLabel);

                GUILayout.FlexibleSpace();

                GUILayout.Label(string.IsNullOrEmpty(SelectedType.Namespace) ? "::global" : SelectedType.Namespace, EditorStyles.whiteLabel);
                GUILayout.Label(SelectedType.Name, EditorStyles.whiteBoldLabel);
            }
            else
            {
                GUILayout.FlexibleSpace();
                GUILayout.Label("None", EditorStyles.whiteBoldLabel);
            };
        }
        GUILayout.EndVertical();

        GUI.enabled = SelectedType != null;
        {
            if (GUILayout.Button("Select"))
            {
                OnTypePicked(SelectedType);
                Close();
            }
        }
        GUI.enabled = true;
    }

    private void BuildInheritanceLine(StringBuilder builder, Type type)
    {
        builder.Append(" > ");
        builder.Append(type.Name);

        if (type != BaseType)
        {
            BuildInheritanceLine(builder, type.BaseType);
        }
    }

    private string GetInheritanceLine(Type type)
    {
        StringBuilder builder = new StringBuilder();
        builder.Append(type.Name);

        BuildInheritanceLine(builder, type.BaseType);

        return builder.ToString();
    }

    #endregion

    #region TypeList

    IEnumerable<Type> GetTypesFromAssembly(
        Assembly assembly,
        string nameSearch,
        Predicate<Type> predicate)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass
                        && t.IsSubclassOf(typeof(UnityEngine.Object))
                        && t.IsPublic
                        && !t.IsGenericType
                        && (predicate == null || predicate(t))
                        && (string.IsNullOrEmpty(nameSearch) || t.FullName.Contains(nameSearch))
            );
    }

    IEnumerable<Type> BuildTypeList(string nameSearch, Predicate<Type> predicate = null)
    {
        var executingAssebly = Assembly.GetExecutingAssembly();
        var typeList = new List<Type>();

        typeList.AddRange(GetTypesFromAssembly(executingAssebly, nameSearch, predicate));

        foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
        {
            var assembly = Assembly.Load(assemblyName.Name);
            var assemblyTypes = GetTypesFromAssembly(assembly, nameSearch, predicate);

            typeList.AddRange(assemblyTypes);
        }

        return typeList.OrderBy(t => t.FullName);
    }

    private bool CheckAbstraction(Type type, AbstractionLevel level)
    {
        switch (level)
        {
            case AbstractionLevel.All:
                return true;
            case AbstractionLevel.Concrete:
                return !type.IsAbstract;
            default:
                return type.IsAbstract;
        }
    }

    IEnumerable<Type> FilterTypeList(string nameSearch, AbstractionLevel level, Predicate<Type> predicate = null)
    {
        return AllTypes.Where(t => t.IsClass
                                && t.IsSubclassOf(typeof(UnityEngine.Object))
                                && t.IsPublic
                                && !t.IsGenericType
                                && CheckAbstraction(t, level)
                                && (predicate == null || predicate(t))
                                && (string.IsNullOrEmpty(nameSearch) || t.FullName.Contains(nameSearch))
        );
    }

    #endregion
}
