﻿using System;
using API;
using System.Threading.Tasks;

namespace Engine
{
	public class AGSAnimationState : IAnimationState
	{
		public AGSAnimationState ()
		{
			OnAnimationCompleted = new TaskCompletionSource<AnimationCompletedEventArgs> ();
		}

		#region IAnimationState implementation

		public bool RunningBackwards { get; set; }

		public int CurrentFrame { get; set; }

		public int CurrentLoop { get; set; }

		public int TimeToNextFrame { get; set; }

		public TaskCompletionSource<AnimationCompletedEventArgs> OnAnimationCompleted { get; private set; }

		#endregion
	}
}

