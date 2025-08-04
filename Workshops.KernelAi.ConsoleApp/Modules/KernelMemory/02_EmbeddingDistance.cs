using System.Numerics.Tensors;

namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class EmbeddingDistance(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "MEAI Embedding Distance";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        IEmbeddingGenerator embedding = ChatClientFactory.CreateEmbeddingGenerator(embeddingSettings);
        var generator = embedding.GetRequiredService<IEmbeddingGenerator<string, Embedding<float>>>();

        console.WriteLine("Enter two pieces of text to compare embeddings");
        string message1 = console.GetUserMessage();

        console.StartAiResponse();
        Embedding<float> embedding1 = await generator.GenerateAsync(message1);
        console.EndAiResponse("First embedding generated.");

        string message2 = console.GetUserMessage();
        console.StartAiResponse();
        Embedding<float> embedding2 = await generator.GenerateAsync(message2);
        console.EndAiResponse("Second embedding generated.");


        // Assuming the embeddings are of the same dimension and Vector<float> is available
        if (embedding1.Vector.Length != embedding2.Vector.Length)
        {
            console.MarkupLine("[red]Embeddings must be of the same dimension to calculate distance.[/]");
            return;
        }

        float similarity = TensorPrimitives.CosineSimilarity(embedding1.Vector.Span, embedding2.Vector.Span);

        console.MarkupLine($"[green]Cosine Similarity between the two embeddings: {similarity:F4}[/]");
    }
}
