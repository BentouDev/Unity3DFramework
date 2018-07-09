using UnityEditor;

namespace Framework.Editor.Base
{
    [CustomEditor(typeof(BaseBehaviour), editorForChildClasses: true)]
    [CanEditMultipleObjects]
    public class BaseBehaviourEditor : BaseFrameworkEditor
    {
        
    }
}