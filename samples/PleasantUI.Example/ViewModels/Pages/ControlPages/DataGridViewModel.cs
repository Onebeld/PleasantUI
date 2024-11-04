using Avalonia.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using PleasantUI.Example.Models;

namespace PleasantUI.Example.ViewModels.Pages.ControlPages;

public class DataGridViewModel : ObservableObject
{
    public AvaloniaList<DataModel> DataModels { get; }

    public DataGridViewModel()
    {
        DataModels = new AvaloniaList<DataModel>
        {
            new("John", 23, true),
            new("Jane", 24, false),
            new("Jack", 25, true),
            new("Jill", 26, false),
            new("Joe", 27, true),
        };
    }
}