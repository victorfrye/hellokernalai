namespace Workshops.KernelAi.ConsoleApp.Domain;

public interface IExample
{
    /// <summary>
    /// Runs the example asynchronously.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task RunAsync();

    string Name { get; }
    WorkshopModule Module { get; }
}
