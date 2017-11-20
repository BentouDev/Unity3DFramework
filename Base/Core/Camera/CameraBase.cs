using System.Collections;
using UnityEngine;
using Utils;

namespace Framework
{
    public abstract class CameraBase : MonoBehaviour
    {
        /*protected bool _isDesaturating;
        
        public void StartDesaturateAnim(float from, float to, float duration)
        {
            StartCoroutine(DoAnim(from, to, duration));
        }

        public bool IsDesaturating()
        {
            return _isDesaturating;
        }

        public virtual void OnLevelCleanUp()
        {
            StopAllCoroutines();

            if (Tilt)
            {
                Tilt.enabled = false;
                Tilt.blurArea = 0;
                Tilt = null;
            }

            if (ColorCorrection)
            {
                ColorCorrection.saturation = 1;
                ColorCorrection = null;
            }

            _isDesaturating = false;
        }

        public virtual void OnLevelLoaded()
        {
            StopAllCoroutines();

            Tilt = GetComponent<TiltShift>();
            ColorCorrection = GetComponent<ColorCorrectionCurves>();

            if (Tilt)
            {
                Tilt.enabled = false;
                Tilt.blurArea = 0;
            }

            if(ColorCorrection)
                ColorCorrection.saturation = 1;

            _isDesaturating = false;
        }

        private IEnumerator DoAnim(float from, float to, float duration)
        {
            if (Tilt || ColorCorrection)
            {
                _isDesaturating = true;

                if(Tilt)
                    Tilt.enabled = true;

                float elapsed = 0;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;

                    float ratio = Mathf.Clamp01((float)Tween.Get(elapsed, duration, Tweener.SinusoidalEaseOut));
                    float value = Mathf.Lerp(from, to, ratio);

                    if(ColorCorrection)
                        ColorCorrection.saturation = value;

                    if(Tilt)
                        Tilt.blurArea = (1 - value) * 15;

                    yield return null;
                }

                if(Tilt)
                    Tilt.blurArea = (1 - to) * 15;
                if(ColorCorrection)
                    ColorCorrection.saturation = to;

                _isDesaturating = false;
            }
        }*/
    }
}