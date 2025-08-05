namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemoryAsk(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Asking Questions with Kernel Memory (RAG)";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings, "Chat");

        IKernelMemory kernelMemory = new KernelMemoryBuilder()
            .WithWorkshopTextEmbeddingGeneration(embeddingSettings)
            .WithWorkshopTextGenerator(chatSettings)
            .Build();

        console.MarkupLine("[yellow]Scraping web pages...[/]");
        await kernelMemory.ImportWebPageAsync("https://microsoft.github.io/kernel-memory/", documentId: "kmHome");
        await kernelMemory.ImportWebPageAsync("https://learn.microsoft.com/en-us/semantic-kernel/overview/", documentId: "skHome");
        console.MarkupLine("[green]Web pages scraped successfully![/]");

        string query = console.GetUserMessage();

        console.StartAiResponse();
        MemoryAnswer answer = await kernelMemory.AskAsync(query);
        console.EndAiResponse(answer.Result);

        // We can cite the relevant search results too
        foreach (var source in answer.RelevantSources)
        {
            console.MarkupLine($"[blue]Source:[/] {source.DocumentId}");

            /* You also have access to the text of the source and their relevance if you wanted to display it
            foreach (var partition in source.Partitions)
            {
                console.MarkupLine($"  [dim]Text:[/] {partition.Text}");
            }
            */
        }

    }
}
