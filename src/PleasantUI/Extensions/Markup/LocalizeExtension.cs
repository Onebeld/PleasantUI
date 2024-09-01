﻿using Avalonia.Data;
using Avalonia.Data.Core;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Xaml.MarkupExtensions.CompiledBindings;
using PleasantUI.Converters;
using PleasantUI.Localization;

namespace PleasantUI.Extensions.Markup;

/// <summary>
/// A markup extension that provides localized strings.
/// </summary>
public class LocalizeExtension : MarkupExtension
{
	private readonly BindingBase[] _bindings = [];
	
	/// <summary>
	/// Gets or sets the key of the localized string.
	/// </summary>
	public string Key { get; set; }

	/// <summary>
	/// Gets or sets the context of the localized string.
	/// </summary>
	public string? Context { get; set; }

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizeExtension"/> class with the specified key.
	/// </summary>
	/// <param name="key">The key of the localized string.</param>
	public LocalizeExtension(string key)
	{
		Key = key;
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizeExtension"/> class with the specified key and a binding.
	/// </summary>
	/// <param name="key">The key of the localized string.</param>
	/// <param name="binding">The binding to use for string formatting.</param>
	public LocalizeExtension(string key, BindingBase binding) : this(key)
	{
		_bindings = [binding];
	}
	
	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizeExtension"/> class with the specified key and two bindings.
	/// </summary>
	/// <param name="key">The key of the localized string.</param>
	/// <param name="binding1">The first binding to use for string formatting.</param>
	/// <param name="binding2">The second binding to use for string formatting.</param>
	public LocalizeExtension(string key, BindingBase binding1, BindingBase binding2) : this(key)
	{
		_bindings = [binding1, binding2];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizeExtension"/> class with the specified key and three bindings.
	/// </summary>
	/// <param name="key">The key of the localized string.</param>
	/// <param name="binding1">The first binding to use for string formatting.</param>
	/// <param name="binding2">The second binding to use for string formatting.</param>
	/// <param name="binding3">The third binding to use for string formatting.</param>
	public LocalizeExtension(string key, BindingBase binding1, BindingBase binding2, BindingBase binding3) : this(key)
	{
		_bindings = [binding1, binding2, binding3];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizeExtension"/> class with the specified key and four bindings.
	/// </summary>
	/// <param name="key">The key of the localized string.</param>
	/// <param name="binding1">The first binding to use for string formatting.</param>
	/// <param name="binding2">The second binding to use for string formatting.</param>
	/// <param name="binding3">The third binding to use for string formatting.</param>
	/// <param name="binding4">The fourth binding to use for string formatting.</param>
	public LocalizeExtension(string key, BindingBase binding1, BindingBase binding2, BindingBase binding3, BindingBase binding4) : this(key)
	{
		_bindings = [binding1, binding2, binding3, binding4];
	}

	/// <summary>
	/// Initializes a new instance of the <see cref="LocalizeExtension"/> class with the specified key and five bindings.
	/// </summary>
	/// <param name="key">The key of the localized string.</param>
	/// <param name="binding1">The first binding to use for string formatting.</param>
	/// <param name="binding2">The second binding to use for string formatting.</param>
	/// <param name="binding3">The third binding to use for string formatting.</param>
	/// <param name="binding4">The fourth binding to use for string formatting.</param>
	/// <param name="binding5">The fifth binding to use for string formatting.</param>
	public LocalizeExtension(string key, BindingBase binding1, BindingBase binding2, BindingBase binding3, BindingBase binding4, BindingBase binding5) : this(key)
	{
		_bindings = [binding1, binding2, binding3, binding4, binding5];
	}

	/// <summary>
	/// Provides the localized string.
	/// </summary>
	/// <param name="serviceProvider">The service provider.</param>
	/// <returns>The localized string.</returns>
	public override object ProvideValue(IServiceProvider serviceProvider)
	{
		string keyToUse = Key;
		if (!string.IsNullOrWhiteSpace(Context))
			keyToUse = $"{Context}/{Key}";

		ClrPropertyInfo keyInfo = new(
			nameof(Key),
			_ => Localizer.Instance[keyToUse],
			null,
			typeof(string));

		CompiledBindingPath path = new CompiledBindingPathBuilder()
			.Property(keyInfo, PropertyInfoAccessorFactory.CreateInpcPropertyAccessor)
			.Build();

		CompiledBindingExtension binding = new(path)
		{
			Mode = BindingMode.OneWay,
			Source = Localizer.Instance
		};

		if (_bindings.Length <= 0)
			return binding;
		
		BindingBase[] bindingBases = [binding];

		MultiBinding multiBinding = new()
		{
			Bindings = bindingBases.Concat(_bindings).ToArray(),
			Converter = new TranslateConverter()
		};

		return multiBinding;
	}
}