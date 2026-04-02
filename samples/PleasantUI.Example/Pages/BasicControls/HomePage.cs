using Avalonia.Controls;
using PleasantUI.Example.Views.Pages;

namespace PleasantUI.Example.Pages.BasicControls;

public class HomePage : LocalizedPage
{
    public override string TitleKey { get; } = "Home";
    public override bool ShowTitle { get; } = false;

    // Lazily created once per HomePage instance — a new HomePage means a new HomePageView
    // with fresh LocalizeKeyObservable bindings, which is exactly what we want on language change.
    private HomePageView? _view;
    public override Control Content => _view ??= new HomePageView();
}
