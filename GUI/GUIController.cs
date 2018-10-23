using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Framework;
using UnityEngine.Serialization;

namespace Framework
{
    public class GUIController : BaseBehaviour
    {
        [Header("Misc")]
        public bool UseDefaultFade;
        public bool UseDefaultCinematics;
        
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
            Gameplay.Elements = new List<GUIBase>(Gameplay.Canvas.GetComponentsInChildren<GUIBase>());
            Menu.Elements     = new List<GUIBase>(Menu.Canvas.GetComponentsInChildren<GUIBase>());
            
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
    
        public void PlayUnfade()
        {
            Fade.UnfadePlayer.Play();
        }
    
        public void PlayFade()
        {
           Fade.FadePlayer.Play();
        }
    
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
            return Fade.FadePlayer.IsPlaying;
        }
    
        public bool IsDuringUnfade()
        {
            return Fade.UnfadePlayer.IsPlaying;
        }
    }
}