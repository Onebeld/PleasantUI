using Avalonia.Collections;
using PleasantUI.Core;
using PleasantUI.Example.Models;

namespace PleasantUI.Example.ViewModels.Pages.ControlPages;

public class DataGridViewModel : ViewModelBase
{
    private bool _showGridLines;
    private bool _canUserReorderColumns = true;
    private bool _canUserResizeColumns = true;
    private bool _canUserSortColumns = true;
    private bool _showRowDetails;

    public AvaloniaList<DataModel> DataModels { get; }

    public bool ShowGridLines
    {
        get => _showGridLines;
        set => SetProperty(ref _showGridLines, value);
    }

    public bool CanUserReorderColumns
    {
        get => _canUserReorderColumns;
        set => SetProperty(ref _canUserReorderColumns, value);
    }

    public bool CanUserResizeColumns
    {
        get => _canUserResizeColumns;
        set => SetProperty(ref _canUserResizeColumns, value);
    }

    public bool CanUserSortColumns
    {
        get => _canUserSortColumns;
        set => SetProperty(ref _canUserSortColumns, value);
    }

    public bool ShowRowDetails
    {
        get => _showRowDetails;
        set => SetProperty(ref _showRowDetails, value);
    }

    public DataGridViewModel()
    {
        DataModels = new AvaloniaList<DataModel>
        {
            new("Alice Johnson",  "Engineering",  31, 95000,  true,  "Active"),
            new("Bob Smith",      "Marketing",    28, 72000,  false, "Active"),
            new("Carol White",    "Engineering",  35, 110000, false, "On Leave"),
            new("David Brown",    "HR",           42, 68000,  false, "Active"),
            new("Eva Martinez",   "Engineering",  27, 88000,  true,  "Active"),
            new("Frank Lee",      "Marketing",    33, 75000,  false, "Inactive"),
            new("Grace Kim",      "HR",           29, 65000,  true,  "Active"),
            new("Henry Wilson",   "Engineering",  38, 105000, false, "Active"),
            new("Iris Chen",      "Marketing",    26, 70000,  true,  "Active"),
            new("James Taylor",   "HR",           45, 72000,  false, "On Leave"),
            new("Karen Davis",    "Engineering",  32, 98000,  false, "Active"),
            new("Leo Garcia",     "Marketing",    30, 78000,  true,  "Active"),
        };
    }
}
