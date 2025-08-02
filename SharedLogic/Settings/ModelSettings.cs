namespace SharedLogic.Settings;

public class ModelSettings
{
    public required AiProvider Provider { get; init; }
    public required string Url { get; init; }
    public required string ModelId { get; init; }
}
