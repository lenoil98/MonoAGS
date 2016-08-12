﻿namespace AGS.API
{
    public interface IHasImage
    {
        byte Opacity { get; set; }
        Color Tint { get; set; }
        PointF Anchor { get; set; }

        IImage Image { get; set; }
        IImageRenderer CustomRenderer { get; set; }

        IEvent<AGSEventArgs> OnImageChanged { get; }
    }

    public interface IImageComponent : IHasImage, IComponent
    {
    }
}