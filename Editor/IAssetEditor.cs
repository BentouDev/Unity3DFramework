using UnityEngine;

namespace Framework.Editor
{
    public interface IAssetEditor<TAsset> where TAsset : ScriptableObject
    {
        void OnLoadAsset(TAsset asset);
        void ReloadAssetFromSelection();
    }
}
