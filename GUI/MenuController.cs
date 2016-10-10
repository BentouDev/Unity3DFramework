using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Audio;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MenuController : GUIBase
{
    public bool Autostart = true;

    public float AnimDuration;
    
    public MenuBase TitleMenu;
    public MenuBase MainMenu;

    public AudioMixer GameplayMixer;

    public string GameplayVolume;

    public AudioSource UIConfirm;
    public AudioSource UICancel;

    [Space]

    public Button.ButtonClickedEvent OnStartingBack;

    private List<MenuBase> allMenus = new List<MenuBase>();

    private MenuBase CurrentMenu;
    private MenuBase LastMenu;

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

    public override void OnLevelCleanUp()
    {
        Hide();

        foreach (MenuBase menu in allMenus)
        {
            menu.Hide();
        }

        allMenus.Clear();
    }

    public override void OnLevelLoaded()
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
            if(OnStartingBack != null)
                OnStartingBack.Invoke();
            
            return;
        }

        if (UICancel)
            UICancel.Play();

        if (LastMenu == TitleMenu)
            return;

        SwitchToMenu(LastMenu, false);
    }

    public void SwitchToMenu(MenuBase baseMenu, bool playEffect = true)
    {
        Debug.Log("SWITCH MENU from " + (CurrentMenu ? CurrentMenu.name : "null") + " to " + (baseMenu ? baseMenu.name : "null") + " , last was " + (LastMenu ? LastMenu.name : "null"));

        if (playEffect && UIConfirm)
            UIConfirm.Play();

        LastMenu = CurrentMenu;
        if (LastMenu)
        {
            LastMenu.OnEnd();
        }

        CurrentMenu = baseMenu;

        if (CurrentMenu)
        {
            CurrentMenu.OnStart();
        }
    }

    void Update()
    {
        if (IsDuringShowing || IsDuringHiding)
            return;

        if (EventSystem.current && EventSystem.current.currentSelectedGameObject == null && CurrentMenu && CurrentMenu.FirstToSelect)
        {
            CurrentMenu.FirstToSelect.Select();
        }

        if (Input.GetButtonDown("Back"))
        {
            GoBack(CurrentMenu);
        }
    }
}
