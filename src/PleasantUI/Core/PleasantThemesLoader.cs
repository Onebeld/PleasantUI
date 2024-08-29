using System.Text;
using Avalonia.Media;
using PleasantUI.Core.Constants;
using PleasantUI.Core.Models;
using PleasantUI.Core.Structures;

namespace PleasantUI.Core;

public static class PleasantThemesLoader
{
	private const uint MagicNumber = 0x4C474E41;

	private const byte MajorVersion = 1;
	private const byte MinorVersion = 0;

	private static readonly string Path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PleasantFileNames.Themes);

	public static void Save()
	{
		using FileStream fileStream = new(Path, FileMode.Create, FileAccess.Write);
		using BinaryWriter writer = new(fileStream, Encoding.ASCII);

		WriteVersion(writer);

		Dictionary<string, Color> themeTemplateDictionary = PleasantTheme.GetThemeTemplateDictionary();

		int themesCount = PleasantTheme.CustomThemes.Count;
		int keysCount = themeTemplateDictionary.Keys.Count;

		writer.Write(themesCount);
		writer.Write(keysCount);

		if (themesCount == 0)
			return;
		
		WriteColorKeys(writer, themeTemplateDictionary);

		WriteCustomThemes(writer);
	}

	public static CustomTheme[] Load()
	{
		if (!File.Exists(Path))
			return [];
		
		using FileStream fileStream = new(Path, FileMode.Open, FileAccess.Read);
		using BinaryReader reader = new(fileStream, Encoding.ASCII);

		uint magicNumber = reader.ReadUInt32();

		if (magicNumber != MagicNumber)
			throw new InvalidDataException();

		Version version = ReadVersion(reader);

		if (version < new Version(MajorVersion, MinorVersion))
			throw new InvalidDataException();

		int themesCount = reader.ReadInt32();
		int keysCount = reader.ReadInt32();
		
		if (themesCount == 0)
			return [];

		string[] colorKeys = ReadColorKeys(reader, keysCount);
		CustomTheme[] customThemes = ReadCustomThemes(reader, colorKeys, keysCount, themesCount);

		return customThemes;
	}
	
	#region Read

	private static Version ReadVersion(BinaryReader reader)
	{
		byte majorVersion = reader.ReadByte();
		byte minorVersion = reader.ReadByte();
		
		return new Version(majorVersion, minorVersion);
	}
	
	private static string[] ReadColorKeys(BinaryReader reader, int keysCount)
	{
		string[] colorKeys = new string[keysCount];
		
		for (int i = 0; i < keysCount; i++)
		{
			int length = reader.ReadByte();
			
			string key = "";
			for (int j = 0; j < length; j++)
				key += (char)reader.ReadInt16();
			
			colorKeys[i] = key;
		}

		return colorKeys;
	}

	private static CustomTheme[] ReadCustomThemes(BinaryReader reader, string[] colorKeys, int keysCount, int themesCount)
	{
		CustomTheme[] customThemes = new CustomTheme[themesCount];
		
		for (int i = 0; i < themesCount; i++)
		{
			byte[] guidBytes = reader.ReadBytes(16);
			Guid id = new(guidBytes);
			
			int nameLength = reader.ReadByte();

			string themeName = "";
			for (int j = 0; j < nameLength; j++)
				themeName += (char)reader.ReadInt16();

			Dictionary<string, Color> dictionary = new();

			for (int j = 0; j < keysCount; j++)
			{
				string key = colorKeys[j];
				Color color = Color.FromUInt32(reader.ReadUInt32());
				
				dictionary.Add(key, color);
			}

			customThemes[i] = new CustomTheme(id, themeName, dictionary);
		}

		return customThemes;
	}

	#endregion

	#region Write
	
	private static void WriteVersion(BinaryWriter writer)
	{
		writer.Write(MagicNumber);
		writer.Write(MajorVersion);
		writer.Write(MinorVersion);
	}
	
	private static void WriteColorKeys(BinaryWriter writer, Dictionary<string, Color> themeTemplateDictionary)
	{
		foreach (KeyValuePair<string, Color> pair in themeTemplateDictionary)
		{
			string key = pair.Key;
			
			writer.Write((byte)key.Length);
			foreach (char c in key) 
				writer.Write((short)c);
		}
	}
	
	private static void WriteCustomThemes(BinaryWriter writer)
	{
		foreach (CustomTheme customTheme in PleasantTheme.CustomThemes)
		{
			writer.Write(customTheme.Id.ToByteArray());
			
			string themeName = customTheme.Name;
			
			writer.Write((byte)themeName.Length);
			foreach (char c in themeName) 
				writer.Write((short)c);

			foreach (KeyValuePair<string, Color> pair in customTheme.Colors)
				writer.Write(pair.Value.ToUInt32());
		}
	}
	
	#endregion
}