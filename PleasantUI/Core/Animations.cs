using Avalonia.Animation;
using Avalonia.Collections;

namespace PleasantUI.Core;

/// <summary>
/// A collection of <see cref="Avalonia.Controls.Control"/>s.
/// </summary>
public class Animations : AvaloniaList<Animation>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="Controls"/> class.
    /// </summary>
    public Animations()
    {
        Configure();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Controls"/> class.
    /// </summary>
    /// <param name="items">The initial items in the collection.</param>
    public Animations(IEnumerable<Animation> items)
    {
        Configure();
        AddRange(items); // virtual member call in ctor, ok for our current implementation
    }

    private void Configure()
    {
        ResetBehavior = ResetBehavior.Remove;
        Validate = item =>
        {
            if (item is null)
            {
                throw new ArgumentNullException(nameof(item),
                    $"A null control cannot be added to a {nameof(Controls)} collection.");
            }
        };
    }
}