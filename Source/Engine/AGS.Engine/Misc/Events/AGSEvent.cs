﻿using System;
using AGS.API;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;

namespace AGS.Engine
{
    public class AGSEvent : IEvent, IBlockingEvent
	{
		private readonly IConcurrentHashSet<Callback> _invocationList;
        private readonly EventSubscriberCounter _counter = new EventSubscriberCounter();

		public AGSEvent()
		{
			_invocationList = new AGSConcurrentHashSet<Callback>(fireListChangedEvent: false);
		}

        #region IEvent implementation

        public int SubscribersCount => _counter.Count;

        public void Subscribe(Action callback) => subscribe(new Callback(callback));

        public void Unsubscribe(Action callback) => unsubscribe(new Callback(callback));

        public void Subscribe(ClaimableCallback callback) => subscribe(new Callback(callback));

        public void Unsubscribe(ClaimableCallback callback) => unsubscribe(new Callback(callback));

        public void SubscribeToAsync(Func<Task> callback) => subscribe(new Callback(callback));

        public void UnsubscribeToAsync(Func<Task> callback) => unsubscribe(new Callback(callback));

		public async Task WaitUntilAsync(Func<bool> condition)
		{
			TaskCompletionSource<object> tcs = new TaskCompletionSource<object>(null);
			var callback = new Callback(condition, tcs);
			subscribe(callback);
			await tcs.Task;
			unsubscribe(callback);
		}

		public async Task InvokeAsync()
		{
			try
			{
                ClaimEventToken token = new ClaimEventToken();
				foreach (var target in _invocationList)
				{
                    if (target.BlockingEventWithClaimToken != null)
                    {
                        target.BlockingEventWithClaimToken(ref token);
                        if (token.Claimed) return;
                    }
					await target.Event();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("Exception when invoking an event asynchronously:");
				Debug.WriteLine(e.ToString());
				throw;
			}
		}

		public void Invoke()
		{
			try
			{
                ClaimEventToken token = new ClaimEventToken();
				foreach (var target in _invocationList)
				{
                    if (target.BlockingEventWithClaimToken != null)
                    {
                        target.BlockingEventWithClaimToken(ref token);
                        if (token.Claimed) return;
                    }
                    if (target.BlockingEvent != null) target.BlockingEvent();
                    else target.Event();
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine("Exception when invoking an event:");
				Debug.WriteLine(e.ToString());
				throw;
			}
		}

		#endregion

		private void subscribe(Callback callback)
		{
            if (_invocationList.Add(callback))
            {
                _counter.Add();
            }
		}

		private void unsubscribe(Callback callback)
		{
			if (_invocationList.Remove(callback))
			{
                _counter.Remove();
			}
		}

		private class Callback
		{
			private readonly Delegate _origObject;

			public Callback(Func<Task> callback)
			{
				_origObject = callback;
				Event = callback;
			}

			public Callback(Action callback)
			{
				_origObject = callback;
				Event = convert(callback);
				BlockingEvent = callback;
			}

			public Callback(Func<bool> condition, TaskCompletionSource<object> tcs)
			{
				_origObject = condition;
				Event = convert(condition, tcs);
			}

            public Callback(ClaimableCallback callback)
            {
                _origObject = callback;
                BlockingEventWithClaimToken = callback;
            }

			public Func<Task> Event { get; }
			public Action BlockingEvent { get; }
            public ClaimableCallback BlockingEventWithClaimToken { get; }

			public override bool Equals(object obj)
			{
				Callback other = obj as Callback;
				if (other == null) return false;
				if (other._origObject == _origObject) return true;
				if (_origObject.Target != other._origObject.Target) return false;
				return getMethodName(_origObject) == getMethodName(other._origObject);
			}

			public override int GetHashCode()
			{
				if (_origObject.Target == null) return getMethodName(_origObject).GetHashCode(); //static method subscriptions
				return _origObject.Target.GetHashCode();
			}

			public override string ToString()
			{
				return $"[Event on {_origObject.Target.ToString()} ({getMethodName(_origObject)})]";
			}

            private string getMethodName(Delegate del) => RuntimeReflectionExtensions.GetMethodInfo(del).Name;

            private Func<Task> convert(Action callback)
			{
				return () =>
				{
					callback();
					return Task.CompletedTask;
				};
			}

			private Func<Task> convert(Func<bool> condition, TaskCompletionSource<object> tcs)
			{
				return () =>
				{
					if (!condition()) return Task.CompletedTask;
					tcs.TrySetResult(null);
					return Task.CompletedTask;
				};
			}
		}
	}
}