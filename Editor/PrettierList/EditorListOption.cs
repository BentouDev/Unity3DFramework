#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[System.Flags]
public enum EditorListOption
{
	None			= 0,
	ListSize		= 1,
	ListLabel		= 2,
	ElementLabels	= 4,
	ForceResize		= 8,
	Buttons			= 16,
	AddButton		= 32,
	Default			= ListSize	| ListLabel | ElementLabels,
	NoElementLabels	= ListSize	| ListLabel,
	All				= Default	| Buttons | ForceResize
}
#endif