﻿using System;
using System.Threading.Tasks;

namespace API
{
	public interface IAnimationState
	{
		bool RunningBackwards { get; set; }
		int CurrentFrame { get; set; }
		int CurrentLoop { get; set; }
		int TimeToNextFrame { get; set; }

		TaskCompletionSource<AnimationCompletedEventArgs> OnAnimationCompleted { get; }
	}
}

