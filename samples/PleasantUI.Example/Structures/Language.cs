namespace PleasantUI.Example.Structures;

public readonly struct Language(string name, string key)
{
    public string Name { get; } = name;
    
    public string Key { get; } = key;
}