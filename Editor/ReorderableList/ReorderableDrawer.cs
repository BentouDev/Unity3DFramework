using System.Collections.Generic;
using Framework.Editor.Base;
using Framework.Utils;
using Malee;
using UnityEditor;
using UnityEngine;

namespace Framework.Editor 
{
	[CustomPropertyDrawer(typeof(ReorderableAttribute))]
	public class ReorderableDrawer : PropertyDrawer 
	{
		private static Dictionary<int, ReorderableList> lists = new Dictionary<int, ReorderableList>();

		public override bool CanCacheInspectorGUI(SerializedProperty property) {
			return false;
		}
		
		public override float GetPropertyHeight(SerializedProperty property, GUIContent label) {

			Pair<bool, ReorderableList> list = GetList(property, attribute as ReorderableAttribute);

			return list.Second?.GetHeight() ?? EditorGUIUtility.singleLineHeight;
		}

		public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {

			Pair<bool, ReorderableList> list = GetList(property, attribute as ReorderableAttribute);

			if (list.Second != null) 
			{
				list.Second.DoList(EditorGUI.IndentedRect(position), label);
			}
			else 
			{
				GUI.Label(position, "Array must extend from ReorderableArray", EditorStyles.label);
			}
		}

		public static int GetListId(SerializedProperty property) {

			if (property != null) {

				int h1 = property.serializedObject.targetObject.GetHashCode();
				int h2 = property.propertyPath.GetHashCode();

				return (((h1 << 5) + h1) ^ h2);
			}

			return 0;
		}

		public static Pair<bool, ReorderableList> GetList(SerializedProperty property) {

			return GetList(property, null, GetListId(property));
		}

		public static Pair<bool, ReorderableList> GetList(SerializedProperty property, ReorderableAttribute attrib) {

			return GetList(property, attrib, GetListId(property));
		}

		public static Pair<bool, ReorderableList> GetList(SerializedProperty property, int id) {

			return GetList(property, null, id);
		}

		public static Pair<bool, ReorderableList> GetList(SerializedProperty property, ReorderableAttribute attrib, int id)
		{
			if (property == null)
			{
				return new Pair<bool, ReorderableList>(false, null);
			}

			ReorderableList list = null;
			SerializedProperty array = property.isArray ? property : property.FindPropertyRelative("array");

			bool justCreated = false;
			if (array != null && array.isArray)
			{
				if (!lists.TryGetValue(id, out list))
				{
					if (attrib != null)
					{
						Texture icon = !string.IsNullOrEmpty(attrib.elementIconPath) ? AssetDatabase.GetCachedIcon(attrib.elementIconPath) : null;

						ReorderableList.ElementDisplayType displayType = attrib.singleLine ? ReorderableList.ElementDisplayType.SingleLine : ReorderableList.ElementDisplayType.Auto;

						list = new ReorderableList(array, attrib.add, attrib.remove, attrib.draggable, displayType, attrib.elementNameProperty, attrib.elementNameOverride, icon);
						list.paginate = attrib.paginate;
						list.pageSize = attrib.pageSize;
					}
					else
					{
						list = new ReorderableList(array, true, true, true);
					}

					justCreated = true;
					lists.Add(id, list);
				}
				else
				{
					list.List = array;
				}
			}

			return new Pair<bool, ReorderableList>(justCreated, list);
		}
	}
}