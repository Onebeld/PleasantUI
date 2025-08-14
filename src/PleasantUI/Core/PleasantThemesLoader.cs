using System.Text;
using Avalonia.Media;
using PleasantUI.Core.Constants;
using PleasantUI.Core.Models;

namespace PleasantUI.Core;

/// <summary>
/// Provides methods for saving and loading PleasantUI themes.
/// </summary>
public static class PleasantThemesLoader
{
    private const uint MagicNumber = 0x4C474E41;

    private const byte MajorVersion = 1;
    private const byte MinorVersion = 0;

    private static readonly string PathToThemes = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, PleasantFileNames.Themes);

    /// <summary>
    /// Saves the current custom themes to a file.
    /// </summary>
    public static void Save()
    {
        EnsureFilePathFolderExists(PathToThemes);
        
        using FileStream fileStream = new(PathToThemes, FileMode.Create, FileAccess.Write);
        using BinaryWriter writer = new(fileStream, Encoding.ASCII);

        WriteVersion(writer);

        int themesCount = PleasantTheme.CustomThemes.Count;
        
        Dictionary<string, Color> themeTemplateDictionary = PleasantTheme.GetThemeTemplateDictionary();
        
        int keysCount = themeTemplateDictionary.Keys.Count;

        writer.Write(themesCount);
        writer.Write(keysCount);

        if (themesCount == 0)
            return;

        WriteColorKeys(writer, themeTemplateDictionary);

        WriteCustomThemes(writer);
    }

    /// <summary>
    /// Loads custom themes from a file.
    /// </summary>
    /// <returns>An array of loaded custom themes, or an empty array if the file does not exist or is invalid.</returns>
    /// <exception cref="InvalidDataException">
    /// Thrown if the file is not a valid PleasantUI theme file or if the file version
    /// is incompatible.
    /// </exception>
    public static CustomTheme[] Load()
    {
        if (!File.Exists(PathToThemes))
            return [];

        using FileStream fileStream = new(PathToThemes, FileMode.Open, FileAccess.Read);
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
    
    private static void EnsureFilePathFolderExists(string path)
    {
        string? folder = Path.GetDirectoryName(path);
        if (string.IsNullOrEmpty(folder))
            throw new ArgumentException("Failed to get the directory path", nameof(path));

        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
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

            colorKeys[i] = Encoding.ASCII.GetString(reader.ReadBytes(length));
        }

        return colorKeys;
    }

    private static CustomTheme[] ReadCustomThemes(
        BinaryReader reader,
        string[] colorKeys,
        int keysCount,
        int themesCount)
    {
        CustomTheme[] customThemes = new CustomTheme[themesCount];

        for (int i = 0; i < themesCount; i++)
        {
            byte[] guidBytes = reader.ReadBytes(16);
            Guid id = new(guidBytes);

            int nameLength = reader.ReadByte();
            byte[] buffer = reader.ReadBytes(nameLength * 2);
            string themeName = Encoding.Unicode.GetString(buffer);

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
            writer.Write(Encoding.ASCII.GetBytes(key));
        }
    }

    private static void WriteCustomThemes(BinaryWriter writer)
    {
        foreach (CustomTheme customTheme in PleasantTheme.CustomThemes)
        {
            writer.Write(customTheme.Id.ToByteArray());

            string themeName = customTheme.Name;

            writer.Write((byte)themeName.Length);
            writer.Write(Encoding.Unicode.GetBytes(themeName));

            foreach (KeyValuePair<string, Color> pair in customTheme.Colors)
                writer.Write(pair.Value.ToUInt32());
        }
    }

    #endregion
}