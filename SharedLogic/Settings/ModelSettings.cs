namespace SharedLogic.Settings;

public class ModelSettings
{
    public required AiProvider Provider { get; init; }
    public string? Url { get; init; }
    public required string Model { get; init; }
    public string? Key { get; init; }
}
