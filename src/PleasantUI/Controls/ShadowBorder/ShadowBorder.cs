using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using SkiaSharp;

namespace PleasantUI.Controls;

/// <summary>
/// A control class for creating a <see cref="Border"/> with a shadow effect without using <see cref="BoxShadow"/>.
/// </summary>
public class ShadowBorder : Decorator
{
    private WriteableBitmap? _shadowBitmap;
    private Size _lastContentSize;
    private double _lastBlur;
    private Color _lastColor;
    private CornerRadius _lastCorner;
    private Vector _lastOffset;
    
    private Geometry? _cachedClipGeometry;
    private Size _lastClipSize;
    private CornerRadius _lastClipCornerRadius;
    private Thickness _lastClipThickness;
    private Thickness _lastClipPadding;
    
    /// <summary>
    /// Defines the <see cref="ClipContentToBounds"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> IsShadowEnabledProperty =
        AvaloniaProperty.Register<ShadowBorder, bool>(nameof(IsShadowEnabled), true);

    /// <summary>
    /// Defines the <see cref="ShadowColor"/> property.
    /// </summary>
    public static readonly StyledProperty<Color> ShadowColorProperty =
        AvaloniaProperty.Register<ShadowBorder, Color>(nameof(ShadowColor), Colors.Black);

    /// <summary>
    /// Defines the <see cref="BlurRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<double> BlurRadiusProperty =
        AvaloniaProperty.Register<ShadowBorder, double>(nameof(BlurRadius), 12.0);

    /// <summary>
    /// Defines the <see cref="Offset"/> property.
    /// </summary>
    public static readonly StyledProperty<Vector> OffsetProperty =
        AvaloniaProperty.Register<ShadowBorder, Vector>(nameof(Offset), new Vector(0, 4));
    
    /// <summary>
    /// Defines the <see cref="ClipContentToBounds"/> property.
    /// </summary>
    public static readonly StyledProperty<bool> ClipContentToBoundsProperty =
        AvaloniaProperty.Register<ShadowBorder, bool>(nameof(ClipContentToBounds));

    /// <summary>
    /// Defines the <see cref="Background"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BackgroundProperty =
        Border.BackgroundProperty.AddOwner<ShadowBorder>();

    /// <summary>
    /// Defines the <see cref="BorderBrush"/> property.
    /// </summary>
    public static readonly StyledProperty<IBrush?> BorderBrushProperty =
        Border.BorderBrushProperty.AddOwner<ShadowBorder>();

    /// <summary>
    /// Defines the <see cref="BorderThickness"/> property.
    /// </summary>
    public static readonly StyledProperty<Thickness> BorderThicknessProperty =
        Border.BorderThicknessProperty.AddOwner<ShadowBorder>();

    /// <summary>
    /// Defines the <see cref="CornerRadius"/> property.
    /// </summary>
    public static readonly StyledProperty<CornerRadius> CornerRadiusProperty =
        Border.CornerRadiusProperty.AddOwner<ShadowBorder>();
    
    /// <summary>
    /// Gets or sets a value indicating whether to clip content within the borders.
    /// </summary>
    public bool ClipContentToBounds
    {
        get => GetValue(ClipContentToBoundsProperty);
        set => SetValue(ClipContentToBoundsProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow color value.
    /// </summary>
    public Color ShadowColor
    {
        get => GetValue(ShadowColorProperty);
        set => SetValue(ShadowColorProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow blur value.
    /// </summary>
    public double BlurRadius
    {
        get => GetValue(BlurRadiusProperty);
        set => SetValue(BlurRadiusProperty, value);
    }

    /// <summary>
    /// Gets or sets the shadow offset value.
    /// </summary>
    public Vector Offset
    {
        get => GetValue(OffsetProperty);
        set => SetValue(OffsetProperty, value);
    }
    
    /// <summary>
    /// 
    /// </summary>
    public bool IsShadowEnabled
    {
        get => GetValue(IsShadowEnabledProperty);
        set => SetValue(IsShadowEnabledProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush with which to paint the background.
    /// </summary>
    public IBrush? Background
    {
        get => GetValue(BackgroundProperty);
        set => SetValue(BackgroundProperty, value);
    }

    /// <summary>
    /// Gets or sets a brush with which to paint the border.
    /// </summary>
    public IBrush? BorderBrush
    {
        get => GetValue(BorderBrushProperty);
        set => SetValue(BorderBrushProperty, value);
    }

    /// <summary>
    /// Gets or sets the thickness of the border.
    /// </summary>
    public Thickness BorderThickness
    {
        get => GetValue(BorderThicknessProperty);
        set => SetValue(BorderThicknessProperty, value);
    }

    /// <summary>
    /// Gets or sets the radius of the border rounded corners.
    /// </summary>
    public CornerRadius CornerRadius
    {
        get => GetValue(CornerRadiusProperty);
        set => SetValue(CornerRadiusProperty, value);
    }
    
    static ShadowBorder()
    {
        AffectsRender<ShadowBorder>(BackgroundProperty, BorderBrushProperty, BorderThicknessProperty, CornerRadiusProperty, ClipContentToBoundsProperty);
        AffectsMeasure<ShadowBorder>(BorderThicknessProperty, PaddingProperty);
    }
    
    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        Size contentSize = Bounds.Size;

        if (Opacity > 0.001 && IsShadowEnabled && contentSize is { Width: > 0, Height: > 0 })
        {
            EnsureShadowBitmap(contentSize);

            if (_shadowBitmap != null)
            {
                double pad = ComputePad(BlurRadius);
                Size bmpSize = _shadowBitmap.Size;
                Rect destRect = new(-pad + Offset.X, -pad + Offset.Y, bmpSize.Width, bmpSize.Height);
                Rect srcRect = new(0, 0, bmpSize.Width, bmpSize.Height);

                context.DrawImage(_shadowBitmap, srcRect, destRect);
            }
        }

        IBrush? background = Background;
        IBrush? borderBrush = BorderBrush;
        Thickness thickness = BorderThickness;
        CornerRadius cornerRadius = CornerRadius;
        
        Rect rect = new(contentSize);
        Pen? pen = null;
        
        if (borderBrush != null && thickness != default)
        {
            pen = new Pen(borderBrush, thickness.Left); 
            rect = rect.Deflate(thickness * 0.5);
        }

        if (background != null || pen != null)
        {
            if (cornerRadius == default)
                context.DrawRectangle(background, pen, rect);
            else
            {
                RoundedRect roundedRect = new(rect, cornerRadius);
                context.DrawRectangle(background, pen, roundedRect);
            }
        }

        base.Render(context);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ChildProperty)
        {
            if (change.OldValue is Control oldChild)
                oldChild.Clip = null;
            
            InvalidateClipCache();
            UpdateClipGeometry(Bounds.Size);
        }

        if (change.Property == ClipContentToBoundsProperty ||
            change.Property == BorderThicknessProperty ||
            change.Property == PaddingProperty ||
            change.Property == WidthProperty ||
            change.Property == HeightProperty)
        {
            InvalidateClipCache();
            UpdateClipGeometry(Bounds.Size);
        }

        if (change.Property == OffsetProperty)
        {
            InvalidateVisual();
            return;
        }

        if (change.Property == ShadowColorProperty ||
            change.Property == CornerRadiusProperty ||
            change.Property == IsEnabledProperty ||
            change.Property == BlurRadiusProperty)
        {
            InvalidateShadowCache();
            InvalidateVisual();
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size availableSize)
    {
        return LayoutHelper.MeasureChild(Child, availableSize, Padding, BorderThickness);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        Size contentSize = Child?.Bounds.Size ?? finalSize;
        if (!SizesEqual(contentSize, _lastContentSize))
            InvalidateShadowCache();

        Size result = LayoutHelper.ArrangeChild(Child, finalSize, Padding, BorderThickness);
        
        UpdateClipGeometry(finalSize);

        return result;
    }
    
    /// <inheritdoc/>
    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        _shadowBitmap?.Dispose();
        _shadowBitmap = null;
        _cachedClipGeometry = null;
    }
    
    private void UpdateClipGeometry(Size finalSize)
    {
        Control? child = Child;
        if (child == null) return;

        if (!ClipContentToBounds || finalSize.Width <= 0 || finalSize.Height <= 0)
        {
            child.Clip = null;
            return;
        }

        Thickness thickness = BorderThickness;
        Thickness padding = Padding;
        CornerRadius cornerRadius = CornerRadius;

        if (_cachedClipGeometry != null &&
            SizesEqual(finalSize, _lastClipSize) &&
            cornerRadius.Equals(_lastClipCornerRadius) &&
            thickness.Equals(_lastClipThickness) &&
            padding.Equals(_lastClipPadding))
        {
            child.Clip = _cachedClipGeometry;
            return;
        }

        double x = -padding.Left;
        double y = -padding.Top;
        double w = finalSize.Width - thickness.Left - thickness.Right;
        double h = finalSize.Height - thickness.Top - thickness.Bottom;

        if (w <= 0 || h <= 0)
        {
            _cachedClipGeometry = new RectangleGeometry(new Rect(0, 0, 0, 0));
            child.Clip = _cachedClipGeometry;
            return;
        }

        Rect clipRect = new(x, y, w, h);

        if (cornerRadius != default)
        {
            CornerRadius innerCorners = AdjustCornerRadius(cornerRadius, thickness);
            
            innerCorners = NormalizeCornerRadius(innerCorners, new Size(w, h));
            
            StreamGeometry geometry = new();
            using (StreamGeometryContext context = geometry.Open())
            {
                context.BeginFigure(new Point(clipRect.X + innerCorners.TopLeft, clipRect.Y));
                context.LineTo(new Point(clipRect.Right - innerCorners.TopRight, clipRect.Y));
                context.ArcTo(new Point(clipRect.Right, clipRect.Y + innerCorners.TopRight), new Size(innerCorners.TopRight, innerCorners.TopRight), 0, false, SweepDirection.Clockwise);
                context.LineTo(new Point(clipRect.Right, clipRect.Bottom - innerCorners.BottomRight));
                context.ArcTo(new Point(clipRect.Right - innerCorners.BottomRight, clipRect.Bottom), new Size(innerCorners.BottomRight, innerCorners.BottomRight), 0, false, SweepDirection.Clockwise);
                context.LineTo(new Point(clipRect.X + innerCorners.BottomLeft, clipRect.Bottom));
                context.ArcTo(new Point(clipRect.X, clipRect.Bottom - innerCorners.BottomLeft), new Size(innerCorners.BottomLeft, innerCorners.BottomLeft), 0, false, SweepDirection.Clockwise);
                context.LineTo(new Point(clipRect.X, clipRect.Y + innerCorners.TopLeft));
                context.ArcTo(new Point(clipRect.X + innerCorners.TopLeft, clipRect.Y), new Size(innerCorners.TopLeft, innerCorners.TopLeft), 0, false, SweepDirection.Clockwise);
                context.EndFigure(true);
            }
            _cachedClipGeometry = geometry;
        }
        else
        {
            _cachedClipGeometry = new RectangleGeometry(clipRect);
        }

        _lastClipSize = finalSize;
        _lastClipCornerRadius = cornerRadius;
        _lastClipThickness = thickness;
        _lastClipPadding = padding;

        child.Clip = _cachedClipGeometry;
    }
    
    private static CornerRadius AdjustCornerRadius(CornerRadius corner, Thickness thickness)
    {
        return new CornerRadius(
            Math.Max(0, corner.TopLeft - thickness.Left),
            Math.Max(0, corner.TopRight - thickness.Right),
            Math.Max(0, corner.BottomRight - thickness.Right),
            Math.Max(0, corner.BottomLeft - thickness.Left)
        );
    }
    
    private static CornerRadius NormalizeCornerRadius(CornerRadius corner, Size size)
    {
        double topLeft = corner.TopLeft;
        double topRight = corner.TopRight;
        double bottomLeft = corner.BottomLeft;
        double bottomRight = corner.BottomRight;

        double topRadiusSum = topLeft + topRight;
        double bottomRadiusSum = bottomLeft + bottomRight;
    
        double leftRadiusSum = topLeft + bottomLeft;
        double rightRadiusSum = topRight + bottomRight;

        double maxScale = 1.0;

        if (topRadiusSum > size.Width)
            maxScale = Math.Min(maxScale, size.Width / topRadiusSum);
    
        if (bottomRadiusSum > size.Width)
            maxScale = Math.Min(maxScale, size.Width / bottomRadiusSum);

        if (leftRadiusSum > size.Height)
            maxScale = Math.Min(maxScale, size.Height / leftRadiusSum);

        if (rightRadiusSum > size.Height)
            maxScale = Math.Min(maxScale, size.Height / rightRadiusSum);

        if (maxScale < 1.0)
        {
            topLeft *= maxScale;
            topRight *= maxScale;
            bottomLeft *= maxScale;
            bottomRight *= maxScale;
        }

        return new CornerRadius(topLeft, topRight, bottomRight, bottomLeft);
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

    private WriteableBitmap CreateShadowBitmap(Size contentSizeDip, double blurDip, Color color, CornerRadius corner)
    {
        double scale = 1.0;
        if (TopLevel.GetTopLevel(this) is { } topLevel)
            scale = topLevel.RenderScaling;

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

        WriteableBitmap wbmp = new(
            new PixelSize(pixelW, pixelH),
            new Vector(96 * scale, 96 * scale),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

        using ILockedFramebuffer lockedBuffer = wbmp.Lock();
        SKImageInfo info = new(pixelW, pixelH, SKColorType.Bgra8888, SKAlphaType.Premul);
            
        using SKSurface? surface = SKSurface.Create(info, lockedBuffer.Address, lockedBuffer.RowBytes);
        
        SKCanvas? canvas = surface.Canvas;
        canvas.Clear(SKColors.Transparent);

        SKColor skColor = new(color.R, color.G, color.B, color.A);

        using SKPaint paint = new();
        paint.IsAntialias = true;
        paint.ImageFilter = SKImageFilter.CreateDropShadowOnly(0, 0, sigma, sigma, skColor);

        SKRect rect = new(padPx, padPx, padPx + contentWpx, padPx + contentHpx);

        float radiusPx = (float)(corner.TopLeft * scale);
        if (radiusPx > 0.001f)
            canvas.DrawRoundRect(rect, radiusPx, radiusPx, paint);
        else
            canvas.DrawRect(rect, paint);
                
        canvas.Flush();

        return wbmp;
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
    
    private void InvalidateClipCache()
    {
        _cachedClipGeometry = null;
        _lastClipSize = default;
    }

    private static bool SizesEqual(Size a, Size b)
        => Math.Abs(a.Width - b.Width) < 0.5 && Math.Abs(a.Height - b.Height) < 0.5;
}