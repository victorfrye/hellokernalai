
namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemorySemanticKernelIntegration(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Kernel Memory as a Semantic Kernel Plugin";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    private class KernelMemoryPlugin(ModelSettings embeddingSettings)
    {
        private readonly IKernelMemory _memory = new KernelMemoryBuilder()
                .WithWorkshopTextEmbeddingGeneration(embeddingSettings)
                .WithoutTextGenerator()
                .Build();

        public void IndexAsync(Action<IKernelMemory> indexingAction)
        {
            indexingAction(_memory);
        }

        [KernelFunction]
        public async Task<string> Search(string query)
        {
            SearchResult results = await _memory.SearchAsync(query, limit: 3);
            
            if (results.NoResult) { 
                return "I couldn't find anything relevant in my knowledge base.";
            }

            return "Here are some relevant facts I found:\r\n" +
                   string.Join("\r\n - ", results.Results.SelectMany(r => r.Partitions).Select(r => r.Text));
        }
    }

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings, "Chat");

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings, allowFunctionCalls: true)
            .Build();

        kernel.AutoFunctionInvocationFilters.Add(new ConsoleLoggingFunctionFilter(console));

        KernelMemoryPlugin memoryPlugin = new(embeddingSettings);
        memoryPlugin.IndexAsync(async memory =>
        {
            await memory.ImportTextAsync("Mermaids are made out of cheese");
            await memory.ImportTextAsync("Cheese is a bioengineered lure to attract sailors.");
            await memory.ImportTextAsync("Sailors do not actually exist.");
            await memory.ImportTextAsync("The flying spaghetti monster is real.");
            await memory.ImportTextAsync("Giving random facts to an AI agent may produce interesting results");
        });
        kernel.Plugins.AddFromObject(memoryPlugin);

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are an eccentric librarian that loves to share obscure facts.
            You have access to a knowledge base of random facts that you can share with the user.
            Keep your responses short, to the point, and always a little quirky.
            """);

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

        string userInput = console.GetUserMessage();
        while (!string.IsNullOrWhiteSpace(userInput) && userInput != "exit")
        {
            history.AddUserMessage(userInput);

            PromptExecutionSettings execSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            console.StartAiResponse();
            StringBuilder sb = new();
            await foreach (var update in chat.GetStreamingChatMessageContentsAsync(history, execSettings, kernel))
            {
                console.Write(update.Content ?? "");
                sb.Append(update.Content);
            }
            console.EndAiResponse();
            history.AddAssistantMessage(sb.ToString());

            // Get the next user input
            userInput = console.GetUserMessage();
        }
    }
}
