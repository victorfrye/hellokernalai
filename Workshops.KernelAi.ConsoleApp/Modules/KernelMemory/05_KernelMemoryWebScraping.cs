namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemoryWebScraping(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Web Scraping with Kernel Memory";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        IKernelMemory kernelMemory = new KernelMemoryBuilder()
            .WithWorkshopTextEmbeddingGeneration(embeddingSettings)
            .WithDefaultWebScraper()
            .WithDefaultMimeTypeDetection()
            .WithDefaultContentDecoders()
            .WithoutTextGenerator()
            .Build();

        console.MarkupLine("[yellow]Scraping web pages...[/]");
        await kernelMemory.ImportWebPageAsync("https://microsoft.github.io/kernel-memory/", documentId: "kmHome");
        await kernelMemory.ImportWebPageAsync("https://microsoft.github.io/kernel-memory/serverless", documentId: "kmServerless");
        await kernelMemory.ImportWebPageAsync("https://microsoft.github.io/kernel-memory/quickstart/configuration", documentId: "kmConfiguration");
        await kernelMemory.ImportWebPageAsync("https://microsoft.github.io/kernel-memory/azure/architecture", documentId: "kmAzure");
        console.MarkupLine("[green]Web pages scraped successfully![/]");

        string query = console.GetUserMessage();
        SearchResult searchResult = await kernelMemory.SearchAsync(query);

        foreach (var citation in searchResult.Results)
        {
            console.Write(new Table()
                .AddColumns("Property", "Value")
                .AddRow("Document Id", citation.DocumentId)
                .AddRow("Source URL", citation.SourceUrl ?? "[red]null[/]")
            );

            foreach (var partition in citation.Partitions)
            {
                console.Write(new Panel(new Text(partition.Text ?? "No Content"))
                    .Header($"{partition.SectionNumber}: {partition.Relevance:p} Relevant, " +
                            $"Last Updated {partition.LastUpdate:g}"));
            }
        }
    }
}
