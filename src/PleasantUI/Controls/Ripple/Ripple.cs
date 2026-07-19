/*
 * SPDX-FileCopyrightText: 2026 Dmitry Zhutkov (Onebeld) <onebeld@gmail.com>
 * SPDX-FileCopyrightText: 2023 AvaloniaCommunity <https://github.com/AvaloniaCommunity>
 * SPDX-License-Identifier: MIT
 *
 * Derived from:
 * https://github.com/AvaloniaCommunity/Material.Avalonia/blob/master/Material.Ripple/Ripple.cs
 */

using Avalonia.Animation.Easings;

namespace PleasantUI.Controls;

/// <summary>
/// Provides static properties for configuring ripple effects.
/// </summary>
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