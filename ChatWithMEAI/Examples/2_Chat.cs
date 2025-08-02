namespace ChatWithMEAI.Examples;

public class Chat(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat Loop";

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);

        // Get the first message from the user
        string message = console.GetUserMessage();

        while (!string.IsNullOrWhiteSpace(message) && !message.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            // Send the message to the chat client and get a response
            console.StartAiResponse();
            ChatResponse result = await chatClient.GetResponseAsync(message);

            // Display the response in the console
            console.WriteLine(result.Text);

            // Get the next message from the user
            message = console.GetUserMessage();
        }
    }
}
