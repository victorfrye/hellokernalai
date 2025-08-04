namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class PromptFunctions(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Kernel with Nested Experts";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings, allowFunctionCalls: true)
            .Build();

        kernel.AutoFunctionInvocationFilters.Add(new ConsoleLoggingFunctionFilter(console));

        kernel.Plugins.AddFromFunctions("Experts", "Experts that can answer questions",
            [
                KernelFunctionFactory.CreateFromMethod(
                    functionName: "Mechanic",
                    method: (Kernel kernel, string question) => 
                        kernel.InvokePromptAsync<string>(
                            $"""
                            You are a highly qualified, yet aloof mechanic. 
                            Briefly answer the following question: {question}
                            """),
                    description: "Gets a response from a mechanic on a particular question"),
                KernelFunctionFactory.CreateFromMethod(
                    functionName: "Doctor",
                    method: (Kernel kernel, string question) =>
                        kernel.InvokePromptAsync<string>(
                            $"""
                            You are an irritated starfleet doctor.
                            Answer medical questions with a paragraph of technobabble.
                            Answer non-medical questions with "Dammit Jim, I'm a doctor, not a [relevant other profession]!".
                            You've just been asked: {question}
                            """),
                    description: "Gets a response from a doctor on a particular question"),
            ]);

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are a triage AI chat that can answer questions by calling on experts.
            Ask the doctor and/or mechanic functions for help as needed and use their 
            responses to answer the user's question. Your answer should be only a few
            sentences long and should mention which expert you consulted. 
            You do not need to ask for permission to call experts.
            """);

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();
        string userInput = console.GetUserMessage();

        // Loop until the user enters "exit" or an empty message
        while (!string.IsNullOrWhiteSpace(userInput) && userInput != "exit")
        {
            history.AddUserMessage(userInput);

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
        }
    }
}