// using Duel;

using System.Linq;
using Framework;
using UnityEngine;
using UnityEngine.UI;

public class AnimationDirector : MonoBehaviour
{
    [Header("Animations")]
    [SerializeField]
	public	ClipData[]	Animations;
    
    [Header("Camera")]
	public	Camera		PreviewCamera;
	public	Transform	FinalCamera;
    
    [Header("Preview Events")]
    public Button.ButtonClickedEvent OnStarting;
    public Button.ButtonClickedEvent OnEnding;
    public Button.ButtonClickedEvent OnFinalCamera;


    private bool _isWorking;
    private bool IsFinishing;

    private int CurrentAnim = 0;
    private float CamAnimDuration;
    private float Elapsed;

    void Awake()
	{
		PreviewCamera.gameObject.SetActive(false);
	}

	public bool IsAnimating()
	{
		return CurrentAnim != Animations.Length;
	}

	public bool IsFinished()
	{
		return !IsFinishing || Elapsed > CamAnimDuration;
	}

	public void StartPreview()
	{
	    if (OnStarting != null)
	    {
	        OnStarting.Invoke();
	    }

		_isWorking = true;

		Elapsed		= 0;
		CurrentAnim	= 0;

		PreviewCamera.gameObject.SetActive(true);

		var clip = Animations[CurrentAnim];
	    foreach (AnimData anim in clip.Animations)
	    {
	        anim.Animator.Play(anim.Animation);
	    }
	}

	public void SkipPreview()
	{
	    if (_isWorking && OnEnding != null)
	    {
	        OnEnding.Invoke();
	    }

		_isWorking	= false;
		IsFinishing	= false;
	}

	void FixedUpdate()
	{
		if (_isWorking)
		{
			Elapsed += Time.fixedDeltaTime;
			var anim = Animations[CurrentAnim];
		    var hasAnimEnded = !anim.Animations.Any(a => a.Animator.GetCurrentAnimatorStateInfo(0).IsName(a.Animation));

		    if (hasAnimEnded || Elapsed > anim.Duration)
		    {
		        ClipFinished();
		    }
		}
		else if (IsFinishing)
		{
			Elapsed		+= Time.fixedDeltaTime;
			//IsFinishing	 = !MainCamera.IsAnimationCompleted();
		}
	}

	private void ClipFinished()
	{
		Elapsed = 0;

		var oldClip = Animations[CurrentAnim];
	    {
	        foreach (AnimData anim in oldClip.Animations)
            {
                anim.Animator.StopPlayback();
            }
	    }

		CurrentAnim++;

		if (!IsAnimating())
		{
		    if (OnEnding != null)
		    {
		        OnEnding.Invoke();
		    }

		    _isWorking = false;
			return;
		}

		var clip = Animations[CurrentAnim];
	    {
	        foreach (AnimData anim in clip.Animations)
            {
                anim.Animator.Play(anim.Animation);
            }
	    }
	}

	public void StartFinalCamera(Camera cam, float speed = 1, float rate = 4, float duration = 0)
	{
	    if (OnFinalCamera != null)
	    {
            OnFinalCamera.Invoke();
        }

	    CamAnimDuration = duration;

	    if (FinalCamera)
	    {
	        cam.transform.position = FinalCamera.transform.position;
            cam.transform.rotation = FinalCamera.transform.rotation;
        }

	    var cameraBase = (ICinematicCamera) cam.GetComponent(typeof(ICinematicCamera));
		if (cameraBase != null)
			cameraBase.StartAnimating(speed, duration, rate);

		IsFinishing	= true;

        cam.gameObject.SetActive(true);
        PreviewCamera.gameObject.SetActive(false);
	}
}
