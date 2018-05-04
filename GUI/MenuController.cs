using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : GUIBase
{
    [Header("Misc")]
    public bool Autostart = true;
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

    private MenuBase             CurrentMenu;
    private MenuBase             PreviousMenu;
    private Tuple<MenuBase,bool> NextMenu;

    public override bool IsGameplayGUI { get { return false; } }

    public bool IsDuringShowing { get; private set; }
    public bool IsDuringHiding { get; private set; }

    protected IEnumerator DoHideAnim()
    {
        IsDuringHiding = true;
        CanvasGroup.interactable = false;

        float elapsed  = 0;
        float duration = Master.Hide.Duration;

        Master.Hide.Play();
        switch (Master.Hide.AlphaMode)
        {
            case AlphaMode.Custom:
                yield return new WaitForSecondsRealtime(duration);
                break;
            case AlphaMode.Animate:
            {
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
    
                    var ratio = Mathf.Lerp(1, 0, elapsed / duration);
                    CanvasGroup.alpha = ratio;
    
                    if (GameplayMixer)
                        GameplayMixer.SetFloat(GameplayVolume, SettingsManager.LinearToDecibel(1 - ratio));
    
                    yield return null;
                }
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

        SwitchToMenu(null, false);
    }

    protected IEnumerator DoShowAnim()
    {
        var menu = TitleMenu ?? MainMenu;
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
                    while (elapsed < duration)
                    {
                        elapsed += Time.unscaledDeltaTime;
    
                        var ratio = Mathf.Lerp(0, 1, elapsed / duration);
                        CanvasGroup.alpha = ratio;
    
                        if (GameplayMixer)
                            GameplayMixer.SetFloat(GameplayVolume, SettingsManager.LinearToDecibel(1 - ratio));
    
                        yield return null;
                    }
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

        SwitchToMenu(menu, false);
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

    public void OnLevelLoaded()
    {
        var menus = GetComponentsInChildren<MenuBase>();
        foreach (MenuBase menu in menus)
        {
            menu.Init(this);
            menu.Hide();
            allMenus.Add(menu);
        }

        Hide();
    }

    void Start()
    {
        if (Autostart)
        {
            var menus = FindObjectsOfType<MenuBase>();
            foreach (MenuBase menu in menus)
            {
                menu.Init(this);
                menu.Hide();
                allMenus.Add(menu);
            }

            StartCoroutine(DoShowAnim());
        }
    }

    public void GoBack(MenuBase menu)
    {
        if (menu == null)
            return;

        if (menu != CurrentMenu || CurrentMenu == TitleMenu)
            return;

        if (menu == MainMenu)
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

    public void SwitchToMenuImmediate(MenuBase baseMenu, bool playEffect = true)
    {
        if (playEffect && UIConfirm)
            UIConfirm.Play();

        PreviousMenu = CurrentMenu;

        if (PreviousMenu) PreviousMenu.End();
        CurrentMenu = baseMenu;
        if (CurrentMenu) CurrentMenu.Begin();
    }

    public void SwitchToMenu(MenuBase baseMenu, bool playEffect = true)
    {
        if (NextMenu != null && NextMenu.Item1 != null )
        {
            Debug.LogWarningFormat(
                "Changed menu twice in this frame! From '{0}' to '{1}' and now to '{2}'", 
                CurrentMenu, NextMenu.Item1, baseMenu
            );
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
