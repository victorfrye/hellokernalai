namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemoryAsk(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Asking Questions with Kernel Memory";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");
    }
}
