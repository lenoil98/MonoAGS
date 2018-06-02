﻿using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using AGS.API;
using OpenTK;
using OpenTK.Input;

namespace AGS.Engine.Desktop
{
    public class AGSInput : IAGSInput
    {
        private GameWindow _game;
        private IWindowInfo _window;
        private Size _virtualResolution;
        private float _mouseX, _mouseY;
        private readonly IGameState _state;
        private readonly IShouldBlockInput _shouldBlockInput;
        private readonly IConcurrentHashSet<API.Key> _keysDown;
        private readonly IGameEvents _events;

        private IObject _mouseCursor;
        private MouseCursor _originalOSCursor;
        private readonly ConcurrentQueue<Func<Task>> _actions;
        private int _inUpdate; //For preventing re-entrancy

        public AGSInput(IGameState state, IGameEvents events, IShouldBlockInput shouldBlockInput, 
                        IEvent<AGS.API.MouseButtonEventArgs> mouseDown, 
                        IEvent<AGS.API.MouseButtonEventArgs> mouseUp, IEvent<MousePositionEventArgs> mouseMove,
                        IEvent<KeyboardEventArgs> keyDown, IEvent<KeyboardEventArgs> keyUp)
        {
            _events = events;
            _actions = new ConcurrentQueue<Func<Task>>();
            this._shouldBlockInput = shouldBlockInput;
            this._state = state;
            this._keysDown = new AGSConcurrentHashSet<API.Key>();

            MouseDown = mouseDown;
            MouseUp = mouseUp;
            MouseMove = mouseMove;
            KeyDown = keyDown;
            KeyUp = keyUp;

            if (AGSGameWindow.GameWindow != null) Init(AGSGameWindow.GameWindow);
        }

        public void Init(API.Size virtualResolution) => _virtualResolution = virtualResolution;

        public void Init(IWindowInfo window)
        {
            _window = window;
            if (_game != null) init(_game);
        }

        public void Init(GameWindow game)
        {
            if (_game != null) return;
            _game = game;
            if (_window != null) init(game);
        }

        private void init(GameWindow game)
        {
            this._originalOSCursor = game.Cursor;

            game.MouseDown += (sender, e) =>
            {
                if (isInputBlocked()) return;
                var button = convert(e.Button);
                _actions.Enqueue(() => MouseDown.InvokeAsync(new AGS.API.MouseButtonEventArgs(null, button, MousePosition)));
            };
            game.MouseUp += (sender, e) =>
            {
                if (isInputBlocked()) return;
                var button = convert(e.Button);
                _actions.Enqueue(() => MouseUp.InvokeAsync(new AGS.API.MouseButtonEventArgs(null, button, MousePosition)));
            };
            game.MouseMove += (sender, e) =>
            {
                _mouseX = e.Mouse.X;
                _mouseY = e.Mouse.Y;
                _actions.Enqueue(() => MouseMove.InvokeAsync(new MousePositionEventArgs(MousePosition)));
            };
            game.KeyDown += (sender, e) =>
            {
                API.Key key = convert(e.Key);
                _keysDown.Add(key);
                if (isInputBlocked()) return;
                _actions.Enqueue(() => KeyDown.InvokeAsync(new KeyboardEventArgs(key)));
            };
            game.KeyUp += (sender, e) =>
            {
                API.Key key = convert(e.Key);
                _keysDown.Remove(key);
                if (isInputBlocked()) return;
                _actions.Enqueue(() => KeyUp.InvokeAsync(new KeyboardEventArgs(key)));
            };

            _events.OnRepeatedlyExecuteAlways.Subscribe(onRepeatedlyExecute);
        }

        #region IInputEvents implementation

        public IEvent<AGS.API.MouseButtonEventArgs> MouseDown { get; private set; }

        public IEvent<AGS.API.MouseButtonEventArgs> MouseUp { get; private set; }

        public IEvent<MousePositionEventArgs> MouseMove { get; private set; }

        public IEvent<KeyboardEventArgs> KeyDown { get; private set; }

        public IEvent<KeyboardEventArgs> KeyUp { get; private set; }

        #endregion

        public bool IsKeyDown(AGS.API.Key key) => _keysDown.Contains(key);

        public MousePosition MousePosition => new MousePosition(_mouseX, _mouseY, _state.Viewport, _virtualResolution, _window);

        public bool LeftMouseButtonDown { get; private set; }
        public bool RightMouseButtonDown { get; private set; }
        public bool IsTouchDrag => false;  //todo: support touch screens on desktops

        public IObject Cursor
        {
            get => _mouseCursor;
            set
            {
                _mouseCursor = value;
                if (value != null)
                {
                    _game.Cursor = MouseCursor.Empty;
                }
            }
        }

        public bool ShowHardwareCursor
        {
            get => _game.Cursor != MouseCursor.Empty;
            set => _game.Cursor = value ? _originalOSCursor : MouseCursor.Empty;
        }

        private bool isInputBlocked() => _shouldBlockInput.ShouldBlockInput();

        private AGS.API.MouseButton convert(OpenTK.Input.MouseButton button)
        {
            switch (button)
            {
                case OpenTK.Input.MouseButton.Left:
                    return AGS.API.MouseButton.Left;
                case OpenTK.Input.MouseButton.Right:
                    return AGS.API.MouseButton.Right;
                case OpenTK.Input.MouseButton.Middle:
                    return AGS.API.MouseButton.Middle;
                default:
                    throw new NotSupportedException();
            }
        }

        private void onRepeatedlyExecute()
        {
            if (!isInputBlocked())
            {
                var cursorState = Mouse.GetCursorState();
                LeftMouseButtonDown = cursorState.LeftButton == ButtonState.Pressed;
                RightMouseButtonDown = cursorState.RightButton == ButtonState.Pressed;
            }

            if (Interlocked.CompareExchange(ref _inUpdate, 1, 0) != 0) return;
            try
            {
                while (_actions.TryDequeue(out var action))
                {
                    action();
                }
            }
            finally
            {
                _inUpdate = 0;
            }
        }

        private AGS.API.Key convert(OpenTK.Input.Key key) => (AGS.API.Key)(int)key;
    }
}