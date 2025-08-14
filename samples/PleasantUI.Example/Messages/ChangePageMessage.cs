using PleasantUI.Example.Interfaces;

namespace PleasantUI.Example.Messages;

public class ChangePageMessage
{
    public IPage Page { get; }

    public ChangePageMessage(IPage page)
    {
        Page = page;
    }
}