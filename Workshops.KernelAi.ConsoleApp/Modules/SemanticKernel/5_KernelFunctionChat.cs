namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class KernelFunctionChat(IAnsiConsole console, WorkshopSettings settings) : IExample
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

        kernel.Plugins.AddFromFunctions("Preferences", "Used to get the user's preferences",
            [
                KernelFunctionFactory.CreateFromMethod(
                    functionName: "GetFavoriteColor",
                    method: () => "Dark Clear", 
                    description: "Gets the user's favorite color"),
                KernelFunctionFactory.CreateFromMethod(
                    functionName: "GetFavoriteAnimal",
                    method: () => "Puppies",
                    description: "Gets the user's favorite animal"),
            ]);

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are a helpful assistant that can answer questions and provide commentary.
            Try to work the user's favorite things into your responses.
            Keep your responses to a sentence or two, and always include a touch of humor.
            Call functions as needed.
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
                foreach (var item in update.Items)
                {
                    if (item is StreamingFunctionCallUpdateContent func)
                    {
                        console.DisplayToolCall(func.Name, func.Arguments);
                    }
                }
                sb.Append(update.Content);
            }
            console.EndAiResponse();
            history.AddAssistantMessage(sb.ToString());

            // Get the next user input
            userInput = console.GetUserMessage();
        }
    }
}