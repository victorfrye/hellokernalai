namespace ChatWithMEAI.ChatModule;

public class Module1Exercise(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Module 1 Exercise";

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create a chat client (You can do this on your own or use the ChatClientFactory class)

        // Build a chat history with a system prompt and optionally some few-shot examples

        // Get the first message from the user

        // Send the message to the chat client and get a response

        // Display the response in the console

        // Optionally, you can loop to continue the conversation until the user types "exit"

        await Task.CompletedTask; // Can be removed once you have at least one await statement
    }
}
