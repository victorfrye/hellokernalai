namespace SharedLogic.Settings;

public class WorkshopSettings
{
    public required ModelSettings Chat { get; init; }
    public required ModelSettings Embedding { get; init; }
}
