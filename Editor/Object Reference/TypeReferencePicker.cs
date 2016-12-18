using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class TypeReferencePicker : EditorWindow
{
    private static TypeReferencePicker _instance;

    private System.Action<System.Type> OnTypePicked;
    private System.Predicate<System.Type> TypePredicate;

    public static void ShowWindow(System.Action<System.Type> onObjectPicked,
        System.Predicate<System.Type> predicate = null)
    {
        if (_instance == null)
        {
            _instance = CreateInstance<TypeReferencePicker>();
            _instance.OnInit(onObjectPicked, predicate);
            _instance.ShowUtility();
        }
    }

    void OnEnable()
    {
        titleContent = new GUIContent("Pick Object Reference Type");
    }

    void OnDestroy()
    {
        _instance = null;
    }

    void OnInit(
        System.Action<System.Type> onTypePicked,
        System.Predicate<System.Type> predicate
    )
    {
        OnTypePicked = onTypePicked;
        TypePredicate = predicate;
    }

    IEnumerable<System.Type> GetTypesFromAssembly(Assembly assembly, string nameSearch,
        System.Predicate<System.Type> predicate)
    {
        return assembly.GetTypes()
            .Where(t => t.IsClass
                        && t.IsSubclassOf(typeof(UnityEngine.Object))
                        && t.IsPublic
                        && !t.IsAbstract
                        && !t.IsGenericType
                        && (predicate == null || predicate(t))
                        && (string.IsNullOrEmpty(nameSearch) || t.FullName.Contains(nameSearch))
            );
    }

    IEnumerable<System.Type> GetObjectReferenceList(string nameSearch, System.Predicate<System.Type> predicate = null)
    {
        var typeList = new List<System.Type>();

        var executingAssebly = Assembly.GetExecutingAssembly();
        typeList.AddRange(GetTypesFromAssembly(executingAssebly, nameSearch, predicate));

        foreach (var assemblyName in Assembly.GetExecutingAssembly().GetReferencedAssemblies())
        {
            try
            {
                var assembly = Assembly.Load(assemblyName.Name);
                typeList.AddRange(GetTypesFromAssembly(assembly, nameSearch, predicate));
            }
            catch (System.Exception)
            {

            }
        }

        return typeList;
        
        /*return Assembly.GetExecutingAssembly()//GetAssembly(typeof(UnityEngine.Object))
            .GetTypes()
            .Where(t => t.IsClass 
                && !t.IsAbstract 
                && t.IsSubclassOf(typeof(UnityEngine.Object))
                && (string.IsNullOrEmpty(nameSearch) || t.FullName.Contains(nameSearch))
                && (predicate == null || predicate(t)));*/
    }

    private Vector2 scrollPosition;
    private System.Type SelectedType;
    private string SearchString = string.Empty;
    
    void OnGUI()
    {
        GUILayout.BeginVertical();
        {
            GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(20));
            {
                SearchString = GUILayout.TextField(SearchString, (GUIStyle)"SearchTextField", GUILayout.ExpandWidth(true));
                GUILayout.Box(GUIContent.none, (GUIStyle)"SearchCancelButtonEmpty");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true), GUILayout.Height(24));
            {
                GUILayout.TextField("Pick Object Reference Type", EditorStyles.whiteMiniLabel);
            }
            GUILayout.EndHorizontal();
            
            scrollPosition = GUILayout.BeginScrollView(scrollPosition);
            {
                foreach (System.Type type in GetObjectReferenceList(SearchString, TypePredicate))
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
            
            GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(100));
            {
                GUILayout.FlexibleSpace();
                EditorStyles.whiteBoldLabel.normal.textColor = Color.white;
                EditorStyles.whiteBoldLabel.onNormal.textColor = Color.white;
                GUILayout.Label((SelectedType != null ? SelectedType.Name : "None"), EditorStyles.whiteBoldLabel);
                GUILayout.FlexibleSpace();

                if (SelectedType != null)
                {
                    if (GUILayout.Button("Select"))
                    {
                        OnTypePicked(SelectedType);
                        Close();
                    }
                }
            }

            GUILayout.EndVertical();
        }
        GUILayout.EndVertical();

        if (Event.current.type != EventType.Repaint
        &&  Event.current.type != EventType.Layout)
        {
            Event.current.Use();
        }

        Focus();
    }
}
