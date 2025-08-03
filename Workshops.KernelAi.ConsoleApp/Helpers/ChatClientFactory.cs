public static class ChatClientFactory
{
    public static IChatClient CreateChatClient(ModelSettings settings)
    {
        return settings.Provider switch
        {
            AiProvider.Ollama => CreateOllamaChatClient(settings),
            AiProvider.OpenAI => CreateOpenAiChatClient(settings),
            AiProvider.AzureOpenAI => CreateAzureOpenAiChatClient(settings),
            _ => throw new NotSupportedException($"The AI provider {settings.Provider} is not supported."),
        };
    }

    private static IChatClient CreateAzureOpenAiChatClient(ModelSettings settings)
    {
        string url = settings.Url ?? throw new ArgumentException("Url is required for Azure OpenAI", nameof(settings));
        string key = settings.Key ?? throw new ArgumentException("Key is required for Azure OpenAI", nameof(settings));

        ApiKeyCredential credential = new(key);
        OpenAIClientOptions options = new()
        {
            Endpoint = new Uri(url)
        };
        
        return new OpenAIClient(credential, options)
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }

    private static IChatClient CreateOpenAiChatClient(ModelSettings settings)
    {
        string key = settings.Key ?? throw new ArgumentException("Key is required for Azure OpenAI", nameof(settings));

        ApiKeyCredential credential = new(key);
        OpenAIClientOptions options = new()
        {
            Endpoint = string.IsNullOrWhiteSpace(settings.Url) ? null : new Uri(settings.Url)
        };

        return new OpenAIClient(credential, options)
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }

    private static IChatClient CreateOllamaChatClient(ModelSettings settings)
    {
        return new OllamaApiClient(settings.Url ?? "http://localhost:11434", settings.Model);
    }

    public static IKernelBuilder AddWorkshopChatCompletion(this IKernelBuilder builder, ModelSettings settings)
    {
        IChatClient chatClient = CreateChatClient(settings)
            .AsBuilder()
            .UseKernelFunctionInvocation()
            .Build();

        builder.Services.AddSingleton(chatClient);
#pragma warning disable SKEXP0001 // AsChatCompletionService is experimental.
        builder.Services.AddSingleton(chatClient.AsChatCompletionService());
#pragma warning restore SKEXP0001

        return builder;
    }
}
