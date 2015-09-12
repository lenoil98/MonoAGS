﻿using System;

namespace API
{
	public interface IGame
	{
		IGameFactory Factory { get; }
		IGameState State { get; }

		IGameLoop GameLoop { get; }
		ISaveLoad SaveLoad { get; }

		IInputEvents Input { get; }

		IEvent<EventArgs> OnLoad { get; }

		void Start(string title, int width, int height);
	}
}

