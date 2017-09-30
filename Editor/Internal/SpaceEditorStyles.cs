#if UNITY_EDITOR
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;

public static class SpaceEditorStyles
{
    private static GUISkin _skin;
    public static GUISkin Skin
    {
        get
        {
            if (_skin == null)
                _skin = AssetDatabase.LoadAssetAtPath<GUISkin>("Assets/Framework/Gizmos/SpaceEditorGUI.guiskin");
            return _skin;
        }
    }

    private static GUIStyle _lockButton;
    public static GUIStyle LockButton
    {
        get
        {
            if (_lockButton == null)
            {
                _lockButton = (GUIStyle)"IN LockButton";
                _lockButton.padding = EditorStyles.toggle.padding;
                _lockButton.margin  = EditorStyles.toggle.margin;
            }

            return _lockButton;
        }
    }
    
    private static GUIStyle _lightObjectField;
    public static GUIStyle LightObjectField
    {
        get
        {
            if (_lightObjectField == null)
            {
                _lightObjectField = (GUIStyle) "ShurikenObjectField";
            }

            return _lightObjectField;
        }
    }
    
    private static GUIStyle _listItem;
    public static GUIStyle ListItem
    {
        get
        {
            if (_listItem == null)
            {
                _listItem = new GUIStyle(EditorStyles.label);

                _listItem.margin = new RectOffset();

                _listItem.onActive.textColor = _listItem.onNormal.textColor;
                _listItem.active.textColor = _listItem.normal.textColor;
                _listItem.onActive.background = ActiveBackground;
                _listItem.active.background = ActiveBackground;

                _listItem.onFocused.background = ActiveBackground;
                _listItem.focused.background = ActiveBackground;
            }

            return _listItem;
        }
    }

    private static GUIStyle _selectorRectangle;

    public static GUIStyle SelectorRectangle
    {
        get
        {
            if (_selectorRectangle == null)
            {
                _selectorRectangle = (GUIStyle) "RectangleToolSelection";
            }

            return _selectorRectangle;
        }
    }

    private static GUIStyle _selectedListItem;
    public static GUIStyle SelectedListItem
    {
        get
        {
            if (_selectedListItem == null)
            {
                _selectedListItem = new GUIStyle(EditorStyles.label);

                _selectedListItem.margin = new RectOffset();

                _selectedListItem.onNormal.background = ActiveBackground;
                _selectedListItem.normal.background = ActiveBackground;

                _selectedListItem.onActive.textColor = _selectedListItem.onNormal.textColor;
                _selectedListItem.active.textColor = _selectedListItem.normal.textColor;
                _selectedListItem.onActive.background = ActiveBackground;
                _selectedListItem.active.background = ActiveBackground;

                _selectedListItem.onFocused.background = ActiveBackground;
                _selectedListItem.focused.background = ActiveBackground;
            }

            return _selectedListItem;
        }
    }

    private static GUIStyle _editableLabel;
    public static GUIStyle EditableLabel
    {
        get
        {
            if (_editableLabel == null)
            {
                _editableLabel = new GUIStyle(EditorStyles.textField);
                _editableLabel.normal = EditorStyles.label.normal;
                _editableLabel.onNormal = EditorStyles.label.onNormal;
            }

            return _editableLabel;
        }
    }

    private static GUIStyle _countBadge;
    public static GUIStyle CountBadge
    {
        get
        {
            if (_countBadge == null)
                _countBadge = (GUIStyle)"CN CountBadge";
            return _countBadge;
        }
    }

    private static GUIStyle _lightBackground;
    public static GUIStyle LightBackground
    {
        get
        {
            if (_lightBackground == null)
                _lightBackground = (GUIStyle)"ProjectBrowserPreviewBg";
            return _lightBackground;
        }
    }

    private static GUIStyle _darkBackground;
    public static GUIStyle DarkBackground
    {
        get
        {
            if (_darkBackground == null)
                _darkBackground = (GUIStyle)"ObjectPickerPreviewBackground";
            return _darkBackground;
        }
    }

    private static GUIStyle _largebuttonRight;
    public static GUIStyle LargeButtonRight
    {
        get
        {
            if (_largebuttonRight == null)
                _largebuttonRight = (GUIStyle)"LargeButtonRight";
            return _largebuttonRight;
        }
    }

    private static GUIStyle _largebuttonMid;
    public static GUIStyle LargeButtonMid
    {
        get
        {
            if (_largebuttonMid == null)
                _largebuttonMid = (GUIStyle)"LargeButtonMid";
            return _largebuttonMid;
        }
    }

    private static GUIStyle _largeButtonLeft;
    public static GUIStyle LargeButtonLeft
    {
        get
        {
            if (_largeButtonLeft == null)
                _largeButtonLeft = (GUIStyle)"LargeButtonLeft";
            return _largeButtonLeft;
        }
    }

    private static GUIStyle _buttonRight;
    public static GUIStyle ButtonRight
    {
        get
        {
            if (_buttonRight == null)
                _buttonRight = (GUIStyle)"ButtonRight";
            return _buttonRight;
        }
    }

    private static GUIStyle _buttonMid;
    public static GUIStyle ButtonMid
    {
        get
        {
            if (_buttonMid == null)
                _buttonMid = (GUIStyle)"ButtonMid";
            return _buttonMid;
        }
    }

    private static GUIStyle _buttonLeft;
    public static GUIStyle ButtonLeft
    {
        get
        {
            if (_buttonLeft == null)
                _buttonLeft = (GUIStyle) "ButtonLeft";
            return _buttonLeft;
        }
    }

    private static GUIStyle _timeLineThumbLine;
    public static GUIStyle TimeLineThumbLine
    {
        get
        {
            if (_timeLineThumbLine == null)
                _timeLineThumbLine = Skin.customStyles.First(s => s.name == "TimelineThumbLine");;
            return _timeLineThumbLine;
        }
    }

    private static GUIStyle _timeLineThumb;
    public static GUIStyle TimeLineThumb
    {
        get
        {
            if (_timeLineThumb == null)
                _timeLineThumb = Skin.customStyles.First(s => s.name == "TimelineThumb");// (GUIStyle)"MeBlendPosition";
            return _timeLineThumb;
        }
    }

    private static GUIStyle _timeLineBackground;
    public static GUIStyle TimeLineBackground
    {
        get
        {
            if (_timeLineBackground == null)
                _timeLineBackground = (GUIStyle)"ShurikenEffectBg";
            return _timeLineBackground;
        }
    }

    private static GUIStyle _bar;
    public static GUIStyle Bar
    {
        get
        {
            if (_bar == null)
                _bar = Skin.customStyles.First(s => s.name == "Bar");
            return _bar;
        }
    }

    private static GUIStyle _frameRangeThumb;
    public static GUIStyle FrameRangeThumb
    {
        get
        {
            if (_frameRangeThumb == null)
                _frameRangeThumb = Skin.customStyles.First(s => s.name == "TimelineBar");
            return _frameRangeThumb;
        }
    }

    private static GUIStyle _timeRangeThumb;
    public static GUIStyle TimeRangeThumb
    {
        get
        {
            if (_timeRangeThumb == null)
                _timeRangeThumb = Skin.customStyles.First(s => s.name == "TimeRangeThumb");
            return _timeRangeThumb;
        }
    }

    private static GUIStyle _timeRangeBackground;
    public static GUIStyle TimeRangeBackground
    {
        get
        {
            if(_timeRangeBackground == null)
                _timeRangeBackground = (GUIStyle)"ShurikenEffectBg";
            return _timeRangeBackground;
        }
    }
    
    private static GUIStyle _graphNodeEditorBackground;
    public static GUIStyle GraphNodeEditorBackground
    {
        get
        {
            if (_graphNodeEditorBackground == null)
                _graphNodeEditorBackground = (GUIStyle)"flow background";
            return _graphNodeEditorBackground;
        }
    }
    
    private static GUIStyle _graphNodeBackground;
    public static GUIStyle GraphNodeBackground
    {
        get
        {
            if (_graphNodeBackground == null)
                _graphNodeBackground = Skin.customStyles.First(s => s.name == "GraphNode");
            return _graphNodeBackground;
        }
    }

    private static GUIStyle _dotFlowTarget;
    public static GUIStyle DotFlowTarget
    {
        get
        {
            if (_dotFlowTarget == null)
                _dotFlowTarget = Skin.customStyles.First(s => s.name == "DotFlowTarget");
            return _dotFlowTarget;
        }
    }

    private static GUIStyle _dotFlowTargetFill;
    public static GUIStyle DotFlowTargetFill
    {
        get
        {
            if (_dotFlowTargetFill == null)
                _dotFlowTargetFill = Skin.customStyles.First(s => s.name == "DotFlowTargetFill");
            return _dotFlowTargetFill;
        }
    }

    private static Texture2D _behaviourTreeIcon;
    public static Texture2D BehaviourTreeIcon
    {
        get
        {
            if (_behaviourTreeIcon == null)
            {
                if (EditorGUIUtility.isProSkin)
                    _behaviourTreeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Framework/Gizmos/d_BehaviourTreeEditorIcon.png");
                else
                    _behaviourTreeIcon = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Framework/Gizmos/BehaviourTreeEditorIcon.png");
            }
            return _behaviourTreeIcon;
        }
    }

    private static Texture2D _activeBackground;
    public static Texture2D ActiveBackground
    {
        get
        {
            if (_activeBackground == null)
            {
                var color = EditorGUIUtility.isProSkin ? new Color(61 / 255.0f, 96 / 255.0f, 145 / 255.0f) : new Color(0.33f, 0.66f, 1f, 0.66f);
                _activeBackground = new Texture2D(1, 1);
                _activeBackground.SetPixel(0, 0, color);
                _activeBackground.Apply();
            }
            return _activeBackground;
        }
    }
}
#endif
