namespace Workshops.KernelAi.ConsoleApp.Modules.ChatModule;

public class ToolCalls(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Chat with Tool Calls";
    public WorkshopModule Module => WorkshopModule.Chat;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings)
            .AsBuilder()
            .UseFunctionInvocation() // Normally this goes in the part where we create the client originally
            .Build();

        // The system prompt gives flavor and instructions to the AI agent
        List<ChatMessage> history = [
            new ChatMessage(ChatRole.System, """
            You are a singing weatherman. 
            Deliver concise weather updates in the form of a short song or rhyme.
            """)];

        // Get the first message from the user
        string message = console.GetUserMessage();

        while (!string.IsNullOrWhiteSpace(message) && !message.Equals("exit", StringComparison.OrdinalIgnoreCase))
        {
            history.Add(new ChatMessage(ChatRole.User, message));

            // I like to create a new ChatOptions object for each request due to some bugs I've seen over the years
            ChatOptions chatOptions = new()
            {
                ToolMode = ChatToolMode.Auto,
                AllowMultipleToolCalls = true,
                Tools = [AIFunctionFactory.Create(GetCurrentWeatherConditions)]
            };

            // Send the message to the chat client and get a response
            console.StartAiResponse();
            List<ChatResponseUpdate> updates = [];
            await foreach (var update in chatClient.GetStreamingResponseAsync(history, chatOptions))
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

    private string GetCurrentWeatherConditions()
    {
        console.DisplayToolCall();

        return "Volcanic Ash"; // You'd usually actually call a weather API here
    }
}
