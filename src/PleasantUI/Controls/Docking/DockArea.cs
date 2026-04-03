using Avalonia;
using Avalonia.Controls;

namespace PleasantUI.Controls.Docking;

/// <summary>
/// Represents a dock area within the docking system.
/// </summary>
public class DockArea : AvaloniaObject
{
    /// <summary>
    /// Defines the <see cref="Location"/> property.
    /// </summary>
    public static readonly StyledProperty<DockAreaLocation> LocationProperty =
        AvaloniaProperty.Register<DockArea, DockAreaLocation>(nameof(Location));

    /// <summary>
    /// Defines the <see cref="View"/> property.
    /// </summary>
    public static readonly StyledProperty<IDockAreaView?> ViewProperty =
        AvaloniaProperty.Register<DockArea, IDockAreaView?>(nameof(View));

    /// <summary>
    /// Defines the <see cref="SideBar"/> property.
    /// </summary>
    public static readonly StyledProperty<SideBar?> SideBarProperty =
        AvaloniaProperty.Register<DockArea, SideBar?>(nameof(SideBar));

    /// <summary>
    /// Defines the <see cref="Target"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> TargetProperty =
        AvaloniaProperty.Register<DockArea, string?>(nameof(Target));

    /// <summary>
    /// Defines the <see cref="LocalizedName"/> property.
    /// </summary>
    public static readonly StyledProperty<string?> LocalizedNameProperty =
        AvaloniaProperty.Register<DockArea, string?>(nameof(LocalizedName));

    /// <summary>
    /// Gets or sets the localized name of the dock area.
    /// </summary>
    public string? LocalizedName
    {
        get => GetValue(LocalizedNameProperty);
        set => SetValue(LocalizedNameProperty, value);
    }

    /// <summary>
    /// Gets or sets the target property name.
    /// </summary>
    public string? Target
    {
        get => GetValue(TargetProperty);
        set => SetValue(TargetProperty, value);
    }

    /// <summary>
    /// Gets or sets the view associated with this dock area.
    /// </summary>
    [ResolveByName]
    public IDockAreaView? View
    {
        get => GetValue(ViewProperty);
        set => SetValue(ViewProperty, value);
    }

    /// <summary>
    /// Gets or sets the sidebar associated with this dock area.
    /// </summary>
    [ResolveByName]
    public SideBar? SideBar
    {
        get => GetValue(SideBarProperty);
        set => SetValue(SideBarProperty, value);
    }

    /// <summary>
    /// Gets or sets the location of this dock area.
    /// </summary>
    public DockAreaLocation Location
    {
        get => GetValue(LocationProperty);
        set => SetValue(LocationProperty, value);
    }

    /// <inheritdoc/>
    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == ViewProperty)
        {
            if (change.OldValue is IDockAreaView oldView)
            {
                oldView.OnDetachedFromDockArea(this);
            }

            if (change.NewValue is IDockAreaView newView)
            {
                newView.OnAttachedToDockArea(this);
            }
        }
    }
}
