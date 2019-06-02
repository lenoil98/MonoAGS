﻿using AGS.API;

namespace AGS.Engine
{
	public class AGSGameSettings : IGameSettings
	{
		public AGSGameSettings(string title, AGS.API.Size virtualResolution, WindowState windowState = WindowState.Maximized,
               AGS.API.Size? windowSize = null, VsyncMode vsync = VsyncMode.On, bool preserveAspectRatio = true,
               WindowBorder windowBorder = WindowBorder.Resizable, GraphicsBackend? backend = null)
		{
            Title = title;
            VirtualResolution = virtualResolution;
            WindowState = windowState;
            WindowSize = windowSize.HasValue ? windowSize.Value : virtualResolution;
            Vsync = vsync;
            PreserveAspectRatio = preserveAspectRatio;
            WindowBorder = windowBorder;
            Backend = backend ?? AGSGame.Device.GraphicsBackend.AutoDetect();
            var fonts = new AGSDefaultFonts();
            var dialogs = new AGSDialogSettings(AGSGame.Device, fonts);
            Defaults = new AGSDefaultsSettings(fonts, dialogs);
		}

        public string Title { get; }
        
        public Size VirtualResolution { get; }

        public VsyncMode Vsync { get; }

        public Size WindowSize { get; }

        public bool PreserveAspectRatio { get; }

        public WindowState WindowState { get; }

        public WindowBorder WindowBorder { get; }

        public IDefaultsSettings Defaults { get; }

        public GraphicsBackend Backend { get; }
    }
}
