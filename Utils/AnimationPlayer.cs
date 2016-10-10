using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public struct AnimationPlayer
{
	[SerializeField]
	public bool IsPlaying;

	[SerializeField]
	public bool ForceStopOnEnd;

	[SerializeField]
	public bool StopBeforePlay;

	[SerializeField]
	public	Animator	Animator;

	[SerializeField]
	public	string		Animation;

	[SerializeField]
	public	string		StopState;

	[SerializeField]
	public	float		Duration;

	[SerializeField]
	public	Button.ButtonClickedEvent OnStart;

	[SerializeField]
	public	Button.ButtonClickedEvent OnFinish;

	private float Elapsed;

    public bool IsValid()
    {
        return Animator && !string.IsNullOrEmpty(Animation);
    }

	public void Stop()
    {
        if (!IsValid())
            return;

        Animator.StopPlayback();
		Animator.Play(StopState);
	}

	public void Play()
	{
	    if (!IsValid())
	        return;

		if (StopBeforePlay)
			Stop();

        if(Duration < Mathf.Epsilon)
            Debug.LogWarning("AnimPlayer: anim " + Animation + " from " + Animator + " duration is zero!");

		Elapsed		= 0;
		IsPlaying	= true;
	    if (Animator && !string.IsNullOrEmpty(Animation))
	    {
            // Animator.StartPlayback();
	        Animator.Play(Animation);
	    }

		OnStart.Invoke();
	}

	public void Update()
	{
		if (!IsPlaying)
			return;

		Elapsed += Time.unscaledDeltaTime;
		if (Elapsed > Duration)
		//    || (Animator && !string.IsNullOrEmpty(Animation)
		//        && !Animator.GetCurrentAnimatorStateInfo(0).IsName(Animation)))
		{
		    if (ForceStopOnEnd)
		    {
		        Stop();
		    }

			IsPlaying = false;
			OnFinish.Invoke();
		}
	}
}