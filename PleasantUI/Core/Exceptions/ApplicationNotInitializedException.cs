namespace PleasantUI.Core.Exceptions;

public class ApplicationNotInitializedException : Exception
{
    public ApplicationNotInitializedException(string? message) : base(message)
    {
        
    }
}