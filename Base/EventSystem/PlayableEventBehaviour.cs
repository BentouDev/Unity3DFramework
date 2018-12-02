using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace Framework
{	
	// A behaviour that is attached to a playable
	[System.Serializable]
	public class PlayableEventBehaviour : PlayableBehaviour
	{
		[SerializeField]
		public string OnStart;

		private UnityEvent _onStart;
	
		[SerializeField]
		public string OnEnd;

		private UnityEvent _onEnd;

		private bool _hasBegun;

		public override void OnBehaviourPlay(Playable playable, FrameData info)
		{
			if (!_hasBegun)
			{
				_hasBegun = true;
				Debug.Log("Start of " + this + " at " + Time.time + " or " + info.frameId);
				_onStart?.Invoke();
			}
		}

		public override void OnBehaviourPause(Playable playable, FrameData info)
		{
			if (_hasBegun)
			{
				_hasBegun = false;
				Debug.Log("End of " + this + " at " + Time.time + " or " + info.frameId);
				_onEnd?.Invoke();
			}
		}

		public void Init(EventScheduler eventScheduler)
		{
			SetupEvent(eventScheduler, OnStart, ref _onStart);
			SetupEvent(eventScheduler, OnEnd,   ref _onEnd);
		}

		private void SetupEvent(EventScheduler eventScheduler, string name, ref UnityEvent target)
		{
			var result = eventScheduler.Events.FirstOrDefault(x => x.Name.Equals(name));
			if (!string.IsNullOrEmpty(result.Name) && result.Event != null)
			{
				target = result.Event;
			}		
		}
	}

}
