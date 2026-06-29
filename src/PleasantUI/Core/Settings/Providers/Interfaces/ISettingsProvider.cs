namespace PleasantUI.Core.Settings.Providers.Interfaces;

internal interface ISettingsProvider<T> where T : new()
{
    T Load(string path);

    void Save(T? settings, string path);
}