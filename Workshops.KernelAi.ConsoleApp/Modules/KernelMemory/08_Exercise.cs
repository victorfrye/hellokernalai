
namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemoryExercise(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Kernel Memory Exercise";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings, "Chat");

        // Create a Kernel Memory instance with the embedding and text models you want to use

        // Index documents, facts, or web pages of interest to you

        // Ask questions of the Kernel Memory instance and display the results

        await Task.CompletedTask; // Remove once you have at least one await statement
    }
}
