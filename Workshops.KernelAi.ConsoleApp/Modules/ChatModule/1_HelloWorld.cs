namespace Workshops.KernelAi.ConsoleApp.Modules.ChatModule;

public class HelloWorld(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Hello World";
    public WorkshopModule Module => WorkshopModule.Chat;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);

        // Use a hardcoded message for now
        string message = "Hello from Beer City Code!";
        console.WriteUserMessage(message);

        // Send the message to the chat client and get a response
        console.StartAiResponse();
        ChatResponse result = await chatClient.GetResponseAsync(message);

        // Display the response in the console
        console.EndAiResponse(result.Text);
    }
}
