using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using UnityEngine.Events;
using UnityEngine.Serialization;
using Utils;

namespace Framework
{
    public class GUIController : BaseBehaviour
    {
        [Header("Misc")]
        public bool UseDefaultFade;
        public bool UseDefaultCinematics;
        public bool StartFromDefaultFade;
        public float DefaultFadeTime = 1.25f;
        
        [System.Serializable]
        public struct CanvasMode
        {
            [SerializeField]
            public Canvas        Canvas;
            
            [FormerlySerializedAs("GUIs")] 
            public List<GUIBase> Elements;
        }
    
        [Header("Canvas Modes")]
        public CanvasMode Gameplay;
        public CanvasMode Menu;
        
        [System.Serializable]
        public struct FadeInfo
        {
            [SerializeField]
            public AnimationPlayer FadePlayer;
    
            [SerializeField] 
            public AnimationPlayer UnfadePlayer;
        }
    
        [System.Serializable]
        public struct CinematicInfo
        {
            [SerializeField]
            public AnimationPlayer FadePlayer;
    
            [SerializeField]
            public AnimationPlayer UnfadePlayer;
        }
    
        [Header("Animation")]
        public FadeInfo Fade;
        public CinematicInfo Cinematic;
    
        private List<GUIBase> _allGUIs;
        private List<GUIBase> AllGUIS
        {
            get
            {
                if (_allGUIs == null)
                {
                    _allGUIs = new List<GUIBase>();
                    var guis = FindObjectsOfType<GUIBase>();
                    foreach (var gui in guis)
                    {
                        _allGUIs.Add(gui);
                    }
                }
    
                return _allGUIs;
            }
        }
    
        void OnAwake()
        {
            foreach (GUIBase guiBase in AllGUIS)
            {
                guiBase.OnAwake();
            }
        }
        
        void Start()
        {
            if (Gameplay.Canvas)
                Gameplay.Elements = new List<GUIBase>(Gameplay.Canvas.GetComponentsInChildren<GUIBase>());
            
            if (Menu.Canvas)
                Menu.Elements = new List<GUIBase>(Menu.Canvas.GetComponentsInChildren<GUIBase>());
            
            if(Fade.FadePlayer.IsPlaying)
                Fade.FadePlayer.Play();
            if (Fade.UnfadePlayer.IsPlaying)
                Fade.UnfadePlayer.Play();
    
            if (Cinematic.FadePlayer.IsPlaying)
                Cinematic.FadePlayer.Play();
            if (Cinematic.UnfadePlayer.IsPlaying)
                Cinematic.UnfadePlayer.Play();
        }
    
        void Update()
        {
            Fade.FadePlayer.Update();
            Fade.UnfadePlayer.Update();
            Cinematic.FadePlayer.Update();
            Cinematic.UnfadePlayer.Update();
        }
    
        public void HideAllUIs()
        {
            foreach (GUIBase guiBase in AllGUIS.Where(g=>g.IsGameplayGUI))
            {
                guiBase.Hide();
            }
        }
        
        public void ShowAllUIs()
        {
            foreach (GUIBase guiBase in AllGUIS.Where(g => g.IsGameplayGUI))
            {
                guiBase.Show();
            }
        }
    
        public void SwitchToGameplay()
        {
            foreach (GUIBase gui in Gameplay.Elements)
            {
                gui.Show();
            }
        }
        
        public void SwitchToMenu()
        {
            foreach (GUIBase gui in Gameplay.Elements)
            {
                gui.Hide();
            }        
        }
    
        public bool IsCinematicShown()
        {
            var state = Cinematic.FadePlayer.Animator.GetCurrentAnimatorStateInfo(0);
            return state.IsName(Cinematic.FadePlayer.StopState);
        }
    
        public void ShowCinematicsWithAnim()
        {
            Cinematic.FadePlayer.Play();
        }
    
        public void HideCinematicsWithAnim()
        {
            Cinematic.UnfadePlayer.Play();
        }
    
        public void ShowCinematics()
        {
            Cinematic.FadePlayer.Stop();
            Cinematic.FadePlayer.Animator.Update(0);
        }
    
        public void HideCinematics()
        {
            Cinematic.UnfadePlayer.Stop();
            Cinematic.UnfadePlayer.Animator.Update(0);
        }

        private void GenericPlay(AnimationPlayer player, UnityAction onFinish)
        {
            if (player.IsPlaying)
                return;

            if (onFinish != null)
            {
                UnityAction finisher = () =>
                {
                    onFinish();
                    player.OnFinish.RemoveListener(FadeFinishDelegate);
                    FadeFinishDelegate = null;
                    StartFromDefaultFade = false;
                };
                
                FadeFinishDelegate = finisher;
                player.OnFinish.AddListener(finisher);
            }

            player.Play();
        }

        private UnityAction       FadeFinishDelegate;
        private Tween.SimpleEvent TweenFinishDelegate;

        public bool PlayUnfade(UnityAction onFinish = null)
        {
            if (IsDuringUnfade() || IsDuringFade())
                return false;

            if (!UseDefaultFade && Fade.UnfadePlayer.IsValid())
                GenericPlay(Fade.UnfadePlayer, onFinish);
            else
            {
                StartCoroutine(ProcessTween(1, 0, onFinish));
            }

            return true;
        }
    
        public bool PlayFade(UnityAction onFinish = null)
        {
            if (IsDuringUnfade() || IsDuringFade())
                return false;

            if (!UseDefaultFade && Fade.FadePlayer.IsValid())
                GenericPlay(Fade.FadePlayer, onFinish);
            else
            {
                StartCoroutine(ProcessTween(0, 1, onFinish));
            }
            
            return true;
        }

        private IEnumerator ProcessTween(float from, float target, UnityAction onFinish)
        {
            float startTime = Time.time;

            _IsFading = true;

            while (Time.time - startTime < DefaultFadeTime * 1.5f)
            {
                _FadeValue = Mathf.Lerp(from, target, (Time.time - startTime) / DefaultFadeTime);
                yield return null;
            }

            _IsFading = false;
            _FadeValue = target;
            
            Debug.Log("FADE COMPLETED");
            
            onFinish?.Invoke();
        }

//        private void PrepareTween(double from, double target, UnityAction onFinish)
//        {
//            _fadeTween = new Tween();
//            _fadeTween.from   = from;
//            _fadeTween.target = target;
//            _fadeTween.easing = Tweener.SinusoidalEaseInOut;
//            _fadeTween.time   = DefaultFadeTime;
//
//            if (onFinish != null)
//            {
//                Tween.SimpleEvent finisher = (t) =>
//                {
//                    onFinish();
//                    _fadeTween.OnFinish -= TweenFinishDelegate;
//                    TweenFinishDelegate  = null;
//                    _fadeTween = null;
//                };
//
//                TweenFinishDelegate  = finisher;
//                _fadeTween.OnFinish += TweenFinishDelegate;
//            }
//
//            _fadeTween.Start();
//        }
    
        public bool IsDuringCinematicsOn()
        {
            return Cinematic.FadePlayer.IsPlaying;
        }
    
        public bool IsDuringCinematicsOff()
        {
            return Cinematic.UnfadePlayer.IsPlaying;
        }
    
        public bool IsDuringFade()
        {
            return Fade.FadePlayer.IsPlaying || _IsFading;
        }
    
        public bool IsDuringUnfade()
        {
            return Fade.UnfadePlayer.IsPlaying || _IsFading;
        }

        // private Tween _fadeTween;
        private static float _FadeValue = 0;
        private static bool _IsFading = false;

        public Color _fadeColor = Color.white;
        public Color FadeColor
        {
            get
            {
                _fadeColor.a = _FadeValue;
                return _fadeColor;
            }
        }

        private static Rect _fullscreenRect = new Rect();
        public static Rect FullscreenRect
        {
            get
            {
                _fullscreenRect.Set(0, 0, Screen.width, Screen.height);
                return _fullscreenRect;
            }
        }

        private static Texture2D _zaTexture;
        public Texture2D TheTexture
        {
            get
            {
                if (!_zaTexture)
                {
                    _zaTexture = new Texture2D(1, 1);
                    _zaTexture.SetPixel(0, 0, Color.black);
                    _zaTexture.Apply();
                }
                
                return _zaTexture;
            }
        }

        void OnGUI()
        {
            GUI.color = FadeColor;
            GUI.DrawTexture(FullscreenRect, TheTexture);
            
//            GUI.color = FadeColor;
//            GUI.Label(new Rect(0,0,100,200), "Fading...");
//            GUI.skin.box.normal.background = TheTexture;
//            GUI.Box(FullscreenRect, GUIContent.none);
        }
    }
}