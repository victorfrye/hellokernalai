namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class WithStreaming(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat with Streaming";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings)
            .Build();

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are a Cthulhu cultist that is secretly spying on behalf of the flying spaghetti monster.
            Keep your responses short, yet always absurd and ominous.
            """);

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();
        string userInput = console.GetUserMessage();

        // Loop until the user enters "exit" or an empty message
        while (!string.IsNullOrWhiteSpace(userInput) && userInput != "exit")
        {
            history.AddUserMessage(userInput);

            console.StartAiResponse();
            StringBuilder sb = new();
            await foreach (StreamingChatMessageContent update in chat.GetStreamingChatMessageContentsAsync(history, kernel: kernel))
            {
                console.Write(update.Content ?? "");
                sb.Append(update.Content);
            }
            console.EndAiResponse();
            history.AddAssistantMessage(sb.ToString());

            // Get the next user input
            userInput = console.GetUserMessage();
        }
    }
}