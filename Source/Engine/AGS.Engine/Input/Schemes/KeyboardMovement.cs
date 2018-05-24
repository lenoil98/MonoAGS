﻿using AGS.API;
using System.Threading.Tasks;
using System.Linq;

namespace AGS.Engine
{
	public enum KeyboardMovementMode
	{
		/// <summary>
		/// In pressing mode, the user has to keep pressing the keys to move.
		/// </summary>
		Pressing,
		/// <summary>
		/// In tapping mode, the user taps once to move, and taps again to stop.
		/// </summary>
		Tapping,
	}

	public class KeyboardMovement
	{
		private IConcurrentHashSet<Key> _up, _down, _left, _right, _keysDown;
		private ICharacter _character;
        private readonly IFocusedUI _focusedUi;

		public KeyboardMovement(ICharacter character, IInput input, IFocusedUI focusedUi, KeyboardMovementMode mode)
		{
			_character = character;
            _focusedUi = focusedUi;
			Enabled = true;
			Mode = mode;
			_up = new AGSConcurrentHashSet<Key> ();
			_down = new AGSConcurrentHashSet<Key> ();
			_left = new AGSConcurrentHashSet<Key> ();
			_right = new AGSConcurrentHashSet<Key> ();
			_keysDown = new AGSConcurrentHashSet<Key> ();

			input.KeyDown.SubscribeToAsync(onKeyDown);
			input.KeyUp.SubscribeToAsync(onKeyUp);
		}

		public void AddArrows()
		{
			_up.Add(Key.Up);
			_down.Add(Key.Down);
			_left.Add(Key.Left);
			_right.Add(Key.Right);
		}

		public void AddWASD()
		{
			_up.Add(Key.W);
			_down.Add(Key.S);
			_left.Add(Key.A);
			_right.Add(Key.D);
		}

		public void AddUp(Key up) { _up.Add(up); }
		public void AddDown(Key down) { _down.Add(down); }
		public void AddLeft(Key left) { _left.Add(left); }
		public void AddRight(Key right) { _right.Add(right); }

		public bool Enabled { get; set; }
		public KeyboardMovementMode Mode { get; set; }
		public Direction? CurrentDirection { get; private set; }

		private async Task onKeyDown(KeyboardEventArgs args)
		{
            if (!Enabled || _focusedUi.HasKeyboardFocus != null) return;
			_keysDown.Add(args.Key);
			Direction? direction = getDirection();
			if (Mode == KeyboardMovementMode.Pressing)
			{
				if (direction == CurrentDirection) return;
			}
			else
			{
				if (direction == CurrentDirection)
				{
					if (direction == null) return;
					CurrentDirection = null;
					await _character.StopWalkingAsync();
					return;
				}
			}
			CurrentDirection = direction;
			if (direction == null) return;
			await _character.WalkStraightAsync(getTarget(direction.Value));
		}

		private async Task onKeyUp(KeyboardEventArgs args)
		{
            if (!Enabled || _focusedUi.HasKeyboardFocus != null) return;
			_keysDown.Remove(args.Key);
			if (Mode == KeyboardMovementMode.Tapping) return;
			Direction? direction = getDirection();
			if (direction == CurrentDirection) return;
			CurrentDirection = direction;
			if (direction == null)
			{
				await _character.StopWalkingAsync();
				return;
			}
			await _character.WalkStraightAsync(getTarget(direction.Value));
		}

		private Direction? getDirection()
		{
			if (isDown())
			{
				if (isRight()) return Direction.DownRight;
				if (isLeft()) return Direction.DownLeft;
				return Direction.Down;
			}
			if (isUp())
			{
				if (isRight()) return Direction.UpRight;
				if (isLeft()) return Direction.UpLeft;
				return Direction.Up;
			}
			if (isRight()) return Direction.Right;
			if (isLeft()) return Direction.Left;
			return null;
		}

		private Position getTarget(Direction direction)
		{
            (var x, var y, _) = _character.Position;
			switch (direction)
			{
				case Direction.Down: case Direction.DownLeft: case Direction.DownRight:
                    y = float.MinValue;
					break;
				case Direction.Up: case Direction.UpLeft: case Direction.UpRight:
                    y = float.MaxValue;
					break;
			}
			switch (direction)
			{
				case Direction.Left: case Direction.DownLeft: case Direction.UpLeft:
                    x = float.MinValue;
					break;
				case Direction.Right: case Direction.DownRight: case Direction.UpRight:
                    x = float.MaxValue;
					break;
			}
			return (x, y);
		}

        private bool isUp() => _keysDown.Any(_up.Contains);
        private bool isDown() => _keysDown.Any(_down.Contains);
        private bool isLeft() => _keysDown.Any(_left.Contains);
        private bool isRight() => _keysDown.Any(_right.Contains);
    }
}