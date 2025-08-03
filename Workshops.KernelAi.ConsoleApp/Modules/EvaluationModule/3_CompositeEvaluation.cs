using System.Text;

namespace Workshops.KernelAi.ConsoleApp.Modules.EvaluationModule;

public class CompositeEvaluation(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Composite Evaluation";

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

        // Carry out the conversation using streaming responses
        string userInput = console.GetUserMessage();
        console.StartAiResponse();
        StringBuilder sb = new();
        await foreach (var update in chatClient.GetStreamingResponseAsync(userInput))
        {
            console.Write(update.Text);
            sb.Append(update.Text);
        }
        console.EndAiResponse();
        string reply = sb.ToString();

        // Display the user input and AI reply
        console.MarkupLine("[blue]Evaluating...[/]");
        ChatConfiguration chatConfig = new(evalClient);
        IEvaluator evaluator = new CompositeEvaluator(
            new RelevanceTruthAndCompletenessEvaluator(), 
            new FluencyEvaluator(), 
            new CoherenceEvaluator(),
            new RelevanceEvaluator()
        );
        EvaluationResult result = await evaluator.EvaluateAsync(userInput, reply, chatConfig);

        console.DisplayEvaluationResultsTable(result);
    }
#pragma warning restore AIEVAL001
}
