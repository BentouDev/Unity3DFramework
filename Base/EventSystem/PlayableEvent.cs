using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
	[System.Serializable]
	public class PlayableEvent : PlayableAsset
	{
		public PlayableEventBehaviour template = new PlayableEventBehaviour();

		public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
		{
			return ScriptPlayable<PlayableEventBehaviour>.Create(graph, template);
		}
	}

}
