using UnityEngine;

namespace Framework
{
    public class AnimatorParam : BaseEditorAttribute
    {
        public AnimatorControllerParameterType ParamType;
        public string AnimatorName;
        
        public AnimatorParam(string animatorName)
        {
            AnimatorName = animatorName;
        }
        
        public AnimatorParam(AnimatorControllerParameterType type, string animatorName)
        {
            ParamType = type;
            AnimatorName = animatorName;
        }

        public AnimatorParam(AnimatorControllerParameterType type)
        {
            ParamType = type;
        }
    }
}