namespace Workshops.KernelAi.ConsoleApp.Modules.EvaluationModule;

public class ContextualEvaluators(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Contextual Evaluators";

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

        // Carry out the conversation using streaming responses
        string userInput = "What is the answer to the ultimate question of life, the universe, and everything?";
        console.MarkupLine($"[yellow]User:[/] {userInput}");
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
            new CompletenessEvaluator(),
            new EquivalenceEvaluator(),
            new GroundednessEvaluator()
        );

        List<EvaluationContext> context = [
            new CompletenessEvaluatorContext("The answer should address life, the universe, and everything in existence"),
            new EquivalenceEvaluatorContext("The answer should be equivalent to '42'"),
            new GroundednessEvaluatorContext("The answer should be based on the book 'The Hitchhiker's Guide to the Galaxy' by Douglas Adams")
        ];
        EvaluationResult result = await evaluator.EvaluateAsync(userInput, reply, chatConfig, context);

        console.DisplayEvaluationResultsTable(result);
    }
}
