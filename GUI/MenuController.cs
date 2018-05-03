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

    [Header("Animation")]
    public float AnimDuration;
    
    [Header("Main Menu References")]
    public MenuBase TitleMenu;
    public MenuBase MainMenu;

    [Header("Audio")]
    public AudioMixer GameplayMixer;

    public string GameplayVolume;

    public AudioSource UIConfirm;
    public AudioSource UICancel;

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

        float elapsed = 0;

        while (elapsed < AnimDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            var ratio = Mathf.Lerp(1, 0, elapsed / AnimDuration);
            CanvasGroup.alpha = ratio;

            if (GameplayMixer)
                GameplayMixer.SetFloat(GameplayVolume, SettingsManager.LinearToDecibel(1 - ratio));

            yield return null;
        }

        IsDuringHiding = false;

        CanvasGroup.alpha = 0;

        SwitchToMenu(null, false);
    }

    protected IEnumerator DoShowAnim()
    {
        var menu = TitleMenu ?? MainMenu;
        IsDuringShowing = true;

        float elapsed = 0;

        if (!menu)
        {
            Debug.LogError("Unable to pick menu to show!");
        }
        else
        {
            menu.Show();
            menu.gameObject.SetActive(true);

            while (elapsed < AnimDuration)
            {
                elapsed += Time.unscaledDeltaTime;

                var ratio = Mathf.Lerp(0, 1, elapsed / AnimDuration);
                CanvasGroup.alpha = ratio;

                if (GameplayMixer)
                    GameplayMixer.SetFloat(GameplayVolume, SettingsManager.LinearToDecibel(1 - ratio));

                yield return null;
            }            
        }
        
        IsDuringShowing = false;

        CanvasGroup.alpha = 1;
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
            return;

        if (NextMenu != null && NextMenu.Item1 != null)
        {
            var nextMenu = NextMenu;
            
            NextMenu = null;
            
            SwitchToMenuImmediate(nextMenu.Item1, nextMenu.Item2);
        }

        if (EventSystem.current && EventSystem.current.currentSelectedGameObject == null && CurrentMenu && CurrentMenu.FirstToSelect)
        {
            CurrentMenu.FirstToSelect.Select();
        }

        if (Input.GetButtonDown(BackButton))
        {
            GoBack(CurrentMenu);
        }
    }
}
