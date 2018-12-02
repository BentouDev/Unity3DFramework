using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace Framework
{
    public class EventScheduler : BaseBehaviour
    {
        [System.Serializable]
        public struct SchedulableEvent
        {
            [SerializeField] public string Name;
            [SerializeField] public UnityEvent Event;
        }
    
        public List<SchedulableEvent> Events = new List<SchedulableEvent>();
    
        public void RaiseEvent(string name)
        {
            var result = Events.FirstOrDefault(x => x.Name.Equals(name));
    
            if (!string.IsNullOrEmpty(result.Name) && result.Event != null)
            {
                result.Event.Invoke();
            }
        }
    }
}