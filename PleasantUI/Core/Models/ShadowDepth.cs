using Avalonia.Media;
using Color = System.Drawing.Color;

namespace PleasantUI.Core.Models;

public class ShadowDepth
{
    public double OffsetX { get; set; }
    public double OffsetY { get; set; }
    public double Blur { get; set; }
    public double Spread { get; set; }
    public Color Color { get; set; }
    public bool IsInset { get; set; }
}