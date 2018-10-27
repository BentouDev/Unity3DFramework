using UnityEngine;
using Utils;

namespace Framework.Utils
{
    public class TweenSystem : Singleton<TweenSystem>
    {
        public bool Enabled = true;
        
        void FixedUpdate()
        {
            if (Enabled) Tween.Update(Time.fixedTime, Time.time);
        }
    }
}