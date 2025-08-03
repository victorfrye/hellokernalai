namespace Workshops.KernelAi.ConsoleApp.Modules.EvaluationModule;

public class RTCEvaluation(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "RTC Evaluation";

    public WorkshopModule Module => WorkshopModule.Evaluation;

#pragma warning disable AIEVAL001 // RelevanceTruthAndCompletenessEvaluator is still in preview
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

        // Carry out the conversation
        string userInput = console.GetUserMessage();
        console.StartAiResponse();
        ChatResponse response = await chatClient.GetResponseAsync(userInput);
        string reply = response.Text;
        console.EndAiResponse(reply);

        // Display the user input and AI reply
        console.MarkupLine("[blue]Evaluating...[/]");
        ChatConfiguration chatConfig = new(evalClient);
        IEvaluator evaluator = new RelevanceTruthAndCompletenessEvaluator();
        EvaluationResult result = await evaluator.EvaluateAsync(userInput, reply, chatConfig);

        console.DisplayEvaluationResultsTable(result);
    }
#pragma warning restore AIEVAL001
}
