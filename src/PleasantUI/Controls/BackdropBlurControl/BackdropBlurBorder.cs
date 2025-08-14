using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using Avalonia.Threading;
using SkiaSharp;

namespace PleasantUI.Controls;

// Reference: https://github.com/rocksdanister/weather/blob/main/src/Drizzle.UI.Avalonia/UserControls/BackdropBlurControl.cs

/// <summary>
/// A control that blurs the background.
/// </summary>
/// <remarks>
/// This control uses the Skia API to blur the background.
/// </remarks>
public class BackdropBlurBorder : Decorator
{
    /// <summary>
    /// Defines the <see cref="TintOpacity"/> property.
    /// </summary>
    public static readonly StyledProperty<double> TintOpacityProperty =
        AvaloniaProperty.Register<BackdropBlurBorder, double>(nameof(TintOpacity), defaultValue: 0.5);

    /// <summary>
    /// Defines the <see cref="BlurRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BlurRadiusProperty =
        AvaloniaProperty.Register<BackdropBlurBorder, double>(nameof(BlurRadius), defaultValue: 15);
    
    /// <summary>
    /// Defines the <see cref="BackgroundProperty"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(Background));
    
    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        AvaloniaProperty.Register<Border, IBrush?>(nameof(BorderBrush));

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        AvaloniaProperty.Register<Border, Thickness>(nameof(BorderThickness));

    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<Border, CornerRadius>(nameof(CornerRadius));
    
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }
    
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }
    
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    public double TintOpacity
    {
        get => GetValue(TintOpacityProperty);
        set => SetValue(TintOpacityProperty, value);
    }

    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }
    
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    static BackdropBlurBorder()
    {
        AffectsRender<BackdropBlurBorder>(
            BackgroundProperty,
            BlurRadiusProperty,
            TintOpacityProperty,
            CornerRadiusProperty,
            BorderThicknessProperty,
            BorderBrushProperty);
        
        AffectsMeasure<BackdropBlurBorder>(BorderThicknessProperty);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        context.Custom(new BlurBehindRenderOperation(Background, TintOpacity, BlurRadius,
            new Rect(default, Bounds.Size), GetMaxCornerRadius(CornerRadius), GetMaxThickness(BorderThickness),
            BorderBrush));

        // We update every frame because there are artifacts when animating the color of the controls (sharp lines).
        // This is the best we could find, but expensive
        Dispatcher.UIThread.InvokeAsync(InvalidateVisual, DispatcherPriority.Background);
    }

    /// <summary>
    /// Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
    }

    /// <summary>
    /// Arranges the control's child.
    /// </summary>
    /// <param name="finalSize">The size allocated to the control.</param>
    /// <returns>The space taken.</returns>
    protected override Size ArrangeOverride(Size finalSize)
    {
        return LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
    }

    private double GetMaxThickness(Thickness thickness)
    {
        double max = thickness.Left;

        max = Math.Max(max, thickness.Bottom);
        max = Math.Max(max, thickness.Right);
        max = Math.Max(max, thickness.Top);

        return max;
    }

    private double GetMaxCornerRadius(CornerRadius radius)
    {
        double max = radius.BottomLeft;

        max = Math.Max(max, radius.BottomRight);
        max = Math.Max(max, radius.TopLeft);
        max = Math.Max(max, radius.TopRight);

        return max;
    }

    private class BlurBehindRenderOperation : ICustomDrawOperation
    {
        private readonly IBrush? _background;
        private readonly float _tinyOpacity;
        private readonly float _blurRadius;
        private readonly Rect _bounds;
        private readonly float _cornerRadius;
        private readonly float _borderThickness;
        private readonly IBrush? _borderColor;

        public BlurBehindRenderOperation(
            IBrush? background,
            double tintOpacity,
            double blurRadius,
            Rect bounds,
            double cornerRadius,
            double borderThickness,
            IBrush? borderColor)
        {
            _background = background;
            _tinyOpacity = (float)tintOpacity;
            _blurRadius = (float)blurRadius;
            _bounds = bounds;
            _cornerRadius = (float)cornerRadius;
            _borderThickness = (float)borderThickness;
            _borderColor = borderColor;
        }
        
        public bool HitTest(Point p) => _bounds.Contains(p);

        public Rect Bounds => _bounds.Inflate(4);
        
        public void Dispose() { }

        public void Render(ImmediateDrawingContext context)
        {
            if (_background is not ISolidColorBrush background)
                return;

            ISkiaSharpApiLeaseFeature? leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
                return;

            using ISkiaSharpApiLease lease = leaseFeature.Lease();

            if (!lease.SkCanvas.TotalMatrix.TryInvert(out SKMatrix currentInvertedTransform))
                return;

            using SKImage? backgroundSnapshot = lease.SkSurface?.Snapshot();
            if (backgroundSnapshot == null)
                return;

            using SKShader? backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp, currentInvertedTransform);

            int width = Math.Max(1, (int)Math.Ceiling(_bounds.Width));
            int height = Math.Max(1, (int)Math.Ceiling(_bounds.Height));

            using SKSurface? blurred = SKSurface.Create(lease.GrContext, false,
                new SKImageInfo(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul));
            if (blurred == null)
                return;

            using (SKImageFilter? filter = SKImageFilter.CreateBlur(_blurRadius, _blurRadius, SKShaderTileMode.Clamp))
            using (SKPaint blurPaint = new())
            {
                blurPaint.Shader = backdropShader;
                blurPaint.ImageFilter = filter;
                blurPaint.IsAntialias = true;

                blurred.Canvas.Clear(SKColors.Transparent);
                blurred.Canvas.DrawRect(0, 0, width, height, blurPaint);
            }

            float halfBorderThickness = _borderThickness * 0.5f;

            using SKImage? blurSnap = blurred.Snapshot();

            SKRect outerRect = new(_borderThickness, _borderThickness, width - _borderThickness,
                height - _borderThickness);

            using SKRoundRect outerRRect = new(outerRect, _cornerRadius - halfBorderThickness);

            lease.SkCanvas.Save();

            using (SKPath clipPath = new())
            {
                clipPath.AddRoundRect(outerRRect);
                lease.SkCanvas.ClipPath(clipPath, SKClipOperation.Intersect, true);
            }

            using (SKShader? blurSnapShader = SKShader.CreateImage(blurSnap))
            using (SKPaint blurSnapPaint = new())
            {
                blurSnapPaint.Shader = blurSnapShader;
                blurSnapPaint.IsAntialias = true;
                lease.SkCanvas.DrawRect(0, 0, width, height, blurSnapPaint);
            }

            using SKPaint acrylicPaint = new();
            acrylicPaint.IsAntialias = true;

            Color tintColor = background.Color;
            SKColor tint = new(tintColor.R, tintColor.G, tintColor.B, (byte)(tintColor.A * _tinyOpacity));
            acrylicPaint.Color = tint;

            lease.SkCanvas.DrawRect(0, 0, width, height, acrylicPaint);

            if (DrawBorder(lease, halfBorderThickness, width, height)) return;

            lease.SkCanvas.Restore();
        }
        
        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op
                   && op._bounds == _bounds
                   && op._cornerRadius.Equals(_cornerRadius)
                   && op._borderThickness.Equals(_borderThickness)
                   && op._borderColor != null
                   && op._borderColor.Equals(_borderColor)
                   && op._background != null
                   && op._background.Equals(_background)
                   && op._blurRadius.Equals(_blurRadius)
                   && op._tinyOpacity.Equals(_tinyOpacity);
        }

        private bool DrawBorder(ISkiaSharpApiLease lease, float halfBorderThickness, int width, int height)
        {
            if (_borderColor is not ISolidColorBrush brush || _borderThickness == 0)
                return true;

            lease.SkCanvas.Restore();

            Color color = brush.Color;
            SKColor skBorder = new(color.R, color.G, color.B, color.A);

            SKRect innerRect = new(halfBorderThickness, halfBorderThickness, width - halfBorderThickness,
                height - halfBorderThickness);

            using SKRoundRect innerRRect = new(innerRect, _cornerRadius);

            using SKPaint clearPaint = new();
            clearPaint.Style = SKPaintStyle.Stroke;
            clearPaint.Color = skBorder;
            clearPaint.StrokeWidth = _borderThickness;
            clearPaint.IsAntialias = true;

            lease.SkCanvas.DrawRoundRect(innerRRect, clearPaint);

            return false;
        }
    }
}