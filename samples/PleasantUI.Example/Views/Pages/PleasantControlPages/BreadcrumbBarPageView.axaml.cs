using System.Collections.ObjectModel;
using Avalonia.Interactivity;
using PleasantUI.Controls;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Views.Pages.PleasantControlPages;

public partial class BreadcrumbBarPageView : LocalizedUserControl
{
    // Backing collection for the bound demo.
    private readonly ObservableCollection<string> _path = ["Home", "Documents"];

    private static readonly string[] _segments =
        ["Projects", "PleasantUI", "Controls", "BreadcrumbBar", "Samples", "Demo"];

    private int _pushIndex;

    public BreadcrumbBarPageView()
    {
        InitializeComponent();
        WireHandlers();
    }

    protected override void ReinitializeComponent()
    {
        InitializeComponent();
        WireHandlers();
    }

    private void WireHandlers()
    {
        // Detach stale handlers first.
        BasicBreadcrumb.ItemClicked        -= OnBasicItemClicked;
        ClickableLastBreadcrumb.ItemClicked -= OnClickableLastItemClicked;
        PushButton.Click                   -= OnPush;
        PopButton.Click                    -= OnPop;
        ResetButton.Click                  -= OnReset;

        // Attach fresh handlers.
        BasicBreadcrumb.ItemClicked        += OnBasicItemClicked;
        ClickableLastBreadcrumb.ItemClicked += OnClickableLastItemClicked;
        PushButton.Click                   += OnPush;
        PopButton.Click                    += OnPop;
        ResetButton.Click                  += OnReset;

        // Bind the observable collection.
        BoundBreadcrumb.ItemsSource = _path;
        _pushIndex = _path.Count;
    }

    // ── Event handlers ────────────────────────────────────────────────────────

    private void OnBasicItemClicked(object? sender, BreadcrumbBarItemClickedEventArgs e)
        => BasicClickResult.Text = $"{e.Item}  (index {e.Index})";

    private void OnClickableLastItemClicked(object? sender, BreadcrumbBarItemClickedEventArgs e)
        => ClickableLastResult.Text = $"{e.Item}  (index {e.Index})";

    private void OnPush(object? sender, RoutedEventArgs e)
    {
        if (_pushIndex < _segments.Length)
            _path.Add(_segments[_pushIndex++]);
    }

    private void OnPop(object? sender, RoutedEventArgs e)
    {
        if (_path.Count > 1)
        {
            _path.RemoveAt(_path.Count - 1);
            _pushIndex = Math.Max(_pushIndex - 1, _path.Count);
        }
    }

    private void OnReset(object? sender, RoutedEventArgs e)
    {
        _path.Clear();
        _path.Add("Home");
        _path.Add("Documents");
        _pushIndex = _path.Count;
    }
}
