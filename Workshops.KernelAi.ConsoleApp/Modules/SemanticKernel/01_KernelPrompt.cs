namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class KernelPrompt(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Semantic Kernel Haiku";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        IKernelBuilder builder = Kernel.CreateBuilder();

        // Connect the kernel to the chat model provider specified in the settings
        switch (chatSettings.Provider)
        {
            case AiProvider.AzureOpenAI:
                builder.AddAzureOpenAIChatCompletion(chatSettings.Model, chatSettings.Url!, chatSettings.Key!);
                break;
            case AiProvider.OpenAI:
                if (string.IsNullOrWhiteSpace(chatSettings.Url))
                {
                    builder.AddOpenAIChatCompletion(chatSettings.Model, chatSettings.Key!);
                }
                else
                {
                    builder.AddOpenAIChatCompletion(chatSettings.Model, chatSettings.Url, chatSettings.Key);
                }
                break;
            case AiProvider.Ollama:
                builder.AddOllamaChatCompletion(chatSettings.Model, new Uri(chatSettings.Url ?? "http://localhost:11434"));
                break;
            default:
                throw new NotSupportedException($"Model provider {chatSettings.Provider} is not supported.");
        }

        Kernel kernel = builder.Build();

        string userMessage = "Compose a short haiku about Semantic Kernel";
        console.MarkupLine($"[yellow]User:[/] {userMessage}");

        console.StartAiResponse();
        FunctionResult result = await kernel.InvokePromptAsync(userMessage);
        console.EndAiResponse(result.ToString());
    }
}
