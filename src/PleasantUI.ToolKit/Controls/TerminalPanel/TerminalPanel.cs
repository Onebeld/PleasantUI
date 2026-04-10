using System.Text;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;

namespace PleasantUI.ToolKit.Controls;

/// <summary>
/// A terminal-style panel with a scrollable output area and a command input line.
/// Maintains a configurable output buffer and raises events for command submission.
/// </summary>
[TemplatePart(PART_OutputBox,    typeof(TextBox))]
[TemplatePart(PART_InputBox,     typeof(TextBox))]
[TemplatePart(PART_ClearButton,  typeof(Button))]
[TemplatePart(PART_CloseButton,  typeof(Button))]
[TemplatePart(PART_ScrollViewer, typeof(SmoothScrollViewer))]
[PseudoClasses(PC_Running, PC_HasOutput)]
public class TerminalPanel : TemplatedControl
{
    // ── Template part names ───────────────────────────────────────────────────

    internal const string PART_OutputBox    = "PART_OutputBox";
    internal const string PART_InputBox     = "PART_InputBox";
    internal const string PART_ClearButton  = "PART_ClearButton";
    internal const string PART_CloseButton  = "PART_CloseButton";
    internal const string PART_ScrollViewer = "PART_ScrollViewer";

    // ── Pseudo-class names ────────────────────────────────────────────────────

    private const string PC_Running   = ":running";
    private const string PC_HasOutput = ":hasOutput";

    // ── Styled properties ─────────────────────────────────────────────────────

    /// <summary>Defines the <see cref="Title"/> property.</summary>
    public static readonly StyledProperty<string> TitleProperty =
        AvaloniaProperty.Register<TerminalPanel, string>(nameof(Title), defaultValue: "Terminal");

    /// <summary>Defines the <see cref="IsRunning"/> property.</summary>
    public static readonly StyledProperty<bool> IsRunningProperty =
        AvaloniaProperty.Register<TerminalPanel, bool>(nameof(IsRunning));

    /// <summary>Defines the <see cref="StatusText"/> property.</summary>
    public static readonly StyledProperty<string?> StatusTextProperty =
        AvaloniaProperty.Register<TerminalPanel, string?>(nameof(StatusText));

    /// <summary>Defines the <see cref="StatusColor"/> property.</summary>
    public static readonly StyledProperty<IBrush?> StatusColorProperty =
        AvaloniaProperty.Register<TerminalPanel, IBrush?>(nameof(StatusColor));

    /// <summary>Defines the <see cref="InputText"/> property.</summary>
    public static readonly StyledProperty<string?> InputTextProperty =
        AvaloniaProperty.Register<TerminalPanel, string?>(nameof(InputText));

    /// <summary>Defines the <see cref="OutputText"/> property.</summary>
    public static readonly StyledProperty<string?> OutputTextProperty =
        AvaloniaProperty.Register<TerminalPanel, string?>(nameof(OutputText));

    /// <summary>Defines the <see cref="Prompt"/> property.</summary>
    public static readonly StyledProperty<string> PromptProperty =
        AvaloniaProperty.Register<TerminalPanel, string>(nameof(Prompt), defaultValue: "$");

    /// <summary>Defines the <see cref="MaxOutputLines"/> property.</summary>
    public static readonly StyledProperty<int> MaxOutputLinesProperty =
        AvaloniaProperty.Register<TerminalPanel, int>(nameof(MaxOutputLines), defaultValue: 5000);

    /// <summary>Defines the <see cref="ShowCloseButton"/> property.</summary>
    public static readonly StyledProperty<bool> ShowCloseButtonProperty =
        AvaloniaProperty.Register<TerminalPanel, bool>(nameof(ShowCloseButton), defaultValue: true);

    // ── CLR accessors ─────────────────────────────────────────────────────────

    /// <summary>Gets or sets the panel title shown in the header.</summary>
    public string Title
    {
        get => GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    /// <summary>Gets or sets whether the terminal session is active.</summary>
    public bool IsRunning
    {
        get => GetValue(IsRunningProperty);
        set => SetValue(IsRunningProperty, value);
    }

    /// <summary>Gets or sets the status text shown in the header.</summary>
    public string? StatusText
    {
        get => GetValue(StatusTextProperty);
        set => SetValue(StatusTextProperty, value);
    }

    /// <summary>Gets or sets the brush used for the status indicator dot.</summary>
    public IBrush? StatusColor
    {
        get => GetValue(StatusColorProperty);
        set => SetValue(StatusColorProperty, value);
    }

    /// <summary>Gets or sets the current input line text.</summary>
    public string? InputText
    {
        get => GetValue(InputTextProperty);
        set => SetValue(InputTextProperty, value);
    }

    /// <summary>Gets or sets the full output text displayed in the terminal.</summary>
    public string? OutputText
    {
        get => GetValue(OutputTextProperty);
        set => SetValue(OutputTextProperty, value);
    }

    /// <summary>Gets or sets the prompt string shown before the input box.</summary>
    public string Prompt
    {
        get => GetValue(PromptProperty);
        set => SetValue(PromptProperty, value);
    }

    /// <summary>Gets or sets the maximum number of output lines to retain.</summary>
    public int MaxOutputLines
    {
        get => GetValue(MaxOutputLinesProperty);
        set => SetValue(MaxOutputLinesProperty, value);
    }

    /// <summary>Gets or sets whether the close button is visible.</summary>
    public bool ShowCloseButton
    {
        get => GetValue(ShowCloseButtonProperty);
        set => SetValue(ShowCloseButtonProperty, value);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    /// <summary>Raised when the user submits a command (presses Enter).</summary>
    public event EventHandler<string>? CommandSubmitted;

    /// <summary>Raised when the user clicks the close button.</summary>
    public event EventHandler? CloseRequested;

    // ── Private state ─────────────────────────────────────────────────────────

    private TextBox?          _outputBox;
    private TextBox?          _inputBox;
    private Button?           _clearButton;
    private Button?           _closeButton;
    private SmoothScrollViewer? _scrollViewer;

    private readonly StringBuilder _outputBuffer = new();
    private int _lineCount;

    // ── Template ──────────────────────────────────────────────────────────────

    protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
    {
        base.OnApplyTemplate(e);

        DetachHandlers();

        _outputBox    = e.NameScope.Find<TextBox>(PART_OutputBox);
        _inputBox     = e.NameScope.Find<TextBox>(PART_InputBox);
        _clearButton  = e.NameScope.Find<Button>(PART_ClearButton);
        _closeButton  = e.NameScope.Find<Button>(PART_CloseButton);
        _scrollViewer = e.NameScope.Find<SmoothScrollViewer>(PART_ScrollViewer);

        AttachHandlers();

        if (_outputBox is not null)
            _outputBox.Text = OutputText;

        PseudoClasses.Set(PC_Running,   IsRunning);
        PseudoClasses.Set(PC_HasOutput, !string.IsNullOrEmpty(OutputText));
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == IsRunningProperty)
            PseudoClasses.Set(PC_Running, change.GetNewValue<bool>());
        else if (change.Property == OutputTextProperty)
        {
            if (_outputBox is not null)
                _outputBox.Text = change.GetNewValue<string?>();
            PseudoClasses.Set(PC_HasOutput, !string.IsNullOrEmpty(change.GetNewValue<string?>()));
        }
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>Appends text to the output buffer and updates the display.</summary>
    public void AppendOutput(string text)
    {
        _outputBuffer.Append(text);

        // Count newlines and trim if over limit.
        foreach (char c in text)
            if (c == '\n') _lineCount++;

        while (_lineCount > MaxOutputLines)
        {
            int nl = _outputBuffer.ToString().IndexOf('\n');
            if (nl < 0) break;
            _outputBuffer.Remove(0, nl + 1);
            _lineCount--;
        }

        var full = _outputBuffer.ToString();
        OutputText = full;

        ScrollToEnd();
    }

    /// <summary>Clears all output.</summary>
    public void ClearOutput()
    {
        _outputBuffer.Clear();
        _lineCount = 0;
        OutputText = string.Empty;
    }

    /// <summary>Focuses the input box.</summary>
    public void FocusInput() => _inputBox?.Focus();

    // ── Private helpers ───────────────────────────────────────────────────────

    private void AttachHandlers()
    {
        if (_clearButton is not null) _clearButton.Click += OnClearClicked;
        if (_closeButton is not null) _closeButton.Click += OnCloseClicked;
        if (_inputBox    is not null) _inputBox.KeyDown  += OnInputKeyDown;
    }

    private void DetachHandlers()
    {
        if (_clearButton is not null) _clearButton.Click -= OnClearClicked;
        if (_closeButton is not null) _closeButton.Click -= OnCloseClicked;
        if (_inputBox    is not null) _inputBox.KeyDown  -= OnInputKeyDown;
    }

    private void OnClearClicked(object? s, RoutedEventArgs e) => ClearOutput();
    private void OnCloseClicked(object? s, RoutedEventArgs e) => CloseRequested?.Invoke(this, EventArgs.Empty);

    private void OnInputKeyDown(object? s, KeyEventArgs e)
    {
        if (e.Key != Key.Enter) return;

        var cmd = InputText?.Trim() ?? string.Empty;
        InputText = string.Empty;

        if (!string.IsNullOrEmpty(cmd))
            CommandSubmitted?.Invoke(this, cmd);

        e.Handled = true;
    }

    private void ScrollToEnd()
    {
        if (_scrollViewer is null) return;
        _scrollViewer.Offset = new Vector(_scrollViewer.Offset.X, double.MaxValue);
    }
}
