namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemorySemanticKernelIntegration(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Kernel Memory as a Semantic Kernel Plugin";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");
    }
}
