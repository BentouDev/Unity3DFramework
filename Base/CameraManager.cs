using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    public class CameraManager : BaseBehaviour, ILevelDependable, ILoadingDependable
    {
        public string MainTag = "MainCamera";

        public enum InitMode
        {
            Code,
            OnStart,
            OnLevelLoaded
        }

        [Header("Debug")]
        public InitMode Autostart;
        public bool PreferLevelCamera;

        public static Camera MainCamera { internal set; get; }

        private void GatherCameras(ref Dictionary<Scene, List<Camera>> cameras)
        {
            foreach (var cam in FindObjectsOfType<Camera>())
            {
                var scene = cam.gameObject.scene;
                if (cameras.ContainsKey(scene))
                    cameras[scene].Add(cam);
                else
                {
                    cameras.Add(scene, new List<Camera>(){cam});
                }
            }
        }

        private Camera FindMainCamera(IList<Camera> cameras)
        {
        #if UNITY_EDITOR
            if (cameras.Count(c => c.CompareTag(MainTag)) > 1)
                Debug.LogErrorFormat("There are more than one MainCameras in the single scene {0}! You are asking for troubles!",
                    cameras[0].gameObject.scene.ToString());
        #endif
            
            foreach (var cam in cameras)
            {
                if (cam.CompareTag(MainTag))
                    return cam;
            }

            return null;
        }

        public void OnLevelCleanUp()
        { }
        
        /// <summary>
        /// Iterate over all current camera instances and select the Main one
        /// Helps with "multiple audio listeners" Issue!
        /// Use cases:
        /// 1. One scene => there is only one scene, pick MainCamera from there
        /// 2. There are two sys scenes => pick camera from the one with higher build index
        /// 3. There is a game scene => always prefer camera from game scene
        /// </summary>

        public void AdjustCamera()
        {
            var allCameras = new Dictionary<Scene, List<Camera>>();
            var sysScenes = BaseGame.Instance?.GetLoader()?.GetCurrentSystemScenes();
            var gameScenes = BaseGame.Instance?.GetLoader()?.GetLoadedGameScenes();
            var toDisable = new List<Camera>();

            Scene? mainScene = null;
            Camera mainCamera = null;

            GatherCameras(ref allCameras);

            foreach (var (scene, cams) in allCameras)
            {
                var main = FindMainCamera(cams);
                if (main == null)
                    continue;

                if ((PreferLevelCamera && gameScenes.Contains(scene)) || sysScenes.Contains(scene))
                {
                    if (!mainScene.HasValue || scene.buildIndex > mainScene.Value.buildIndex)
                    {
                        if (mainCamera)
                            toDisable.Add(mainCamera);

                        mainScene = scene;
                        mainCamera = main;

                        continue;
                    }
                }

                if (main.enabled)
                    toDisable.Add(main);
            }
            
            SwitchCameras(toDisable, mainCamera);
        }

        public void Start()
        {
            if (Autostart == InitMode.OnStart)
                AdjustCamera();
        }

        public void OnLevelLoaded()
        {
            if (Autostart == InitMode.OnLevelLoaded)
                AdjustCamera();
        }

        public void OnPreLevelLoaded()
        { }

        private void SwitchCameras(IList<Camera> toDisable, Camera toEnable)
        {
            foreach (var cam in toDisable)
            {
                cam.enabled = false;
                cam.GetComponent<AudioListener>().enabled = false;                
            }

            MainCamera = toEnable;

            if (toEnable)
            {
                toEnable.enabled = true;
                toEnable.GetComponent<AudioListener>().enabled = true;
            }
        }

        public void PreOnLoadingScreenOn()
        {
            AdjustCamera();
        }

        public void PostOnLoadingScreenOn()
        {
            AdjustCamera();
        }

        public void PreOnLoadingScreenOff()
        {
            AdjustCamera();
        }

        public void PostOnLoadingScreenOff()
        {
            AdjustCamera();
        }
    }
}