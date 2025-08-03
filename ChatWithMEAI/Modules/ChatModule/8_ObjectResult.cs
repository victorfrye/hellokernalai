namespace Workshops.KernelAi.ConsoleApp.Modules.ChatModule;

public class ObjectResult(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Returning objects instead of strings";
    public WorkshopModule Module => WorkshopModule.Chat;

    private class SupportTicket
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public int Priority { get; set; }
    }

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        // Create the appropriate chat client based on the provider specified in the settings
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);
        // NOTE: We cannot combine function-calling and object results

        // The system prompt gives flavor and instructions to the AI agent
        List<ChatMessage> history = [
            new ChatMessage(ChatRole.System, """
            You are an IT support bot. Your job is to triage incoming messages into support ticket objects.
            Each ticket should have a title, description, and priority level from 1-5 with 1 being low and 5 is urgent.
            """)];

        console.WriteLine("Please describe your issue and the system will create a support ticket.");

        // Get the message from the user
        string message = console.GetUserMessage();
        history.Add(new ChatMessage(ChatRole.User, message));

        // Send the message to the chat client and get a response
        console.StartAiResponse();
        ChatResponse<SupportTicket> response = await chatClient.GetResponseAsync<SupportTicket>(history);

        if (response.TryGetResult(out SupportTicket? ticket))
        {
            console.MarkupLine("[green]Support ticket created successfully![/]");
            Table table = new Table()
                .AddColumns("Field", "Value")
                .AddRow("Title", ticket.Title)
                .AddRow("Description", ticket.Description)
                .AddRow("Priority", ticket.Priority.ToString());
            console.Write(table);
        }
        else
        {
            console.MarkupLine("[red]Failed to create a support ticket[/]");
            console.WriteLine(response.Text ?? "Unknown error occurred.");
        }
        console.EndAiResponse();

        // We could loop like we've done before, but for this example we'll just stop here.
    }
}
