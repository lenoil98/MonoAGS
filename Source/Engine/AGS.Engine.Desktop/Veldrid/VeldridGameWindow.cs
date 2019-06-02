﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using AGS.API;
using Veldrid;
using Veldrid.Sdl2;
using Veldrid.StartupUtilities;

namespace AGS.Engine.Desktop
{
    public class VeldridGameWindow : IGameWindow, IWindowInfo
    {
        private IGameWindowSize _windowSize;
        private readonly Sdl2Window _window;
        private readonly GraphicsDevice _graphicsDevice;
        private bool _hasBorder = true;
        private API.WindowState _windowState = API.WindowState.Normal;
        private AGSUpdateThread _renderThread;
        private double _targetUpdateFrequency;
        private bool _resized;

        public VeldridGameWindow(IGameSettings settings, IGameWindowSize windowSize)
        {
            _windowSize = windowSize;
            WindowCreateInfo wci = new WindowCreateInfo
            {
                X = 100, //todo: window x & y
                Y = 100,
                WindowWidth = settings.WindowSize.Width,
                WindowHeight = settings.WindowSize.Height,
                WindowTitle = settings.Title
            };

            var options = new GraphicsDeviceOptions
            (
                debug: false,
                swapchainDepthFormat: PixelFormat.R16_UNorm,
                syncToVerticalBlank: settings.Vsync == VsyncMode.On,
                resourceBindingModel: ResourceBindingModel.Improved
            );
#if DEBUG
            options.Debug = true;
#endif
            VeldridStartup.CreateWindowAndGraphicsDevice(wci, options, settings.Backend.Convert(), out _window, out _graphicsDevice);
            _window.Resized += onResized;
            _window.Shown += onShown;
            _window.Closing += () => IsExiting = true;
            GameWindow = _window;
            VeldridGraphics.Device = _graphicsDevice;
            OnInit?.Invoke();
        }

        public static Sdl2Window GameWindow { get; private set; }

        public static Action OnInit { get; set; }

        public double TargetUpdateFrequency
        { 
            get => _targetUpdateFrequency;
            set 
            { 
                _targetUpdateFrequency = value;
                var renderThread = _renderThread;
                if (renderThread != null) renderThread.TargetUpdateFrequency = value;
            }
        }
        public string Title { get => _window.Title; set => _window.Title = value; }
        public VsyncMode Vsync 
        { 
            get => _graphicsDevice.SyncToVerticalBlank ? VsyncMode.On : VsyncMode.Off;
            set => _graphicsDevice.SyncToVerticalBlank = value == VsyncMode.On;
        }

        public API.WindowState WindowState
        {
            get => _windowState;
            set 
            {
                _windowState = value;
                updateWindowState();
            }
        }
        public WindowBorder WindowBorder 
        { 
            get => _hasBorder ? WindowBorder.Resizable : WindowBorder.Hidden;
            set
            {
                //todo: https://github.com/mellinoe/veldrid/issues/131
                if (WindowBorder == WindowBorder.Fixed) throw new NotImplementedException("Fixed window border is not currently implemented");
                _hasBorder = WindowBorder == WindowBorder.Resizable;
                updateWindowState();
            }
        }

        public int Width => _window.Width;

        public int Height => _window.Height;

        public int ClientWidth => Math.Max(1, _windowSize.GetWidth(_window.Width));

        public int ClientHeight => Math.Max(1, _windowSize.GetHeight(_window.Height));

        public bool IsExiting { get; private set; }

        public float AppWindowHeight => ClientHeight;

        public float AppWindowWidth => ClientWidth;

        public API.Rectangle GameSubWindow => _windowSize.GetWindow(new API.Rectangle(0, 0, _window.Bounds.Width, _window.Bounds.Height));

        public event EventHandler<EventArgs> Load;
        public event EventHandler<EventArgs> Resize;
        public event EventHandler<FrameEventArgs> UpdateFrame;
        public event EventHandler<FrameEventArgs> RenderFrame;
#pragma warning disable CS0067
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore CS0067

        public void Dispose(){}

        public void Exit() => _window.Close();

        public void Run(double updateRate)
        {
            _renderThread = new AGSUpdateThread(this);
            _renderThread.UpdateFrame += onRenderFrame;
            _window.Visible = true;
            _renderThread.Run(TargetUpdateFrequency, false);
        }

        public void SetSize(Size size)
        {
            if (!_windowSize.AllowSetSize)
            {
                return;
            }
            _window.Width = size.Width;
            _window.Height = size.Height;
            _resized = true;
        }

        public void SwapBuffers() => _graphicsDevice.SwapBuffers();

        private void onResized()
        {
            _resized = true;
        }

        private void onShown() => Load(this, new EventArgs());

        private void updateWindowState()
        {
            _window.WindowState = _windowState.Convert(_hasBorder);
            _resized = true;
        }

        private void onRenderFrame(object sender, FrameEventArgs e)
        {
            _window.PumpEvents();
            if (!_window.Exists) return;
            if (_resized)
            {
                _resized = false;
                _graphicsDevice.ResizeMainWindow((uint)_window.Width, (uint)_window.Height);
                Resize(this, new EventArgs());
            }
            RenderFrame(this, e);
        }
    }
}