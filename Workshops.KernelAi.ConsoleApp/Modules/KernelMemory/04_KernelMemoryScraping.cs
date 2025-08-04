namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemoryScraping(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Document and Web Scraping with Kernel Memory";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");
    }
}
