namespace ChatWithMEAI;

public class HelloWorld(IAnsiConsole console, WorkshopSettings settings)
{
    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);

        // Get a message from the user
        console.MarkupLine($"[orange3]Using {chatSettings.Provider} for chat with model: {chatSettings.Model}[/]");
        console.WriteLine();
        string message = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]User:[/] "));

        // Send the message to the chat client and get a response
        console.Markup($"[blue]{chatSettings.Provider}: [/]");
        ChatResponse result = await chatClient.GetResponseAsync(message);

        // Display the response in the console
        console.WriteLine(result.Text);
    }
}
