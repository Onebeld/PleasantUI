using Avalonia.Automation.Peers;
using Avalonia.Automation.Provider;
using Avalonia.Controls;

namespace PleasantUI.Controls;

/// <summary>
/// Automation peer for <see cref="BreadcrumbBarItem"/>.
/// Exposes the item as an invokable button to accessibility tools.
/// </summary>
public class BreadcrumbBarItemAutomationPeer : ControlAutomationPeer, IInvokeProvider
{
    /// <summary>
    /// <see cref="BreadcrumbBarItemAutomationPeer"/> control class constructor
    /// </summary>
    /// <param name="owner"></param>
    public BreadcrumbBarItemAutomationPeer(Control owner) : base(owner) { }

    /// <inheritdoc />
    protected override string GetClassNameCore() => nameof(BreadcrumbBarItem);

    /// <inheritdoc />
    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Button;

    void IInvokeProvider.Invoke()
    {
        if (Owner is BreadcrumbBarItem item)
            item.OnClickEvent(null, null);
    }
}
