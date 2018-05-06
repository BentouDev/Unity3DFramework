using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Framework
{
	public class EventQueue<TEvent> where TEvent : class, new()
	{
		public delegate bool Processor(TEvent @event);
		public delegate void PostEvent(TEvent @event);

		private readonly List<FieldInfo>     _fields = new List<FieldInfo>();
		private readonly System.Type         _type;
		private readonly UniqueQueue<TEvent> _queue;
		private readonly UniqueQueue<TEvent> _pool;
		private Processor                    _processor;
		private readonly List<PostEvent>     _postEvents;

		public bool AnyFailrue { get; private set; }
		public bool AnySuccess { get; private set; }

		private readonly int _maxPoolSize;
		
		public static readonly int UnlimitedPool = -1;

		public EventQueue(int startPoolSize = 10, int maxPoolSize = 100)
		{
			_type        = typeof(TEvent);
			_maxPoolSize = maxPoolSize;
			_pool        = new UniqueQueue<TEvent>(startPoolSize);
			_queue       = new UniqueQueue<TEvent>();
			_postEvents  = new List<PostEvent>();
			
			_fields.AddRange(_type.GetFields());
		}

		public void Reassign(Processor processor)
		{
			_processor   = processor;
		}

		public void AddPost(PostEvent @event)
		{
			_postEvents.Add(@event);
		}

		public void RemovePost(PostEvent @event)
		{
			_postEvents.Remove(@event);
		}

		private void ResetObjects()
		{
			foreach (TEvent @event in _pool)
			{
				foreach (FieldInfo field in _fields)
				{
					field.SetValue(@event, field.FieldType.GetDefault());
				}
			}
		}

		public void Clear()
		{
			if (_maxPoolSize != UnlimitedPool)
			{
				_pool.EnqueueRange(_queue.Take(_maxPoolSize - _pool.Count));
			}
			else
			{
				_pool.EnqueueRange(_queue);
			}

			_queue.Clear();

			ResetObjects();
		}

		public TEvent Post()
		{
			TEvent @event = _pool.Any() ? _pool.Dequeue() : new TEvent();
			_queue.Enqueue(@event);
			
			return @event;
		}

		public void Process()
		{
			AnyFailrue = false;
			AnySuccess = false;

			if (_processor == null)
				return;
			
			foreach (TEvent @event in _queue)
			{
				var result = _processor(@event);
				AnyFailrue |= !result;
				AnySuccess |=  result;
				
				if (_maxPoolSize == UnlimitedPool || _pool.Count < _maxPoolSize)
					_pool.Enqueue(@event);
			}

			foreach (TEvent @event in _queue)
			{
				foreach (PostEvent signal in _postEvents)
				{
					signal(@event);
				}
			}
			
			_queue.Clear();

			ResetObjects();
		}
	}	
}