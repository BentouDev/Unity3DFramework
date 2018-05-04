using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public abstract class MenuBase : GUIBase
{
    public Selectable FirstToSelect;
    
    public MenuController Controller { get; set; }
    
    protected CanvasGroup Canvas;

    public override bool IsGameplayGUI { get { return false; } }
    
    [Header("Show")]
    public bool IsShowAnim;
    
    [SerializeField]
    public AnimationPlayer OnShow;
    
    [Header("Hide")]
    public bool IsHideAnim;
    
    [SerializeField]
    public AnimationPlayer OnHide;

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

    public void End()
    {
        Canvas.interactable = false;

        if (!IsHideAnim)
        {
            Canvas.alpha = 0;
            gameObject.SetActive(false);
            
            OnHide.OnStart.Invoke();
        }
        else
        {
            OnHide.Play();
        }

        OnEnd();
    }
    
    protected virtual void OnEnd()
    { }

    public void Begin()
    {
        gameObject.SetActive(true);

        Canvas.interactable = true;

        if (!IsShowAnim)
        {
            Canvas.alpha = 1;
            OnShow.OnStart.Invoke();
        }
        else
        {            
            OnShow.Play();
        }
        
        if (FirstToSelect)
            StartCoroutine(SelectFirst(FirstToSelect));
        
        OnBegin();
    }

    protected virtual void OnBegin()
    { }

    IEnumerator SelectFirst(Selectable select)
    {
        yield return new WaitForEndOfFrame();

        EventSystem.current.SetSelectedGameObject(null);
        FirstToSelect.Select();
    }
}
