namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class EmbeddingGenerators(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "MEAI Embedding Generators";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        IEmbeddingGenerator embedding = ChatClientFactory.CreateEmbeddingGenerator(embeddingSettings);

        console.WriteLine("Enter a piece of text to generate an embedding for it");
        string message = console.GetUserMessage();

        console.StartAiResponse();
        var generator = embedding.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();
        Embedding<float> embeddingResult = await generator.GenerateAsync(message);

        float[] values = embeddingResult.Vector.ToArray();
        console.EndAiResponse(string.Join(',', values.Select(f => f.ToString("f3"))));
    }
}
