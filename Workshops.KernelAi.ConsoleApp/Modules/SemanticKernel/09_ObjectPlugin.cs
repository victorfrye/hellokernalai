namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class ObjectPlugin(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Inventory Plugin";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;


    public record InventoryItem(string Name, int Durability);

    public class InventoryPlugin
    {
        private List<InventoryItem> _items = [
            new InventoryItem("Pocket sand", Durability: 5),
            new InventoryItem("Rubber chicken", Durability: 10),
            new InventoryItem("Magic wand", Durability: 15),
            new InventoryItem("Bag of holding", Durability: 20),
        ];

        [KernelFunction]
        public List<InventoryItem> GetInventory()
        {
            return _items;
        }

        [KernelFunction]
        public string AddItem(InventoryItem item)
        {
            _items.Add(item);
            return $"Added '{item.Name}' to your inventory.";
        }

        [KernelFunction]
        public string RemoveItem(InventoryItem item)
        {
            _items = _items.Where(i => !string.Equals(i.Name, item.Name, StringComparison.OrdinalIgnoreCase))
                .ToList();

            return $"Removed '{item.Name}' from your inventory.";
        }
    }

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings, allowFunctionCalls: true)
            .Build();

        kernel.AutoFunctionInvocationFilters.Add(new ConsoleLoggingFunctionFilter(console));
        kernel.Plugins.AddFromType<InventoryPlugin>();

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are an AI Agent running an adventure game.
            The player is a brave adventurer exploring a mystical land in a comedic fantasy setting.
            They currently have a number of items in their inventory. You can check these using function calls.
            Present the player with a puzzle scenario where they must use their inventory to solve it.
            The player can pick up items and items they're carrying can break or be dropped. When this happens,
            you should update the inventory by calling the appropriate function.
            """);
        history.AddSystemMessage("""
            Start the adventure by describing their current setting and obstacle.
            """);

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

        // Loop until the user enters "exit" or an empty message
        string userInput;
        do
        {
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
            history.AddUserMessage(userInput);
        } while (!string.IsNullOrWhiteSpace(userInput) && userInput != "exit");
    }
}