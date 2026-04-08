using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Styling;
using PleasantUI.Controls;
using PleasantUI.Core.Enums;
using PleasantUI.Core.Extensions;
using PleasantUI.Core.Interfaces;
using PleasantUI.Core.Localization;
using PleasantUI.Core.Structures;

namespace PleasantUI.ToolKit;

/// <summary>
/// A rich dialog control with support for header icons, subheader text, command items
/// (radio buttons, checkboxes, command links), a progress bar, an expandable footer,
/// and full Opening/Closing event lifecycle.
/// </summary>
public sealed partial class PleasantDialog : ContentDialog
{
    // Height of the progress bar wrapper when visible: 8px bar + 12px bottom margin
    private const double ProgressBarVisibleHeight = 20.0;

    private Button? _defaultButton;
    private object _result = PleasantDialogResult.None;
    private Action<PleasantDialog>? _onDialogReady;

    private PleasantDialog() => InitializeComponent();

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised before the dialog is shown.</summary>
    public event EventHandler? Opening;

    /// <summary>Raised after the dialog is closed.</summary>
    public event EventHandler? Opened;

    /// <summary>
    /// Raised when the dialog is about to close.
    /// Set <see cref="PleasantDialogClosingEventArgs.Cancel"/> to prevent closing.
    /// </summary>
    public event EventHandler<PleasantDialogClosingEventArgs>? Closing;

    // ── Lifecycle ─────────────────────────────────────────────────────────────

    /// <inheritdoc />
    protected override void OnLoaded(Avalonia.Interactivity.RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_onDialogReady is not null)
        {
            var callback = _onDialogReady;
            _onDialogReady = null;
            // Post at Loaded priority so layout has completed and Bounds are valid.
            Avalonia.Threading.Dispatcher.UIThread.Post(
                () => callback(this),
                Avalonia.Threading.DispatcherPriority.Loaded);
        }
    }

    // ── Static factory ────────────────────────────────────────────────────────

    /// <summary>
    /// Shows a <see cref="PleasantDialog"/> and returns the result.
    /// </summary>
    /// <param name="parent">The parent window that hosts the dialog.</param>
    /// <param name="header">The dialog title text or localization key.</param>
    /// <param name="body">Optional body text or localization key.</param>
    /// <param name="buttons">Optional button definitions. Defaults to a single OK button.</param>
    /// <param name="commands">Optional command items (radio buttons, checkboxes, command links).</param>
    /// <param name="iconGeometryKey">Optional resource key for a header icon geometry.</param>
    /// <param name="headerBackground">Optional background brush for the header area.</param>
    /// <param name="iconForeground">Optional foreground brush for the header icon.</param>
    /// <param name="subHeader">Optional subheader text or localization key.</param>
    /// <param name="extraContent">Optional arbitrary control placed in the body.</param>
    /// <param name="onContentReady">Called with <paramref name="extraContent"/> after it is attached.</param>
    /// <param name="onDialogReady">
    /// Called with the dialog instance after it is fully laid out.
    /// Use this to hold a reference for calling <see cref="SetProgressBarState"/> from a background task.
    /// </param>
    /// <param name="footer">Optional footer content.</param>
    /// <param name="footerExpandable">When true the footer is collapsed behind a toggle button.</param>
    /// <param name="footerToggleText">Label for the footer toggle button.</param>
    /// <param name="style">Visual style variant (e.g. Danger).</param>
    public static Task<object> Show(
        IPleasantWindow parent,
        string header,
        string? body = null,
        IReadOnlyList<PleasantDialogButton>? buttons = null,
        IReadOnlyList<PleasantDialogCommand>? commands = null,
        string? iconGeometryKey = null,
        IBrush? headerBackground = null,
        IBrush? iconForeground = null,
        string? subHeader = null,
        Control? extraContent = null,
        Action<Control>? onContentReady = null,
        Action<PleasantDialog>? onDialogReady = null,
        object? footer = null,
        bool footerExpandable = false,
        string? footerToggleText = null,
        MessageBoxStyle style = MessageBoxStyle.Default)
    {
        var dialog = new PleasantDialog();

        dialog.Opening?.Invoke(dialog, EventArgs.Empty);

        // ── Header ────────────────────────────────────────────────────────────
        string headerValue = Localizer.Instance.TryGetString(header, out string rh) ? rh : header;

        var headerBorder  = dialog.FindControl<Border>("HeaderBorder")!;
        var headerText    = dialog.FindControl<TextBlock>("HeaderText")!;
        var subHeaderText = dialog.FindControl<TextBlock>("SubHeaderText")!;
        var headerIcon    = dialog.FindControl<PathIcon>("HeaderIcon")!;

        headerText.Text = headerValue;

        if (PleasantUI.Core.PleasantSettings.Current?.Theme == "VGUI")
        {
            dialog.CornerRadius    = new CornerRadius(0);
            headerBorder.CornerRadius = new CornerRadius(0);
        }

        if (!string.IsNullOrWhiteSpace(subHeader))
        {
            subHeaderText.Text      = Localizer.Instance.TryGetString(subHeader, out string rs) ? rs : subHeader;
            subHeaderText.IsVisible = true;
        }

        if (iconGeometryKey is not null &&
            Application.Current!.TryFindResource(iconGeometryKey, out object? geo) &&
            geo is Geometry geometry)
        {
            headerIcon.Data      = geometry;
            headerIcon.IsVisible = true;
            if (iconForeground is not null)
                headerIcon.Foreground = iconForeground;
        }

        if (style == MessageBoxStyle.Danger)
        {
            bool isVgui = PleasantUI.Core.PleasantSettings.Current?.Theme == "VGUI";
            if (isVgui)
            {
                headerBorder.Background = new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint   = new RelativePoint(0, 1, RelativeUnit.Relative),
                    GradientStops =
                    {
                        new GradientStop(Color.Parse("#FFE84030"), 0),
                        new GradientStop(Color.Parse("#FFB72B1D"), 0.5),
                        new GradientStop(Color.Parse("#FF8B1A10"), 1),
                    }
                };
                headerBorder.BorderThickness = new Thickness(0, 2, 0, 2);
                headerBorder.BorderBrush     = new SolidColorBrush(Color.Parse("#FF8B1A10"));
                headerBorder.CornerRadius    = new CornerRadius(0);
            }
            else
            {
                headerBorder.Background = Application.Current!.TryFindResource("DangerColor", out object? dc)
                    ? new SolidColorBrush((Color)dc!)
                    : Brushes.Red;
            }
            headerText.Foreground    = Brushes.White;
            subHeaderText.Foreground = Brushes.White;
            headerIcon.Foreground    = Brushes.White;
        }
        else if (headerBackground is not null)
        {
            headerBorder.Background = headerBackground;
        }

        // ── Body text ─────────────────────────────────────────────────────────
        var bodyText = dialog.FindControl<TextBlock>("BodyText")!;
        if (!string.IsNullOrWhiteSpace(body))
        {
            bodyText.Text      = Localizer.Instance.TryGetString(body, out string rb) ? rb : body;
            bodyText.IsVisible = true;
        }

        // ── Commands ──────────────────────────────────────────────────────────
        var commandsHost = dialog.FindControl<ItemsControl>("CommandsHost")!;
        if (commands is { Count: > 0 })
        {
            var items = new List<Control>();
            foreach (var cmd in commands)
            {
                string cmdText = Localizer.Instance.TryGetString(cmd.Text, out string rc) ? rc : cmd.Text;

                switch (cmd)
                {
                    case PleasantDialogRadioButton rb:
                    {
                        var radio = new RadioButton
                        {
                            Content   = cmdText,
                            IsChecked = rb.IsChecked,
                            IsEnabled = rb.IsEnabled,
                            Tag       = rb
                        };
                        radio.IsCheckedChanged += (_, _) => rb.IsChecked = radio.IsChecked == true;
                        items.Add(radio);
                        break;
                    }
                    case PleasantDialogCheckBox cb:
                    {
                        var check = new CheckBox
                        {
                            Content   = cmdText,
                            IsChecked = cb.IsChecked,
                            IsEnabled = cb.IsEnabled,
                            Tag       = cb
                        };
                        check.IsCheckedChanged += (_, _) => cb.IsChecked = check.IsChecked == true;
                        items.Add(check);
                        break;
                    }
                    case PleasantDialogCommandLink cl:
                    {
                        var btn = new Button
                        {
                            Theme = Application.Current!.TryFindResource(
                                "PleasantDialogCommandLinkTheme", out object? t)
                                ? t as ControlTheme : null,
                            IsEnabled = cl.IsEnabled,
                            Tag       = cl
                        };
                        var panel = new StackPanel { Spacing = 2 };
                        panel.Children.Add(new TextBlock { Text = cmdText, FontWeight = FontWeight.SemiBold });
                        if (!string.IsNullOrWhiteSpace(cl.Description))
                            panel.Children.Add(new TextBlock
                            {
                                Text       = cl.Description,
                                FontSize   = 12,
                                Foreground = Application.Current!.TryFindResource(
                                    "TextFillColor2", out object? tf)
                                    ? new SolidColorBrush((Color)tf!)
                                    : Brushes.Gray
                            });
                        btn.Content = panel;
                        btn.Click += (_, _) =>
                        {
                            cl.RaiseClick();
                            if (cl.ClosesOnInvoked)
                            {
                                dialog._result = cl.DialogResult;
                                _ = dialog.CloseAsync();
                            }
                        };
                        items.Add(btn);
                        break;
                    }
                }
            }
            commandsHost.ItemsSource = items;
            commandsHost.IsVisible   = true;
        }

        // ── Extra content ─────────────────────────────────────────────────────
        var extraSlot = dialog.FindControl<ContentPresenter>("ExtraContent")!;
        if (extraContent is not null)
        {
            extraSlot.Content   = extraContent;
            extraSlot.IsVisible = true;
            onContentReady?.Invoke(extraContent);
        }

        // ── Footer ────────────────────────────────────────────────────────────
        if (footer is not null)
        {
            var footerBorder  = dialog.FindControl<Border>("FooterBorder")!;
            var footerContent = dialog.FindControl<ContentPresenter>("FooterContent")!;
            var toggleButton  = dialog.FindControl<Button>("FooterToggleButton")!;
            var toggleText    = dialog.FindControl<TextBlock>("FooterToggleText")!;
            var chevron       = dialog.FindControl<PathIcon>("FooterChevron")!;

            footerContent.Content  = footer;
            footerBorder.IsVisible = true;

            if (PleasantUI.Core.PleasantSettings.Current?.Theme == "VGUI")
                footerBorder.CornerRadius = new CornerRadius(0);

            if (footerExpandable)
            {
                toggleButton.IsVisible  = true;
                toggleText.Text         = footerToggleText ?? "Details";
                footerContent.IsVisible = false;

                toggleButton.Click += (_, _) =>
                {
                    bool expanded = footerContent.IsVisible = !footerContent.IsVisible;
                    chevron.Data = Application.Current!.TryFindResource(
                        expanded ? "ChevronUpRegular" : "ChevronDownRegular", out object? g)
                        ? g as Geometry : null;
                };
            }
            else
            {
                footerContent.IsVisible = true;
            }
        }

        // ── Buttons ───────────────────────────────────────────────────────────
        var buttonsHost = dialog.FindControl<UniformGrid>("ButtonsHost")!;

        void AddButton(PleasantDialogButton mb)
        {
            string value = Localizer.Instance.TryGetString(mb.Text, out string rv) ? rv : mb.Text;

            var button = new Button
            {
                Content           = value,
                IsEnabled         = mb.IsEnabled,
                Margin            = Thickness.Parse("5"),
                VerticalAlignment = VerticalAlignment.Center
            };

            button.Click += (_, _) =>
            {
                mb.RaiseClick();
                mb.Command?.Execute(mb.CommandParameter);

                var args = new PleasantDialogClosingEventArgs(mb.DialogResult);
                dialog.Closing?.Invoke(dialog, args);
                if (args.Cancel) return;

                dialog._result = mb.DialogResult;
                _ = dialog.CloseAsync();
            };

            if (mb.ThemeKey is not null)
                button.Theme = ResourceExtensions.GetResource<ControlTheme>(mb.ThemeKey);
            else if (mb.IsDefault)
            {
                button.Theme = style == MessageBoxStyle.Danger
                    ? ResourceExtensions.GetResource<ControlTheme>("DangerButtonTheme")
                    : ResourceExtensions.GetResource<ControlTheme>("AccentButtonTheme");
                dialog._defaultButton = button;
                dialog._result        = mb.DialogResult;
            }

            buttonsHost.Children.Add(button);
        }

        if (buttons is null || buttons.Count == 0)
        {
            buttonsHost.Columns = 2;
            buttonsHost.Children.Add(new Panel());
            AddButton(new PleasantDialogButton
            {
                Text         = Localizer.TrDefault("Ok", "OK"),
                DialogResult = PleasantDialogResult.OK,
                IsDefault    = true
            });
        }
        else if (buttons.Count == 1)
        {
            buttonsHost.Columns = 2;
            buttonsHost.Children.Add(new Panel());
            AddButton(buttons[0]);
        }
        else
        {
            buttonsHost.Columns = buttons.Count;
            foreach (var btn in buttons)
                AddButton(btn);
        }

        // Enter / Escape key handling
        dialog.KeyDown += (_, e) =>
        {
            if (e.Key == Key.Enter && dialog._defaultButton is { IsEnabled: true })
            {
                dialog._defaultButton.Focus();
                var args = new PleasantDialogClosingEventArgs(dialog._result);
                dialog.Closing?.Invoke(dialog, args);
                if (!args.Cancel)
                    _ = dialog.CloseAsync();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape)
            {
                dialog._result = PleasantDialogResult.Cancel;
                _ = dialog.CloseAsync();
                e.Handled = true;
            }
        };

        // ── Show ──────────────────────────────────────────────────────────────
        var tcs = new TaskCompletionSource<object>();

        dialog.Closed += (_, _) =>
        {
            dialog.Opened?.Invoke(dialog, EventArgs.Empty);
            tcs.TrySetResult(dialog._result);
        };

        dialog._onDialogReady = onDialogReady;
        dialog.ShowAsync(parent);
        return tcs.Task;
    }

    /// <summary>
    /// Shows or updates the progress bar. Safe to call from any thread.
    /// On first call the wrapper animates open; subsequent calls update value/state.
    /// </summary>
    /// <param name="value">Progress value (0–100).</param>
    /// <param name="isIndeterminate">When true the bar shows an indeterminate animation.</param>
    /// <param name="isError">When true the bar foreground switches to the error color.</param>
    public void SetProgressBarState(double value, bool isIndeterminate = false, bool isError = false)
    {
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var wrapper = this.FindControl<Border>("ProgressBarWrapper")!;
            var pb      = this.FindControl<ProgressBar>("ProgressBar")!;

            // Animate the wrapper open the first time.
            if (wrapper.Height < ProgressBarVisibleHeight)
                wrapper.Height = ProgressBarVisibleHeight;

            pb.IsIndeterminate = isIndeterminate;
            pb.Value           = value;

            if (isError && Application.Current!.TryFindResource("SystemFillColorCritical", out object? ec))
                pb.Foreground = new SolidColorBrush((Color)ec!);
            else
                pb.ClearValue(ProgressBar.ForegroundProperty);

        }, Avalonia.Threading.DispatcherPriority.Render);
    }
}
