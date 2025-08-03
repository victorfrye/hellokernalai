namespace Workshops.KernelAi.ConsoleApp.Modules.ChatModule;

public class StreamingChat(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat with Streaming Responses";
    public WorkshopModule Module => WorkshopModule.Chat;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);

        // The system prompt gives flavor and instructions to the AI agent
        List<ChatMessage> history = [];

        // Get the first message from the user
        string message = console.GetUserMessage();

        while (!string.IsNullOrWhiteSpace(message) && !message.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new ChatMessage(ChatRole.User, message));

            // Send the message to the chat client and get a response
            console.StartAiResponse();
            List<ChatResponseUpdate> updates = [];
            await foreach (var update in chatClient.GetStreamingResponseAsync(history))
            {
                console.Write(update.Text);
                updates.Add(update);
            }
            console.EndAiResponse();

            // Combine the updates into a single response
            ChatResponse response = updates.ToChatResponse();

            // Add the AI message to the history
            history.Add(new ChatMessage(ChatRole.Assistant, response.Text));

            // Get the next message from the user
            message = console.GetUserMessage();
        }
    }
}
