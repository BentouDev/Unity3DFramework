#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.Linq;
using System.Collections.Generic;

public static class EditorList {

	public	delegate bool ElementCondition(SerializedProperty property);

	private static	GUILayoutOption	miniButtonWidth = GUILayout.Width(20f);

	private	static	bool			ConditionFullfiled;
	private	static	List<int>		InvalidElements = new List<int>();

	private static GUIContent
		moveButtonContent		= new GUIContent("\u21b4",	"move down"),
		duplicateButtonContent	= new GUIContent("+",		"duplicate"),
		deleteButtonContent		= new GUIContent("-",		"delete"),
		addButtonContent		= new GUIContent("+",		"add element");

	public static void Show (SerializedProperty list, EditorListOption options = EditorListOption.Default, ElementCondition condition = null)
	{
		ConditionFullfiled = true;
		InvalidElements.Clear();

		if (!list.isArray) {
			EditorGUILayout.HelpBox(list.name + " is neither an array nor a list!", MessageType.Error);
			return;
		}

		bool	showListLabel	= (options & EditorListOption.ListLabel)	!= 0,
				showListSize	= (options & EditorListOption.ListSize)		!= 0;

		if (showListLabel)
		{
			EditorGUILayout.PropertyField(list);
			EditorGUI.indentLevel += 1;
		}

		if(!showListLabel || list.isExpanded)
		{
			//if (showListSize)
			//	EditorGUILayout.PropertyField(list.FindPropertyRelative("Array.size"));
			//ShowElements(list, options);
			SerializedProperty size = list.FindPropertyRelative("Array.size");
			if (showListSize) {
				EditorGUILayout.PropertyField(size);
			}
			if (size.hasMultipleDifferentValues) {
				EditorGUILayout.HelpBox("Not showing lists with different sizes.", MessageType.Info);
			}
			else {
				ShowElements(list, options, condition);
			}
		}

		if (!ConditionFullfiled) {
			string number = "";
			foreach(int i in InvalidElements)
				number += i + ", ";

			EditorGUILayout.HelpBox("Elements: " +  number + "are not fulfiling given conditions!", MessageType.Error);
		}

		if(showListLabel)
			EditorGUI.indentLevel -= 1;
	}

	private static void ShowElements (SerializedProperty list, EditorListOption options, ElementCondition condition)
	{
		bool	showElementLabels	= (options & EditorListOption.ElementLabels)	!= 0,
				showButtons			= (options & EditorListOption.Buttons)			!= 0,
                showAdd             = (options & EditorListOption.AddButton)        != 0;
		
		for (int i = 0; i < list.arraySize; i++) {

			if (condition != null && !condition(list.GetArrayElementAtIndex(i))) {
				ConditionFullfiled = false;
				InvalidElements.Add(i);
			}

			EditorGUILayout.BeginVertical();

			if (showButtons) {
				GUILayout.BeginHorizontal();
			}
			if (showElementLabels) {
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUILayout.ExpandWidth(true));
			}
			else {
				EditorGUILayout.PropertyField(list.GetArrayElementAtIndex(i), GUIContent.none, GUILayout.ExpandWidth(true));
			}
			if (showButtons) {
				ShowButtons(list, i);
				GUILayout.EndHorizontal();
			}

			EditorGUILayout.EndVertical();

		}
		if (((showButtons && list.arraySize == 0) || showAdd) && GUILayout.Button(addButtonContent, EditorStyles.miniButton)) {
			list.arraySize++;
		}
	}

	private static bool ShowButtons (SerializedProperty list, int index)
	{
		bool result = false;

		EditorGUILayout.BeginHorizontal(GUILayout.Width(40));
		if (GUILayout.Button(moveButtonContent, EditorStyles.miniButtonLeft, miniButtonWidth)) {
			list.MoveArrayElement(index, index + 1);
		}
		if (GUILayout.Button(duplicateButtonContent, EditorStyles.miniButtonMid, miniButtonWidth)) {
			if(index == list.arraySize)
				list.arraySize++;
			else
				list.InsertArrayElementAtIndex(index);
		}
		if (GUILayout.Button(deleteButtonContent, EditorStyles.miniButtonRight, miniButtonWidth)) {
			//	HAAAAAAAACKS!!!
			if(index == (list.arraySize - 1)){
				list.arraySize--;
			} else {
				list.DeleteArrayElementAtIndex(index);
			}

			result = true;
			list.serializedObject.ApplyModifiedProperties();
		}
		EditorGUILayout.EndHorizontal();
		return result;
	}
}
#endif