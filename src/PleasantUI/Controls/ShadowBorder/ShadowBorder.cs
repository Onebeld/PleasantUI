using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace PleasantUI.Controls;

public class ShadowBorder : Decorator
{
    private double _lastBlur;
    private Bitmap? _shadowBitmap;
    private Size _lastContentSize;
    private Vector _lastOffset;
    private Color _lastColor;
    private CornerRadius _lastCorner;
    
    public static readonly StyledProperty<Color> ShadowColorProperty =
        AvaloniaProperty.Register<ShadowBorder, Color>(nameof(ShadowColor), Colors.Black);

    public static readonly StyledProperty<double> BlurRadiusProperty =
        AvaloniaProperty.Register<ShadowBorder, double>(nameof(BlurRadius), 12.0);

    public static readonly StyledProperty<Vector> OffsetProperty =
        AvaloniaProperty.Register<ShadowBorder, Vector>(nameof(Offset), new Vector(0, 4));

    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        AvaloniaProperty.Register<ShadowBorder, CornerRadius>(nameof(CornerRadius), new CornerRadius(0));

    public Color ShadowColor
    {
        get => GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }

    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    public Vector Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }

    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }

    /// <inheritdoc />
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property != ShadowColorProperty &&
            change.Property != BlurRadiusProperty &&
            change.Property != OffsetProperty &&
            change.Property != CornerRadiusProperty)
            return;
        
        InvalidateShadowCache();
        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        Size contentSize = Child?.Bounds.Size ?? finalSize;
        if (!SizesEqual(contentSize, _lastContentSize))
            InvalidateShadowCache();

        return base.ArrangeOverride(finalSize);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        Size contentSize = Child?.Bounds.Size ?? Bounds.Size;
        EnsureShadowBitmap(contentSize);

        if (_shadowBitmap != null)
        {
            double pad = ComputePad(BlurRadius);
            Size bmpSize = _shadowBitmap.Size;
            Rect destRect = new(-pad + Offset.X, -pad + Offset.Y, bmpSize.Width, bmpSize.Height);
            Rect srcRect = new(0, 0, bmpSize.Width, bmpSize.Height);

            context.DrawImage(_shadowBitmap, srcRect, destRect);
        }

        base.Render(context);
    }

    private void EnsureShadowBitmap(Size contentSize)
    {
        if (contentSize.Width <= 0 || contentSize.Height <= 0)
            return;

        if (_shadowBitmap != null &&
            SizesEqual(contentSize, _lastContentSize) &&
            Math.Abs(_lastBlur - BlurRadius) < 0.001 &&
            _lastColor == ShadowColor &&
            _lastOffset == Offset &&
            _lastCorner.Equals(CornerRadius))
            return;

        _shadowBitmap?.Dispose();
        _shadowBitmap = CreateShadowBitmap(contentSize, BlurRadius, ShadowColor, CornerRadius);
        _lastContentSize = contentSize;
        _lastBlur = BlurRadius;
        _lastColor = ShadowColor;
        _lastOffset = Offset;
        _lastCorner = CornerRadius;
    }

    private Bitmap? CreateShadowBitmap(Size contentSizeDip, double blurDip, Color color, CornerRadius corner)
    {
        double scale = 1.0;
        if (VisualRoot is { } renderRoot)
            scale = renderRoot.RenderScaling;

        double padDip = ComputePad(blurDip);
        double totalWidthDip = contentSizeDip.Width + padDip * 2;
        double totalHeightDip = contentSizeDip.Height + padDip * 2;

        int pixelW = Math.Max(1, (int)Math.Ceiling(totalWidthDip * scale));
        int pixelH = Math.Max(1, (int)Math.Ceiling(totalHeightDip * scale));

        float contentWpx = (float)(contentSizeDip.Width * scale);
        float contentHpx = (float)(contentSizeDip.Height * scale);
        float padPx = (float)(padDip * scale);

        float blurPx = (float)(blurDip * scale);
        float sigma = blurPx * 0.57735f + 0.5f;

        SKImageInfo info = new(pixelW, pixelH, SKColorType.Bgra8888, SKAlphaType.Premul);
        using SKBitmap skBitmap = new(info);
        using (SKCanvas canvas = new(skBitmap))
        {
            canvas.Clear(SKColors.Transparent);

            SKColor skColor = new(color.R, color.G, color.B, color.A);

            using SKPaint paint = new();
            paint.IsAntialias = true;
            paint.Color = SKColors.White;

            using SKImageFilter? filter = SKImageFilter.CreateDropShadowOnly(
                0, 0,
                sigma, sigma,
                skColor);

            paint.ImageFilter = filter;

            SKRect rect = new(padPx, padPx, padPx + contentWpx, padPx + contentHpx);

            float radiusPx = (float)(corner.TopLeft * scale);
            if (radiusPx > 0.001f)
                canvas.DrawRoundRect(rect, radiusPx, radiusPx, paint);
            else
                canvas.DrawRect(rect, paint);
        }

        IntPtr pixelsPtr = skBitmap.GetPixels();
        PixelSize pixelSize = new(skBitmap.Width, skBitmap.Height);
        Vector dpi = new(96.0 * scale, 96.0 * scale);
        int stride = skBitmap.RowBytes;

        try
        {
            Bitmap avaloniaBitmap = new(
                PixelFormat.Bgra8888,
                AlphaFormat.Premul,
                pixelsPtr,
                pixelSize,
                dpi,
                stride);

            return avaloniaBitmap;
        }
        catch
        {
            return null;
        }
    }

    private static double ComputePad(double blur)
    {
        return Math.Ceiling(blur * 2.0);
    }

    private void InvalidateShadowCache()
    {
        _shadowBitmap?.Dispose();
        _shadowBitmap = null;
        _lastContentSize = default;
    }

    /// <inheritdoc />
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _shadowBitmap?.Dispose();
        _shadowBitmap = null;
    }

    private static bool SizesEqual(Size a, Size b)
        => Math.Abs(a.Width - b.Width) < 0.5 && Math.Abs(a.Height - b.Height) < 0.5;
}