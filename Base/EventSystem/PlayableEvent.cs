using UnityEngine;
using UnityEngine.Playables;

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
