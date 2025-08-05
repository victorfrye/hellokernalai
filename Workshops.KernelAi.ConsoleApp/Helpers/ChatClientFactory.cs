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

    public static IKernelMemoryBuilder WithWorkshopTextEmbeddingGeneration(this IKernelMemoryBuilder builder, ModelSettings embeddingSettings)
    {
        switch (embeddingSettings.Provider)
        {
            case AiProvider.AzureOpenAI:
                AzureOpenAIConfig aoaiConfig = new()
                {
                    APIKey = embeddingSettings.Key ?? throw new InvalidOperationException("Azure OpenAI Embedding Key is required"),
                    Endpoint = embeddingSettings.Url ?? throw new InvalidOperationException("Azure OpenAI Embedding URL is required"),
                    Deployment = embeddingSettings.Model ?? throw new InvalidOperationException("Azure OpenAI Embedding Model (Deployment) is required")
                };
                builder.WithAzureOpenAITextEmbeddingGeneration(aoaiConfig);
                break;
            case AiProvider.OpenAI:
                OpenAIConfig openAIConfig = new()
                {
                    APIKey = embeddingSettings.Key ?? throw new InvalidOperationException("OpenAI Embedding Key is required"),
                    EmbeddingModel = embeddingSettings.Model ?? throw new InvalidOperationException("OpenAI Embedding Model is required")
                };
                if (embeddingSettings.Url is not null)
                {
                    openAIConfig.Endpoint = embeddingSettings.Url;
                }
                builder.WithOpenAITextEmbeddingGeneration(openAIConfig);
                break;
            case AiProvider.Ollama:
                builder.WithOllamaTextEmbeddingGeneration(embeddingSettings.Model, embeddingSettings.Url ?? "http://localhost:11434");
                break;
            default:
                throw new NotSupportedException($"Embedding provider {embeddingSettings.Provider} is not supported.");
        }

        return builder;
    }

    public static IKernelMemoryBuilder WithWorkshopTextGenerator(this IKernelMemoryBuilder builder, ModelSettings chatSettings)
    {
        switch (chatSettings.Provider)
        {
            case AiProvider.AzureOpenAI:
                AzureOpenAIConfig aoaiConfig = new()
                {
                    APIKey = chatSettings.Key ?? throw new InvalidOperationException("Azure OpenAI Chat Key is required"),
                    Endpoint = chatSettings.Url ?? throw new InvalidOperationException("Azure OpenAI Chat URL is required"),
                    Deployment = chatSettings.Model ?? throw new InvalidOperationException("Azure OpenAI Chat Model (Deployment) is required")
                };
                builder.WithAzureOpenAITextGeneration(aoaiConfig);
                break;
            case AiProvider.OpenAI:
                OpenAIConfig openAIConfig = new()
                {
                    APIKey = chatSettings.Key ?? throw new InvalidOperationException("OpenAI Chat Key is required"),
                    EmbeddingModel = chatSettings.Model ?? throw new InvalidOperationException("OpenAI Chat Model is required")
                };
                if (chatSettings.Url is not null)
                {
                    openAIConfig.Endpoint = chatSettings.Url;
                }
                builder.WithOpenAITextGeneration(openAIConfig);
                break;
            case AiProvider.Ollama:
                builder.WithOllamaTextGeneration(chatSettings.Model, chatSettings.Url ?? "http://localhost:11434");
                break;
            default:
                throw new NotSupportedException($"Chat provider {chatSettings.Provider} is not supported.");
        }

        return builder;
    }
}
