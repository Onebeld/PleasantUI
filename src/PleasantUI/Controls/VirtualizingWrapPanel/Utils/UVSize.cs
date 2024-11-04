using Avalonia.Layout;

namespace PleasantUI.Controls.Utils;

/// <remarks>
/// Reference: https://github.com/afunc233/BilibiliClient/blob/599d7451e9187e7a967cbbb0cdbbd6a428493672/src/BilibiliClient/Controls/UVSize.cs
/// </remarks>
internal struct UvSize
{
    internal UvSize(Orientation orientation, double width, double height)
    {
        U = V = 0d;
        Orientation = orientation;
        Width = width;
        Height = height;
    }

    internal UvSize(Orientation orientation)
    {
        U = V = 0d;
        Orientation = orientation;
    }

    internal double U;
    internal double V;
    internal Orientation Orientation;
    internal bool IsNaN => double.IsNaN(U) || double.IsNaN(V);

    internal double Width
    {
        get => Orientation == Orientation.Horizontal ? U : V;
        set
        {
            if (Orientation == Orientation.Horizontal) U = value;
            else V = value;
        }
    }

    internal double Height
    {
        get => Orientation == Orientation.Horizontal ? V : U;
        set
        {
            if (Orientation == Orientation.Horizontal) V = value;
            else U = value;
        }
    }
}