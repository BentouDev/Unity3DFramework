using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Framework
{
    [TrackColor(0.4f, 0.7f, 0.6f)]
    [TrackClipType(typeof(PlayableEvent))]
    [TrackBindingType(typeof(EventScheduler))]
    public class PlayableEventTrack : TrackAsset
    {
        public PlayableEventMixer template = new PlayableEventMixer();
        
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            // Retrieve the reference to the track-bound event scheduler via the
            // director.
            var director = go.GetComponent<PlayableDirector>();
            var es = director.GetGenericBinding(this) as EventScheduler;

            // Create a track mixer playable and give the reference to the event
            // scheduler (it has to be initialized before OnGraphStart).
            var playable = ScriptPlayable<PlayableEventMixer>.Create(graph, template, inputCount);
            playable.GetBehaviour().eventScheduler = es;

            return playable;
        }
    }
}