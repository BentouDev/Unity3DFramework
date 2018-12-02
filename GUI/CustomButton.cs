using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[AddComponentMenu("UI/Custom Button", 30)]
public class CustomButton : Selectable, IPointerClickHandler, ISubmitHandler, IEventSystemHandler
{
	public bool OverrideNavigation;
	
	[System.Serializable]
	public class NavigateEvent : UnityEvent<AxisEventData>
	{ }

	[SerializeField] 
	public NavigateEvent OnNavigate;
	
	[SerializeField]
	public UnityEvent OnPress;
	
	[SerializeField]
	public UnityEvent OnSelected;
	
	public override void OnSelect(BaseEventData eventData)
	{
		OnSelected.Invoke();
	}

	public void Press()
	{
		OnPress?.Invoke();
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Press();
	}

	public void OnSubmit(BaseEventData eventData)
	{
		Press();
	}

	public void DoMove(AxisEventData eventData)
	{
		GameObject target = GetNavigationTarget(eventData.moveDir);
		if (target)
			eventData.selectedObject = target;
	}

	public GameObject GetNavigationTarget(MoveDirection moveDir)
	{
		Selectable sel = null;
		switch (moveDir)
		{
			case MoveDirection.Left:
				sel = FindSelectableOnLeft();
				break;
			case MoveDirection.Up:
				sel = FindSelectableOnUp();
				break;
			case MoveDirection.Right:
				sel = FindSelectableOnRight();
				break;
			case MoveDirection.Down:
				sel = FindSelectableOnDown();
				break;
		}
		
		if (!((UnityEngine.Object) sel != (UnityEngine.Object) null))
			return FindRecurseLeft(this);
		
		if (!sel.IsActive())
			return FindRecurseLeft(sel);
		
		return sel.gameObject;
	}

	protected GameObject FindRecurseLeft(Selectable selectable)
	{
		var left = selectable.FindSelectableOnLeft();
		if ((UnityEngine.Object) left != (UnityEngine.Object) null)
		{
			if (!left.IsActive())
				return FindRecurseLeft(left);
			else
				return left.gameObject;
		}

		return null;
	}
	
	public override void OnMove(AxisEventData eventData)
	{
		if (OverrideNavigation)
			OnNavigate.Invoke(eventData);
		else
		{
			DoMove(eventData);
		}
	}
}
