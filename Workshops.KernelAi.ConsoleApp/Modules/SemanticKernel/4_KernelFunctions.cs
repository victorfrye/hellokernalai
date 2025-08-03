namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class KernelFunctions(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Kernel Functions";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings)
            .Build();

        KernelPlugin plugin = kernel.Plugins.AddFromFunctions("Preferences", "Used to get the user's preferences",
            [
                KernelFunctionFactory.CreateFromMethod(
                    functionName: "GetFavoriteColor",
                    method: () => {
                        console.DisplayToolCall("GetFavoriteColor");
                        return "Dark Clear";
                    }, 
                    description: "Gets the user's favorite color"),
                KernelFunctionFactory.CreateFromMethod(
                    functionName: "GetFavoriteAnimal",
                    method: () => {
                        console.DisplayToolCall("GetFavoriteAnimal");
                        return "Puppies";
                    },
                    description: "Gets the user's favorite animal"),
            ]);

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are a helpful assistant that can answer questions. 
            Try to work the user's favorite things into your responses.
            Keep your responses short and to the point.
            Call the functions GetFavoriteColor and GetFavoriteAnimal to get the user's preferences.
            """);

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();
        string userInput = console.GetUserMessage();

        // Loop until the user enters "exit" or an empty message
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