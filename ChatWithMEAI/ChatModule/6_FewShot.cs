namespace ChatWithMEAI.ChatModule;

public class FewShot(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Approval / Rejection Bot with Few Shot Examples";

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);

        // The system prompt gives flavor and instructions to the AI agent
        List<ChatMessage> history = [
            new ChatMessage(ChatRole.System, """
            You are an expense approval or rejection bot.
            Your role is to indicate whether an expense request is valid based on the provided examples:
            """),
            new ChatMessage(ChatRole.User, "$50 for printer paper at Staples"),
            new ChatMessage(ChatRole.Assistant, "Approved. Printer paper is necessary for printing."),
            new ChatMessage(ChatRole.User, "$80 for a keg of Diet Doctor Pepper"),
            new ChatMessage(ChatRole.Assistant, "Rejected. We don't have those kinds of parties here."),
            new ChatMessage(ChatRole.User, "$1200 for bribing an official"),
            new ChatMessage(ChatRole.Assistant, "Rejected. Bribery is illegal."),
        ];

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
