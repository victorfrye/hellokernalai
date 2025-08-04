namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class HelloSemanticKernel(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat Completion Service";

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
        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

        string userInput = console.GetUserMessage();

        console.StartAiResponse();
        ChatMessageContent response = await chat.GetChatMessageContentAsync(userInput, kernel: kernel);
        console.EndAiResponse(response.Content);
    }
}
