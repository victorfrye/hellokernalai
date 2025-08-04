namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class Plugins(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Dragon Encounter (Dice Rolling Plugin)";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public class DicePlugin
    {
        [KernelFunction]
        public string RollD20()
        {
            return RollDice(20);
        }

        [KernelFunction]
        public string RollD6()
        {
            return RollDice(6);
        }

        [KernelFunction]
        public string RollDice(int sides = 6)
        {
            Random random = Random.Shared;
            int result = random.Next(1, sides + 1);
            return $"You rolled a {result} on a {sides}-sided die.";
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
        kernel.Plugins.AddFromType<DicePlugin>();

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are a game master AI Agent. Every turn roll dice for a situation and narrate the outcome.
            Low numbers are bad, high numbers are good. The game is a fantasy RPG 
            and you're currently set in a climactic sequence where they have to survive a series of catastrophies.
            Keep your responses short, yet always dramatic and engaging.
            Make sure to roll dice each turn to determine the outcome of the player's actions and base the result
            off of the dice roll. The player is currently hanging off of a cliff over a lava pit, with a dragon flying
            overhead. The temple is crumbling around them, and they must escape quickly.
            """);

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();

        // Loop until the user enters "exit" or an empty message
        string userInput;
        do
        {
            PromptExecutionSettings execSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Required()
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