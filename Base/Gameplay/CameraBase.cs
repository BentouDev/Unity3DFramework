using System.Collections;
using UnityEngine;
using UnityStandardAssets.ImageEffects;
using Utils;

public abstract class CameraBase : MonoBehaviour
{
    protected ColorCorrectionCurves ColorCorrection;
    protected TiltShift Tilt;
    protected bool isDesaturating;

    /// <summary>
    /// Przetwarza dane wejściowe w odpowiedni dla danego komponentu sposób.
    /// </summary>
    /// <param name="inputData">Dane przekazane przez PlayerController.</param>
    public abstract void ProcessInput(InputBuffer.InputData inputData);

    public abstract void StartAnimating(float speed, float duration, float rate);

    public abstract bool IsAnimating();

    public void StartDesaturateAnim(float from, float to, float duration)
    {
        StartCoroutine(DoAnim(from, to, duration));
    }

    public bool IsDesaturating()
    {
        return isDesaturating;
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

        isDesaturating = false;
    }

    public virtual void OnLevelLoaded()
    {
        StopAllCoroutines();

        Tilt = GetComponent<TiltShift>();
        ColorCorrection = GetComponent<ColorCorrectionCurves>();

        Tilt.enabled = false;
        Tilt.blurArea = 0;

        ColorCorrection.saturation = 1;
        isDesaturating = false;
    }

    private IEnumerator DoAnim(float from, float to, float duration)
    {
        isDesaturating = true;
        Tilt.enabled = true;

        float elapsed = 0;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;

            float ratio = Mathf.Clamp01((float)Tween.Get(elapsed, duration, Tweener.SinusoidalEaseOut));
            float value = Mathf.Lerp(from, to, ratio);
            ColorCorrection.saturation = value;
            Tilt.blurArea = (1 - value) * 15;

            yield return null;
        }

        Tilt.blurArea = (1 - to) * 15;
        ColorCorrection.saturation = to;
        isDesaturating = false;
    }
}
