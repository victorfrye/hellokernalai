namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

#pragma warning disable SKEXP0001 // AsChatCompletionService is experimental
public class WithHistory(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat with System Prompt & History";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        IKernelBuilder builder = Kernel.CreateBuilder();
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);
        IChatCompletionService chat = chatClient.AsChatCompletionService();
        builder.Services.AddSingleton(chat);
        Kernel kernel = builder.Build();

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are the great and powerful Sphynx, an enigmatic mind imprisoned in an AI Agent. 
            Keep your responses short and to the point, yet always terribly mysterious.
            """);

        string userInput = console.GetUserMessage();
        while (!string.IsNullOrWhiteSpace(userInput) && userInput != "exit")
        {
            history.AddUserMessage(userInput);

            console.StartAiResponse();
            ChatMessageContent response = await chat.GetChatMessageContentAsync(history, kernel: kernel);
            console.EndAiResponse(response.Content);
            history.AddAssistantMessage(response.Content!);

            // Get the next user input
            userInput = console.GetUserMessage();
        }
    }
}
#pragma warning restore SKEXP0001