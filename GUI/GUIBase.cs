using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CanvasGroup))]
public abstract class GUIBase : MonoBehaviour, ILevelDependable
{
    private CanvasGroup _canvasGroup;

    public virtual bool IsGameplayGUI { get { return true; } }

    protected CanvasGroup CanvasGroup
    {
        get
        {
            if (!_canvasGroup)
                _canvasGroup = GetComponent<CanvasGroup>();
            return _canvasGroup;
        }
    }

    public virtual void Hide()
    {
        CanvasGroup.alpha = 0;
        CanvasGroup.interactable = false;
    }

    public virtual void Show()
    {
        CanvasGroup.alpha = 1;
        CanvasGroup.interactable = true;
    }

    public abstract void OnLevelLoaded();

    public virtual void OnAwake()
    {
        
    }

    public abstract void OnLevelCleanUp();
}
