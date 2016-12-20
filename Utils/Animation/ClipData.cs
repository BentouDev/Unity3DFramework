using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public struct ClipData
{
	[SerializeField]
	public int Tier;

	[SerializeField]
	public float Duration;

	[SerializeField]
    public List<AnimData> Animations;
}

[System.Serializable]
public struct AnimData
{
    [SerializeField]
    public Animator Animator;

    [SerializeField]
    public string Animation;
}