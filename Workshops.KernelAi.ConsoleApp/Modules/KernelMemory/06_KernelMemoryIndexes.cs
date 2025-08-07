namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemoryIndexes(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Managing Indexes and Metadata with Kernel Memory";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        IKernelMemory kernelMemory = new KernelMemoryBuilder()
            .WithWorkshopTextEmbeddingGeneration(embeddingSettings)
            .WithoutTextGenerator()
            .Build();

        console.MarkupLine("Importing sample documents...");
        await kernelMemory.ImportTextAsync(
            "The business is doing amazing! EvilCorp has never been better!", 
            documentId:"AllHandsJan2025", 
            index: "EvilCorp",
            tags: new TagCollection()
            {
                { "Speaker", "Thadeus McCEO" },
                { "Event", "All Hands" },
                { "Type", "Meeting" },
                { "Date", "2025-01-07" },
                { "Access", ["Public", "Leadership", "Engineering", "HR"] }
            });

        await kernelMemory.ImportTextAsync(
            "Start prepping a list for a possible layoffs in Engineering",
            documentId: "InternalHRJan25",
            index: "EvilCorp",
            tags: new TagCollection()
            {
                { "From", "Thadeus McCEO" },
                { "To", "Melinda HRVonFace" },
                { "Type", "Email" },
                { "Date", "2025-01-11" },
                { "Access", ["Leadership", "HR"] }
            });

        await kernelMemory.ImportTextAsync(
            "We've successfully installed several rootkits at EvilCorp with the help " +
            "of a disgruntled engineering team member.",
            documentId: "SneakyEspionage",
            index: "SneakyCorp",
            tags: new TagCollection()
            {
                { "Type", "Email" },
                { "From", "REDACTED" },
                { "To", "REDACTED" },
                { "Date", "2025-02-04" },
                { "Access", ["Leadership", "Espionage"] }
            });

        console.MarkupLine("[green]Documents imported.[/]");

        IEnumerable<IndexDetails> indexes = await kernelMemory.ListIndexesAsync();
        console.MarkupLine("Select an index to filter by:");
        IndexDetails index = console.GetChoice("Select Index",
            indexes,
            i => i.Name);

        string accessLevel = console.GetChoice("Select Access Level to filter by",
            ["Public", "Leadership", "Engineering", "HR", "Espionage"]);

        string indexName = index.Name;
        console.MarkupLine($"Searching for 'EvilCorp' in index {indexName} with tag filter Access={accessLevel}...");
        SearchResult results = await kernelMemory.SearchAsync(
            query: "EvilCorp", 
            index: indexName,
            filters: [MemoryFilters.ByTag("Access", accessLevel)]
        );

        string json = results.ToJson();
        console.Write(new JsonText(json));
    }
}
