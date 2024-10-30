using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace PleasantUI.Controls;

/// <summary>
/// A control that blurs the background.
/// </summary>
/// <remarks>
/// This control uses the Skia API to blur the background.
///
/// Reference: https://github.com/rocksdanister/weather/blob/main/src/Drizzle.UI.Avalonia/UserControls/BackdropBlurControl.cs
/// </remarks>
public class BackdropBlurControl : Control
{
    /// <summary>
    /// Defines the <see cref="Material"/> property.
    /// </summary>
    public static readonly StyledProperty<ExperimentalAcrylicMaterial?> MaterialProperty = 
        AvaloniaProperty.Register<BackdropBlurControl, ExperimentalAcrylicMaterial?>(nameof(Material));

    /// <summary>
    /// Gets or sets the material.
    /// </summary>
    public ExperimentalAcrylicMaterial? Material
    {
        get => GetValue(MaterialProperty);
        set => SetValue(MaterialProperty, value);
    }

    static BackdropBlurControl()
    {
        AffectsRender<BackdropBlurControl>(MaterialProperty);
    }
    
    // Same as #10ffffff for UWP CardBackgroundBrush.
    private static readonly ImmutableExperimentalAcrylicMaterial DefaultAcrylicMaterial = (ImmutableExperimentalAcrylicMaterial)new ExperimentalAcrylicMaterial
    {
        MaterialOpacity = 0.1,
        TintColor = Colors.White,
        TintOpacity = 0.1
    }.ToImmutable();

    class BlurBehindRenderOperation : ICustomDrawOperation
    {
        private readonly ImmutableExperimentalAcrylicMaterial _material;
        private readonly Rect _bounds;

        public BlurBehindRenderOperation(ImmutableExperimentalAcrylicMaterial material, Rect bounds)
        {
            _material = material;
            _bounds = bounds;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => _bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            ISkiaSharpApiLeaseFeature? leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            
            if (leaseFeature == null)
                return;
            
            using ISkiaSharpApiLease lease = leaseFeature.Lease();

            if (!lease.SkCanvas.TotalMatrix.TryInvert(out SKMatrix currentInvertedTransform))
                return;

            using SKImage? backgroundSnapshot = lease.SkSurface?.Snapshot();
            using SKShader? backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp,
                SKShaderTileMode.Clamp, currentInvertedTransform);

            using SKSurface? blurred = SKSurface.Create(lease.GrContext, false, new SKImageInfo(
                (int)Math.Ceiling(_bounds.Width),
                (int)Math.Ceiling(_bounds.Height), SKImageInfo.PlatformColorType, SKAlphaType.Premul));

            using (SKImageFilter filter = SKImageFilter.CreateBlur(10, 10, SKShaderTileMode.Clamp))
            using (SKPaint blurPaint = new())
            {
                blurPaint.Shader = backdropShader;
                blurPaint.ImageFilter = filter;
                blurred.Canvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurPaint);
            }

            using (SKImage? blurSnap = blurred.Snapshot())
            using (SKShader? blurSnapShader = SKShader.CreateImage(blurSnap))
            using (SKPaint blurSnapPaint = new())
            {
                blurSnapPaint.Shader = blurSnapShader;
                blurSnapPaint.IsAntialias = false;
                lease.SkCanvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, blurSnapPaint);
            }

            using SKPaint acrylicPaint = new();
            acrylicPaint.IsAntialias = false;

            Color tintColor = _material.TintColor;
            SKColor tint = new(tintColor.R, tintColor.G, tintColor.B, tintColor.A);

            using (SKShader? backdrop = SKShader.CreateColor(new SKColor(_material.MaterialColor.R, _material.MaterialColor.G, _material.MaterialColor.B, _material.MaterialColor.A)))
            using (SKShader? tintShader = SKShader.CreateColor(tint))
            using (SKShader? effectiveTint = SKShader.CreateCompose(backdrop, tintShader))
            {
                acrylicPaint.Shader = effectiveTint;
                acrylicPaint.IsAntialias = false;
                lease.SkCanvas.DrawRect(0, 0, (float)_bounds.Width, (float)_bounds.Height, acrylicPaint);
            }
        }

        public Rect Bounds => _bounds.Inflate(4);

        public bool Equals(ICustomDrawOperation? other)
        {
            return other is BlurBehindRenderOperation op && op._bounds == _bounds && op._material.Equals(_material);
        }
    }

    /// <inheritdoc/>
    public override void Render(DrawingContext context)
    {
        ImmutableExperimentalAcrylicMaterial mat = Material != null
            ? (ImmutableExperimentalAcrylicMaterial)Material.ToImmutable()
            : DefaultAcrylicMaterial;
        context.Custom(new BlurBehindRenderOperation(mat, new Rect(default, Bounds.Size)));
    }
}