namespace ChatWithMEAI.ChatModule;

public class ChatWithHistory(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat with History";

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);

        List<ChatMessage> history = [];

        // Get the first message from the user
        string message = console.GetUserMessage();

        while (!string.IsNullOrWhiteSpace(message) && !message.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new ChatMessage(ChatRole.User, message));

            // Send the message to the chat client and get a response
            console.StartAiResponse();
            ChatResponse result = await chatClient.GetResponseAsync(history);

            // Add the AI message to the history
            history.Add(new ChatMessage(ChatRole.Assistant, message));

            // Display the response in the console
            console.EndAiResponse(result.Text);

            // Get the next message from the user
            message = console.GetUserMessage();
        }
    }
}
