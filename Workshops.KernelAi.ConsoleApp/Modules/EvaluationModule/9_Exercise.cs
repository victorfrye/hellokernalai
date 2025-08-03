namespace Workshops.KernelAi.ConsoleApp.Modules.EvaluationModule;

public class Module2Exercise(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Module 2 Exercise";

    public WorkshopModule Module => WorkshopModule.Evaluation;

    public async Task RunAsync()
    {
        // Create the appropriate evaluation client based on the provider specified in the settings
        ModelSettings evalSettings = settings.Evaluation;
        console.WriteModelInfo(evalSettings, "Evaluation");
        IChatClient evalClient = ChatClientFactory.CreateChatClient(evalSettings);

        // We also create a chat client to use for getting the AI's response
        ModelSettings chatSettings = settings.Evaluation;
        console.WriteModelInfo(chatSettings, "Chat");
        IChatClient chatClient = ChatClientFactory.CreateChatClient(chatSettings);

        // Create a reporting configuration to store the results on disk

        // Create a chat history with a system prompt. You may also consider one-shot or few-shot examples

        // Start a scenario run for the evaluation

        // Send fake user input to your AI system and get a response (ensure all are in the chat history)

        // Evaluate the response using at least one evaluator and your reporting configuration.

        // Optionally display the evaluation result in the console

        // Generate a report from the results of your evaluation

        // Open the report in the default browser

        await Task.CompletedTask; // Remove once you have at least one await statement
    }
}
