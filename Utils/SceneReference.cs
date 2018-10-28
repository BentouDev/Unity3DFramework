using UnityEngine;
using UnityEngine.SceneManagement;

namespace Framework
{
    [System.Serializable]
    public class SceneReference : IValueChecker
    {
        [SerializeField]
        private Object sceneAsset;

        [SerializeField] 
        private string sceneName = "";
 
        public string SceneName => sceneName;

        // makes it work with the existing Unity methods (LoadLevel/LoadScene)
        public static implicit operator string(SceneReference sceneField)
        {
            return sceneField.SceneName;
        }

        public bool HasValue()
        {
            return !string.IsNullOrWhiteSpace(SceneName) 
               && SceneManager.GetSceneByPath(SceneName).IsValid();
        }
    }
}