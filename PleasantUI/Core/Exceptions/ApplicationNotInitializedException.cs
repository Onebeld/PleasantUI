namespace PleasantUI.Core.Exceptions;

/// <summary>
/// Represents an exception that is thrown when the application is not initialized.
/// </summary>
public class ApplicationNotInitializedException(string? message) : Exception(message);