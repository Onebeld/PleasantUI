using Avalonia.Animation.Easings;

namespace PleasantUI.Controls;

/// <summary>
/// Provides static properties for configuring ripple effects.
/// </summary>
/// <remarks>
/// Reference: https://github.com/AvaloniaCommunity/Material.Avalonia/blob/master/Material.Ripple/Ripple.cs
/// </remarks>
public static class Ripple
{
	/// <summary>
	/// Gets or sets the easing function used for ripple animations.
	/// </summary>
	/// <remarks>
	/// Defaults to <see cref="CircularEaseOut" />.
	/// </remarks>
	public static Easing Easing { get; set; } = new CircularEaseOut();

	/// <summary>
	/// Gets or sets the duration of ripple animations.
	/// </summary>
	/// <remarks>
	/// Defaults to 800 milliseconds.
	/// </remarks>
	public static TimeSpan Duration { get; set; } = new(0, 0, 0, 0, 800);
}