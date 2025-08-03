namespace Workshops.KernelAi.ConsoleApp.Settings;

public class WorkshopSettings
{
    public required ModelSettings Chat { get; init; }
    public required ModelSettings Evaluation { get; init; }
    public required ModelSettings Embedding { get; init; }
}
