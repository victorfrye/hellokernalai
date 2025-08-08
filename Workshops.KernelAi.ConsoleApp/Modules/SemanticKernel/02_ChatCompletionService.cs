namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class HelloSemanticKernel(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat Completion Service";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings)
            .Build();
        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

        string userInput = console.GetUserMessage();

        console.StartAiResponse();
        ChatMessageContent response = await chat.GetChatMessageContentAsync(userInput, kernel: kernel);
        console.EndAiResponse(response.Content);
    }
}
