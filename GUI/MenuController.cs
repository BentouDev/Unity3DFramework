using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : GUIBase
{
    [Header("Debug")] 
    public bool Verbose;

    public enum InitMode
    {
        Code,
        OnStart,
        OnLevelLoaded
    }

    public enum InitState
    {
        Visible,
        Hidden
    }
    
    [Header("Misc")]
    public InitMode Autostart;
    public InitState InitVisibility;
    public string BackButton = "Back";
    
    [Header("Main Menu References")]
    public MenuBase TitleMenu;
    public MenuBase MainMenu;

    [Header("Audio")]
    public AudioMixer GameplayMixer;

    public string GameplayVolume;

    public AudioSource UIConfirm;
    public AudioSource UICancel;

    public enum AlphaMode
    {
        Animate,
        Snap,
        Custom,
    }

    [System.Serializable]
    public struct AnimInfo
    {
        [SerializeField] public bool DoPlay;
        [SerializeField] public AlphaMode AlphaMode;
        [SerializeField] public AnimationPlayer Anim;

        public float Duration => DoPlay ? Anim.Duration : 0;

        public void Play()
        {
            if (DoPlay)
                Anim.Play();
            else
                Anim.OnStart.Invoke();
        }
    }

    [System.Serializable]
    public struct AnimPair
    {
        [SerializeField] public AnimInfo Show;
        [SerializeField] public AnimInfo Hide;
    }

    [Header("Animation")]
    [SerializeField] public AnimPair Master;
    
    [SerializeField] public AnimPair Local;

    [Space]
    
    public Button.ButtonClickedEvent OnStartingBack;

    private List<MenuBase> allMenus = new List<MenuBase>();
    public IList<MenuBase> AllMenus => allMenus;
    
    private bool                 SkipLocalAnim;
    private bool                 IsDuringSwitch;
    public  MenuBase             CurrentMenu { get; private set; }
    private MenuBase             PreviousMenu;
    private Tuple<MenuBase,bool> NextMenu;

    public override bool IsGameplayGUI { get { return false; } }

    public bool IsDuringShowing { get; private set; }
    public bool IsDuringHiding { get; private set; }

    protected IEnumerator CoHandleAlphaAnim(float from, float to, float duration, CanvasGroup canvas)
    {
        float elapsed  = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
    
            var ratio = Mathf.Lerp(from, to, elapsed / duration);
            canvas.alpha = ratio;
    
            if (GameplayMixer)
                GameplayMixer.SetFloat(GameplayVolume, SettingsManager.LinearToDecibel(1 - ratio));
    
            yield return null;
        }
        
        canvas.alpha = 0;
    }

    protected IEnumerator DoHideAnim()
    {
        IsDuringHiding = true;
        CanvasGroup.interactable = false;

        float duration = Master.Hide.Duration;

        Master.Hide.Play();
        switch (Master.Hide.AlphaMode)
        {
            case AlphaMode.Custom:
                yield return new WaitForSecondsRealtime(duration);
                break;
            case AlphaMode.Animate:
            {
                yield return StartCoroutine(CoHandleAlphaAnim(1, 0, duration, CanvasGroup));
                CanvasGroup.alpha = 0;
                break;
            }
            case AlphaMode.Snap:
            {
                yield return new WaitForSecondsRealtime(duration);
                CanvasGroup.alpha = 0;
                break;
            }
        }

        if (CurrentMenu)
        {
            CurrentMenu.Hide();
            CurrentMenu.gameObject.SetActive(false);
        }
        
        IsDuringHiding = false;

        CanvasGroup.alpha = 0;

        SkipLocalAnim = true;
        SwitchToMenuImmediate(null, false);
    }

    protected IEnumerator DoShowAnim(MenuBase menuBase = null)
    {
        var menu = menuBase ?? TitleMenu ?? MainMenu;
        IsDuringShowing = true;

        if (!menu)
        {
            Debug.LogError("Unable to pick menu to show!");
        }
        else
        {
            float elapsed  = 0;
            float duration = Master.Show.Duration;
        
            menu.Show();
            menu.gameObject.SetActive(true);

            Master.Show.Play();
            switch (Master.Show.AlphaMode)
            {
                case AlphaMode.Custom:
                    yield return new WaitForSecondsRealtime(duration);
                    break;
                case AlphaMode.Animate:
                {
                    yield return StartCoroutine(CoHandleAlphaAnim(0, 1, duration, CanvasGroup));
                    CanvasGroup.alpha = 1;
                    break;
                }
                case AlphaMode.Snap:
                {
                    CanvasGroup.alpha = 1;
                    yield return new WaitForSecondsRealtime(duration);
                    break;
                }
            }            
        }
 
        IsDuringShowing = false;
        
        CanvasGroup.interactable = true;

        SkipLocalAnim = true;
        SwitchToMenu(menu, false);
    }

    public void AnimShow(MenuBase menu)
    {
        if (IsDuringShowing || IsDuringHiding)
            return;

        StartCoroutine(DoShowAnim(menu));        
    }

    public void AnimShow()
    {
        if (IsDuringShowing || IsDuringHiding)
            return;

        StartCoroutine(DoShowAnim());
    }

    public void AnimHide()
    {
        if (IsDuringShowing || IsDuringHiding)
            return;

        StartCoroutine(DoHideAnim());
    }

    public override void Show()
    {
        base.Show();
        SwitchToMenu(TitleMenu ?? MainMenu, false);
    }

    public override void Hide()
    {
        base.Hide();
        SwitchToMenu(null, false);
    }

    public void OnLevelCleanUp()
    {
        Hide();

        foreach (MenuBase menu in allMenus)
        {
            menu.Hide();
        }

        allMenus.Clear();
    }

    public void Init()
    {
        if (allMenus != null)
            allMenus.Clear();
        else
            allMenus = new List<MenuBase>();

        allMenus.AddRange(GetComponentsInChildren<MenuBase>());

        foreach (MenuBase menu in allMenus)
        {
            menu.Init(this);
            menu.Hide();
        }

        if (InitVisibility == InitState.Visible)
        {
            StartCoroutine(DoShowAnim());
        }
        else
        {
            Hide();
        }
    }

    public void OnLevelLoaded()
    {
        if (Autostart == InitMode.OnLevelLoaded)
            Init();
    }

    void Start()
    {
        if (Autostart == InitMode.OnStart)
        {
            Init();
        }
    }

    public void GoBack(MenuBase menu)
    {
        if (menu == null)
            return;

        if (menu != CurrentMenu || CurrentMenu == TitleMenu)
            return;

        if (menu == MainMenu || menu.AlwaysBack)
        {
            if (OnStartingBack != null)
                OnStartingBack.Invoke();
            
            return;
        }

        if (UICancel)
            UICancel.Play();

        if (PreviousMenu == TitleMenu)
            return;

        SwitchToMenu(PreviousMenu, false);
    }

    IEnumerator CoSwitchAnimWorker(MenuBase baseMenu)
    {
        IsDuringSwitch = true;
        
        // override local anim
        if (PreviousMenu && PreviousMenu.IsHideAnim)
        {
            PreviousMenu.OnHide.Play();
            yield return new WaitForSeconds(PreviousMenu.OnHide.Duration);
        }
        else if (Local.Hide.DoPlay)
        {
            Local.Hide.Play();
            switch (Local.Hide.AlphaMode)
            {
                case AlphaMode.Custom:
                    yield return new WaitForSeconds(Local.Hide.Duration);
                    break;
                case AlphaMode.Snap:
                {
                    yield return new WaitForSeconds(Local.Hide.Duration);
                    if (PreviousMenu) PreviousMenu.Hide();
                    
                    break;
                }
                case AlphaMode.Animate:
                {
                    if (CurrentMenu)
                    {
                        yield return StartCoroutine(CoHandleAlphaAnim(1, 0, Local.Hide.Duration, PreviousMenu.Canvas));
                        PreviousMenu.Hide();
                    }
                    else
                    {
                        yield return new WaitForSeconds(Local.Hide.Duration);
                    }
                    
                    break;
                }
            }
        }
        else
        {
            if (PreviousMenu) PreviousMenu.Hide();
        }
        
        if (PreviousMenu) PreviousMenu.End();
        
        CurrentMenu = baseMenu;

        if (CurrentMenu) CurrentMenu.gameObject.SetActive(true);

        if (CurrentMenu && CurrentMenu.IsShowAnim)
        {
            CurrentMenu.OnShow.Play();
            yield return new WaitForSeconds(CurrentMenu.OnShow.Duration);
        }
        else if (Local.Show.DoPlay)
        {
            Local.Show.Play();
            switch (Local.Show.AlphaMode)
            {
                case AlphaMode.Custom:
                    yield return new WaitForSeconds(Local.Show.Duration);
                    break;
                case AlphaMode.Snap:
                {
                    yield return new WaitForSeconds(Local.Show.Duration);
                    if (CurrentMenu) CurrentMenu.Show();
                    
                    break;
                }
                case AlphaMode.Animate:
                {
                    if (CurrentMenu)
                    {
                        yield return StartCoroutine(CoHandleAlphaAnim(1, 0, Local.Show.Duration, CurrentMenu.Canvas));
                        CurrentMenu.Show();
                    }
                    else
                    {
                        yield return new WaitForSeconds(Local.Show.Duration);
                    }
                    
                    break;
                }
            }            
        }
        else
        {
            if (CurrentMenu) CurrentMenu.Show();
        }
        
        if (CurrentMenu) CurrentMenu.Begin();
        
        IsDuringSwitch = false;
    }

    public void SwitchToMenuImmediate(MenuBase baseMenu, bool playEffect = true)
    {
        if (playEffect && UIConfirm)
            UIConfirm.Play();

        PreviousMenu = CurrentMenu;

        if (SkipLocalAnim)
        {
            SkipLocalAnim = false;
            
            if (PreviousMenu) PreviousMenu.End();
            CurrentMenu = baseMenu;
            if (CurrentMenu) CurrentMenu.Begin();
        }
        else
        {
            StartCoroutine(CoSwitchAnimWorker(baseMenu));            
        }
    }

    public void SwitchToMenu(MenuBase baseMenu, bool playEffect = true)
    {
        if (NextMenu != null && NextMenu.Item1 != null )
        {
            Debug.LogWarningFormat
            (
                "Changed menu twice in this frame! From '{0}' to '{1}' and now to '{2}'", 
                CurrentMenu, NextMenu.Item1, baseMenu
            );
        }

        if (Verbose)
        {
            Debug.LogFormat("{0}: Switch from '{1}' to '{2}'", this, CurrentMenu, baseMenu);
        }
        
        NextMenu = new Tuple<MenuBase, bool>(baseMenu, playEffect);
    }

    void Update()
    {
        if (IsDuringShowing || IsDuringHiding)
        {
            Master.Show.Anim.Update();
            Master.Hide.Anim.Update();
            
            return;
        }

        if (IsDuringSwitch)
        {
            Local.Show.Anim.Update();
            Local.Hide.Anim.Update();
            
            return;
        }

        if (NextMenu != null && NextMenu.Item1 != null)
        {
            var nextMenu = NextMenu;
            
            NextMenu = null;
            
            SwitchToMenuImmediate(nextMenu.Item1, nextMenu.Item2);
        }

        if (EventSystem.current
        &&  EventSystem.current.currentSelectedGameObject == null
        &&  CurrentMenu && CurrentMenu.FirstToSelect)
        {
            CurrentMenu.FirstToSelect.Select();
            CurrentMenu.FirstToSelect.OnSelect(new BaseEventData(EventSystem.current));
        }

        if (Input.GetButtonDown(BackButton))
        {
            GoBack(CurrentMenu);
        }
    }
}
