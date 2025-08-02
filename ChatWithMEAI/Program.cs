// Read settings
WorkshopSettings settings = ConfigurationHelper.LoadWorkshopSettings(args);
ModelSettings chatSettings = settings.Chat;

// Create the appropriate chat client based on the provider specified in the settings
IChatClient chatClient;
switch (chatSettings.Provider)
{
    case AiProvider.Ollama:
        chatClient = new OllamaApiClient(chatSettings.Url ?? "http://localhost:11434", chatSettings.Model);
        break;
    case AiProvider.OpenAI:
        ApiKeyCredential openAiKey = new(chatSettings.Key ?? throw new InvalidOperationException("Key is required for OpenAI"));
        OpenAIClientOptions openAiOptions = new()
        {
            Endpoint = string.IsNullOrWhiteSpace(chatSettings.Url) ? null : new Uri(chatSettings.Url!)
        };
        chatClient = new OpenAIClient(openAiKey, openAiOptions)
            .GetChatClient(chatSettings.Model)
            .AsIChatClient();
        break;
    case AiProvider.AzureOpenAI:
        OpenAIClientOptions options = new()
        {
            Endpoint = new Uri(chatSettings.Url ?? throw new InvalidOperationException("Url is required for Azure OpenAI"))
        };
        ApiKeyCredential key = new(chatSettings.Key ?? throw new InvalidOperationException("Key is required for Azure OpenAI"));
        chatClient = new OpenAIClient(key, options)
            .GetChatClient(chatSettings.Model)
            .AsIChatClient();
        break;
    default:
        throw new NotSupportedException($"The AI provider {chatSettings.Provider} is not supported.");
}

// Get a message from the user
AnsiConsole.MarkupLine($"[orange3]Using {chatSettings.Provider} for chat with model: {chatSettings.Model}[/]");
AnsiConsole.WriteLine();
string message = AnsiConsole.Prompt(new TextPrompt<string>("[Yellow]User:[/] "));

// Send the message to the chat client and get a response
AnsiConsole.Markup($"[blue]{chatSettings.Provider}: [/]");
ChatResponse result = await chatClient.GetResponseAsync(message);

// Display the response in the console
AnsiConsole.WriteLine(result.Text);
