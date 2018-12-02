using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace Framework
{
	[System.Serializable]
	public class PlayableEventMixer : PlayableBehaviour
	{
		public EventScheduler eventScheduler { get; set; }

		public override void OnGraphStart(Playable playable)
		{
			if (!Application.isPlaying) return;
			if (!eventScheduler) return;
			if (!eventScheduler.gameObject.activeInHierarchy) return;
		
			var clipCount = playable.GetInputCount();
			for (var i = 0; i < clipCount; i++)
			{
				var clip = ((ScriptPlayable<PlayableEventBehaviour>) playable.GetInput(i)).GetBehaviour();
				clip.Init(eventScheduler);
			}
		}

		/*public override void ProcessFrame(Playable playable, FrameData info, object playerData)
		{
			if (!Application.isPlaying) return;
	
			if (!eventScheduler) return;
	
			if (!eventScheduler.gameObject.activeInHierarchy) return;
			
			// Retrieve the track time (playhead position) from the root playable.
			var rootPlayable = playable.GetGraph().GetRootPlayable(0);
			var time = (float)rootPlayable.GetTime();		
		}*/
	}

}
