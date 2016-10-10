using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public abstract class MenuBase : GUIBase
{
    public Selectable FirstToSelect;
    protected MenuController Controller;
    protected CanvasGroup Canvas;

    public override bool IsGameplayGUI { get { return false; } }

    public override void Hide()
    {
        base.Hide();
        gameObject.SetActive(false);
    }

    public void Init(MenuController menuController)
    {
        Controller = menuController;
        Canvas = GetComponent<CanvasGroup>();

        Canvas.interactable = false;
        Canvas.alpha = 0;
    }

    public void SwitchTo()
    {
        Controller.SwitchToMenu(this);
    }

    public virtual void OnEnd()
    {
        Canvas.interactable = false;
        Canvas.alpha = 0;

        gameObject.SetActive(false);
    }

    public virtual void OnStart()
    {
        gameObject.SetActive(true);

        Canvas.interactable = true;
        Canvas.alpha = 1;

        if (FirstToSelect)
            StartCoroutine(SelectFirst(FirstToSelect));
    }

    IEnumerator SelectFirst(Selectable select)
    {
        yield return new WaitForEndOfFrame();

        EventSystem.current.SetSelectedGameObject(null);
        FirstToSelect.Select();
    }
}
