using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
public class QualitySetter : Selectable, ICanvasElement
{
    [Space]
    [SerializeField]
    private Text _title;
    public Text Title { get { return _title; } set { _title = value; Set(QualitySettings.GetQualityLevel()); } }

    [SerializeField]
    public EnumAxis Axis;
   
    public enum EnumAxis
    {
        Horizontal = 0,
        Vertical = 1
    }

    [System.Serializable]
    public class QualityEvent : UnityEvent<int> { }

    [Space]

    [SerializeField]
    private QualityEvent m_OnValueChanged = new QualityEvent();
    public QualityEvent onValueChanged { get { return m_OnValueChanged; } set { m_OnValueChanged = value; } }

#if UNITY_EDITOR
    protected override void OnValidate()
    {
        base.OnValidate();
        
        //Onvalidate is called before OnEnabled. We need to make sure not to touch any other objects before OnEnable is run.
        if (IsActive())
        {
            Title = _title;
            Set(QualitySettings.GetQualityLevel());
        }

        var prefabType = UnityEditor.PrefabUtility.GetPrefabType(this);
        if (prefabType != UnityEditor.PrefabType.Prefab && !Application.isPlaying)
            CanvasUpdateRegistry.RegisterCanvasElementForLayoutRebuild(this);
    }
#endif // if UNITY_EDITOR

    protected override void OnEnable()
    {
        base.OnEnable();
        Title = _title;
        Set(QualitySettings.GetQualityLevel());
    }

    private void Set(int level)
    {
        if (!_title)
            return;

        var count = QualitySettings.names.Length;

        if (level >= count)
            return;
        if (level < 0)
            return;

        Title.text = QualitySettings.names[level];
        m_OnValueChanged.Invoke(level);
    }

    private void IncLevel()
    {
        QualitySettings.IncreaseLevel();
        Set(QualitySettings.GetQualityLevel());
    }

    private void DecLevel()
    {
        QualitySettings.DecreaseLevel();
        Set(QualitySettings.GetQualityLevel());
    }

    public override void OnMove(AxisEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
        {
            base.OnMove(eventData);
            return;
        }

        switch (eventData.moveDir)
        {
            case MoveDirection.Left:
                if (Axis == EnumAxis.Horizontal && FindSelectableOnLeft() == null)
                    DecLevel();
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Right:
                if (Axis == EnumAxis.Horizontal && FindSelectableOnRight() == null)
                    IncLevel();
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Up:
                if (Axis == EnumAxis.Vertical && FindSelectableOnUp() == null)
                    IncLevel();
                else
                    base.OnMove(eventData);
                break;
            case MoveDirection.Down:
                if (Axis == EnumAxis.Vertical && FindSelectableOnDown() == null)
                    DecLevel();
                else
                    base.OnMove(eventData);
                break;
        }
    }

    public override Selectable FindSelectableOnLeft()
    {
        if (navigation.mode == Navigation.Mode.Automatic && Axis == EnumAxis.Horizontal)
            return null;
        return base.FindSelectableOnLeft();
    }

    public override Selectable FindSelectableOnRight()
    {
        if (navigation.mode == Navigation.Mode.Automatic && Axis == EnumAxis.Horizontal)
            return null;
        return base.FindSelectableOnRight();
    }

    public override Selectable FindSelectableOnUp()
    {
        if (navigation.mode == Navigation.Mode.Automatic && Axis == EnumAxis.Vertical)
            return null;
        return base.FindSelectableOnUp();
    }

    public override Selectable FindSelectableOnDown()
    {
        if (navigation.mode == Navigation.Mode.Automatic && Axis == EnumAxis.Vertical)
            return null;
        return base.FindSelectableOnDown();
    }

    public virtual void OnInitializePotentialDrag(PointerEventData eventData)
    {
        eventData.useDragThreshold = false;
    }

    public virtual void Rebuild(CanvasUpdate executing)
    {
        if (executing == CanvasUpdate.Prelayout)
            onValueChanged.Invoke(QualitySettings.GetQualityLevel());
    }

    public void LayoutComplete()
    {

    }

    public void GraphicUpdateComplete()
    {

    }
}

