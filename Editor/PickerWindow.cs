using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Framework
{
	public class PickerWindow<T> : EditorWindow
	{
		private static PickerWindow<T> _instance;

		private static readonly string SearchFieldControl = "_SearchField";
		private Vector2 scrollPosition = Vector2.zero;

		private string SearchString = string.Empty;

		public string 		   Title = "Pick...";
		public Func<T, string> GetName = arg => arg.ToString();
		public Func<T, string> GetDesc = arg => arg.ToString();
		public Predicate<T>    Filter;
		public Action<T>       OnElementPicked;
		public List<T>         Data;

		public bool 		   CloseOnLostFocus = true;

		private List<T>        CurrentFilterList;

		private T SelectedElement;

		public static void ShowWindow(List<T> data, Action<T> onObjectPicked, Predicate<T> predicate = null)
		{
			if (_instance == null)
			{
				_instance = CreateInstance<PickerWindow<T>>();
			}

			_instance.Data = data;
			_instance.OnElementPicked = onObjectPicked;
			_instance.Filter = predicate;
			
			_instance.ShowUtility();
		}

		IEnumerable<T> GetFilteredData(string nameSearch)
		{
			return Data.Where(e =>
			{
				bool result = true;
				result &= GetName(e).ToLower().Contains(nameSearch);
				result &= Filter?.Invoke(e) ?? true;
				return result;
			});
		}
		
		void OnDestroy()
		{
			_instance = null;
		}

		private void OnLostFocus()
		{
			if (CloseOnLostFocus)
				Close();
		}
		
		void OnGUI()
		{
			titleContent = new GUIContent(Title);

			if (CurrentFilterList == null)
			{
				CurrentFilterList = GetFilteredData(SearchString).ToList();
				SelectedElement = CurrentFilterList.FirstOrDefault();
			}

			HandleKeyboard();
			
			GUILayout.BeginVertical(GUIContent.none, EditorStyles.helpBox);
			{
				DrawSearchField();
	
				scrollPosition = GUILayout.BeginScrollView(scrollPosition);
				{
					foreach (T element in CurrentFilterList)
					{
						bool isSelected = element.Equals(SelectedElement);
						if (GUILayout.Toggle(isSelected, new GUIContent(GetName(element)), 
							isSelected
								? SpaceEditorStyles.SelectedListItem 
								: SpaceEditorStyles.ListItem,
							GUILayout.ExpandWidth(true), GUILayout.Height(20)))
						{
							SelectedElement = element;
						}
					}
				}
				GUILayout.EndScrollView();
	
				DrawSelectionInfoPanel();
			}
			GUILayout.EndVertical();
		}
			
		private void DrawSearchField()
		{
			GUILayout.BeginHorizontal(GUILayout.ExpandWidth(true), GUILayout.Height(20));
			{
				GUI.SetNextControlName(SearchFieldControl);
				var newSearchString = GUILayout.TextField(SearchString, (GUIStyle)"SearchTextField", GUILayout.ExpandWidth(true));
				if (!newSearchString.Equals(SearchString))
				{
					CurrentFilterList = GetFilteredData(newSearchString).ToList();
					SearchString = newSearchString;

					if (!CurrentFilterList.Contains(SelectedElement))
						SelectedElement = CurrentFilterList.FirstOrDefault();
				}
				
				GUI.FocusControl(SearchFieldControl);

				GUILayout.Box(GUIContent.none, (GUIStyle)"SearchCancelButtonEmpty");
			}
			GUILayout.EndHorizontal();
		}

		private void DrawSelectionInfoPanel()
		{
			GUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.Height(100));
			{
				if (SelectedElement != null)
				{
					GUILayout.Label(GetDesc(SelectedElement), EditorStyles.wordWrappedMiniLabel);

					GUILayout.FlexibleSpace();

					GUILayout.Label(GetName(SelectedElement), EditorStyles.boldLabel);
				}
				else
				{
					GUILayout.FlexibleSpace();
					GUILayout.Label("None", EditorStyles.boldLabel);
				};
			}
			GUILayout.EndVertical();

			GUI.enabled = SelectedElement != null;
			{
				if (GUILayout.Button("Select"))
				{
					PickSelected();
				}
			}
			GUI.enabled = true;
		}

		bool PickSelected()
		{
			if (SelectedElement == null)
				return true;
			
			OnElementPicked(SelectedElement);
			Close();

			return false;
		}

		void HandleKeyboard()
		{
			if (Event.current.type == EventType.KeyDown || Event.current.rawType == EventType.KeyDown)
			{
				int index = CurrentFilterList.IndexOf(SelectedElement);

				switch (Event.current.keyCode)
				{
					case KeyCode.Escape:
						Close();
						break;
					case KeyCode.Return:
						if (PickSelected())
							Event.current.Use();
						break;
					case KeyCode.UpArrow:
						if (index == -1)
							index = CurrentFilterList.Count - 1;

						if (index > 0)
						{
							SelectedElement = CurrentFilterList[index - 1];
							scrollPosition.y = GetScrollPosForIndex(index - 1);
							Event.current.Use();
						}

						break;
					case KeyCode.DownArrow:
						if (index == -1)
							index = 0;

						if (index < CurrentFilterList.Count - 1)
						{
							SelectedElement = CurrentFilterList[index + 1];
							scrollPosition.y = GetScrollPosForIndex(index + 1);
							Event.current.Use();
						}

						break;
					case KeyCode.Home:
						if (!Event.current.control)
						{
							SelectedElement = CurrentFilterList.First();
							scrollPosition.y = GetScrollPosForIndex(0);
							Event.current.Use();
						}
						break;
					case KeyCode.End:
						if (!Event.current.control)
						{
							SelectedElement = CurrentFilterList.Last();						
							scrollPosition.y = GetScrollPosForIndex(CurrentFilterList.Count - 1);
							Event.current.Use();
						}
						break;
				}
			}
		}

		private int GetScrollPosForIndex(int index)
		{
			return index * 16;
		}
	}
}
