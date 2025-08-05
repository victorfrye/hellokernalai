using MongoDB.Bson;

namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemoryDocumentScraping(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Document Scraping with Kernel Memory";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        IKernelMemory kernelMemory = new KernelMemoryBuilder()
            .WithWorkshopTextEmbeddingGeneration(embeddingSettings)
            .WithDefaultMimeTypeDetection()
            .WithDefaultContentDecoders()
            .WithoutTextGenerator()
            .Build();
        // NOTE: Image extraction is also supported via Azure AI Services' OCR capabilities, or a custom OCR service

        console.MarkupLine("[yellow]Scraping documents...[/]");
        HashSet<string> supportedExtensions = new(StringComparer.OrdinalIgnoreCase) { 
            ".txt", ".md", ".html", ".htm", ".pdf", ".docx", ".xlsx", ".pptx"
        };

        DirectoryInfo docsDir = new(Path.Combine(Environment.CurrentDirectory, "Modules", "KernelMemory", "SampleDocuments"));
        foreach (var doc in docsDir.GetFiles())
        {
            if (!supportedExtensions.Contains(doc.Extension))
            {
                console.MarkupLine($"[grey]Skipping {doc.Name} as the extension is not supported.[/]");
                continue;
            }
            console.MarkupLine($"[blue]Processing {doc.Name}...[/]");
            await kernelMemory.ImportDocumentAsync(doc.FullName, doc.Name);
        }
        console.MarkupLine("[green]Documents scraped successfully![/]");

        string query = console.GetUserMessage();
        SearchResult searchResult = await kernelMemory.SearchAsync(query);

        Dictionary<string, string> docIds = new(StringComparer.OrdinalIgnoreCase);
        foreach (var partition in searchResult.Results.SelectMany(c => c.Partitions)
                                                      .OrderByDescending(p => p.Relevance)
                                                      .Take(3))
        {
            string docId = partition.Tags["__document_id"]!.First()!;
            console.Write(new Panel(new Text(partition.Text ?? "No Content"))
                   .Header($"{docId} ({partition.Relevance:p} Relevant)")
                   .Expand());
        }
    }
}
