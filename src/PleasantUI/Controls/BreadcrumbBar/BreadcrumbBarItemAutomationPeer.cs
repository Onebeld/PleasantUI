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
    public BreadcrumbBarItemAutomationPeer(Control owner) : base(owner) { }

    protected override string GetClassNameCore() => nameof(BreadcrumbBarItem);

    protected override AutomationControlType GetAutomationControlTypeCore()
        => AutomationControlType.Button;

    void IInvokeProvider.Invoke()
    {
        if (Owner is BreadcrumbBarItem item)
            item.OnClickEvent(null, null);
    }
}
