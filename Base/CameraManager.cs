using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    public class CameraManager : BaseBehaviour, ILevelDependable
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

        private Camera FindSceneCamera(Scene mainScene, bool expectedResult)
        {
            if (!mainScene.IsValid())
                return null;

            foreach (var cam in FindObjectsOfType<Camera>())
            {
                if ((cam.gameObject.scene == mainScene) == expectedResult)
                {
                    if (cam.CompareTag(MainTag))
                        return cam;
                }
            }

            return null;
        }

        public Camera AquireLevelCamera(Scene mainScene)
        {
            return FindSceneCamera(mainScene, false);
        }
        
        public Camera AquireMainCamera(Scene mainScene)
        {
            return FindSceneCamera(mainScene, true);
        }

        public void OnLevelCleanUp()
        { }

        public void Init()
        {
            var scene = BaseGame.Instance?.GetLoader()?.BaseScene;
            if (scene != null && scene.HasValue())
            {
                var mainScene = SceneManager.GetSceneByPath(scene);
                var gameCamera = AquireMainCamera(mainScene);
                var levelCamera = AquireLevelCamera(mainScene);

                if (gameCamera != levelCamera)
                {
                    if (levelCamera && PreferLevelCamera)
                    {
                        SwitchCameras(gameCamera, levelCamera);
                    }
                    else
                    {
                        SwitchCameras(levelCamera, gameCamera);
                    }                    
                }
            }            
        }

        public void Start()
        {
            if (Autostart == InitMode.OnStart)
                Init();
        }

        public void OnLevelLoaded()
        {
            if (Autostart == InitMode.OnLevelLoaded)
                Init();
        }

        private void SwitchCameras(Camera toDisable, Camera toEnable)
        {
            if (toDisable)
                toDisable.enabled = false;

            MainCamera = toEnable;

            if (toEnable)
                toEnable.enabled = true;
        }
    }
}