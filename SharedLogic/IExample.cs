namespace SharedLogic;

public interface IExample
{
    /// <summary>
    /// Runs the example asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync();

    string Name { get; }
}
