using Avalonia.Controls;

namespace PleasantUI.Extensions;

/// <summary>
/// Provides extension methods for working with <see cref="ResourceDictionary"/> objects.
/// </summary>
public static class ResourceDictionaryExtensions
{
	/// <summary>
	/// Converts a <see cref="Dictionary{TKey, TValue}"/> to a <see cref="ResourceDictionary"/>.
	/// </summary>
	/// <typeparam name="T1">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="T2">The type of the values in the dictionary.</typeparam>
	/// <param name="dictionary">The dictionary to convert.</param>
	/// <returns>A new <see cref="ResourceDictionary"/> containing the key-value pairs from the input dictionary.</returns>
	public static ResourceDictionary ToResourceDictionary<T1, T2>(this Dictionary<T1, T2> dictionary) where T1 : notnull
	{
		ResourceDictionary resourceDictionary = new();

		foreach (KeyValuePair<T1,T2> pair in dictionary) 
			resourceDictionary.Add(pair.Key, pair.Value);

		return resourceDictionary;
	}

	/// <summary>
	/// Converts a <see cref="ResourceDictionary"/> to a <see cref="Dictionary{TKey, TValue}"/>.
	/// </summary>
	/// <typeparam name="T1">The type of the keys in the dictionary.</typeparam>
	/// <typeparam name="T2">The type of the values in the dictionary.</typeparam>
	/// <param name="resourceDictionary">The resource dictionary to convert.</param>
	/// <returns>A new <see cref="Dictionary{TKey, TValue}"/> containing the key-value pairs from the input resource dictionary.</returns>
	public static Dictionary<T1, T2> ToDictionary<T1, T2>(this ResourceDictionary resourceDictionary) where T1 : notnull
	{
		Dictionary<T1, T2> dictionary = new();

		foreach (KeyValuePair<object,object?> pair in resourceDictionary) 
			dictionary.Add((T1)pair.Key, (T2)pair.Value);

		return dictionary;
	}
}