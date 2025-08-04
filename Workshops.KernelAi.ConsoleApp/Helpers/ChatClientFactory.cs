public static class ChatClientFactory
{
    public static IChatClient CreateChatClient(ModelSettings settings)
    {
        return settings.Provider switch
        {
            AiProvider.Ollama => CreateOllamaClient(settings),
            AiProvider.OpenAI => CreateOpenAiChatClient(settings),
            AiProvider.AzureOpenAI => CreateAzureOpenAiChatClient(settings),
            _ => throw new NotSupportedException($"The AI provider {settings.Provider} is not supported."),
        };
    }

    private static IChatClient CreateAzureOpenAiChatClient(ModelSettings settings)
    {
        return CreateAzureOpenAiClient(settings)
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }

    private static OpenAIClient CreateAzureOpenAiClient(ModelSettings settings)
    {
        string url = settings.Url ?? throw new ArgumentException("Url is required for Azure OpenAI", nameof(settings));
        string key = settings.Key ?? throw new ArgumentException("Key is required for Azure OpenAI", nameof(settings));

        ApiKeyCredential credential = new(key);
        OpenAIClientOptions options = new()
        {
            Endpoint = new Uri(url)
        };

        return new OpenAIClient(credential, options);
    }

    private static IChatClient CreateOpenAiChatClient(ModelSettings settings)
    {
        return CreateOpenAiClient(settings)
            .GetChatClient(settings.Model)
            .AsIChatClient();
    }

    private static OllamaApiClient CreateOllamaClient(ModelSettings settings)
    {
        return new OllamaApiClient(settings.Url ?? "http://localhost:11434", settings.Model);
    }

    public static IEmbeddingGenerator CreateEmbeddingGenerator(ModelSettings settings)
    {
        return settings.Provider switch
        {
            AiProvider.Ollama => CreateOllamaClient(settings),
            AiProvider.OpenAI => CreateOpenAiEmbeddingGenerator(settings),
            AiProvider.AzureOpenAI => CreateAzureOpenAiEmbeddingGenerator(settings),
            _ => throw new NotSupportedException($"The AI provider {settings.Provider} is not supported."),
        };
    }

    private static IEmbeddingGenerator CreateOpenAiEmbeddingGenerator(ModelSettings settings)
    {
        return CreateOpenAiClient(settings)
            .GetEmbeddingClient(settings.Model)
            .AsIEmbeddingGenerator();
    }

    private static OpenAIClient CreateOpenAiClient(ModelSettings settings)
    {
        string key = settings.Key ?? throw new ArgumentException("Key is required for OpenAI", nameof(settings));

        ApiKeyCredential credential = new(key);
        OpenAIClientOptions options = new()
        {
            Endpoint = string.IsNullOrWhiteSpace(settings.Url) ? null : new Uri(settings.Url)
        };

        return new OpenAIClient(credential, options);
    }

    private static IEmbeddingGenerator CreateAzureOpenAiEmbeddingGenerator(ModelSettings settings)
    {
        return CreateAzureOpenAiClient(settings)
            .GetEmbeddingClient(settings.Model)
            .AsIEmbeddingGenerator();
    }

#pragma warning disable SKEXP0001 // AsChatCompletionService is experimental.
    public static IKernelBuilder AddWorkshopChatCompletion(this IKernelBuilder builder, ModelSettings settings, bool allowFunctionCalls = false)
    {
        // Normally I wouldn't create an IChatClient, but we have existing logic for it in this app.
        // Doing things this way allows for fewer places to update should we need to change how we create the chat client.
        IChatClient chatClient = CreateChatClient(settings);

        if (allowFunctionCalls)
        {
            // This wraps it into a KernelFunctionInvokingChatClient which automatically calls Kernel Functions
            chatClient = chatClient.AsBuilder()
                .UseKernelFunctionInvocation()
                .Build();
        }

        // Now that we have the chat client, we can register it with the KernelBuilder for dependency injection
        builder.Services.AddSingleton(chatClient);
        builder.Services.AddSingleton(chatClient.AsChatCompletionService());

        // Return the builder to allow for fluent call chaining
        return builder;
    }
#pragma warning restore SKEXP0001

}
