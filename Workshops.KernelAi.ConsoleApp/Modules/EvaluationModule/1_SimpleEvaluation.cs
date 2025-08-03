namespace Workshops.KernelAi.ConsoleApp.Modules.EvaluationModule;

public class SimpleEvaluation(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Evaluate String Fluency";

    public WorkshopModule Module => WorkshopModule.Evaluation;

    public async Task RunAsync()
    {
        // Create the appropriate evaluation client based on the provider specified in the settings
        ModelSettings evalSettings = settings.Evaluation;
        console.WriteModelInfo(evalSettings, "Evaluation");
        IChatClient evalClient = ChatClientFactory.CreateChatClient(evalSettings);

        // We typically will evaluate AI messages, but we'll start with a user input for simplicity
        console.WriteLine("Enter a piece of text and the evaluator will assess it.");
        string userInput = console.GetUserMessage();

        // Ask the evaluation model to evaluate the message
        console.MarkupLine("[blue]Evaluating...[/]");
        IEvaluator evaluator = new FluencyEvaluator();
        ChatConfiguration chatConfig = new(evalClient);
        EvaluationResult result = await evaluator.EvaluateAsync(userInput, chatConfig);

        // We can get individual metrics by their names. You can also iterate through all metrics.
        result.Metrics.TryGetValue("Fluency", out EvaluationMetric? fluencyMetric);
        if (fluencyMetric is NumericMetric numericMetric)
        {
            console.MarkupLine($"Score: [orange3]{numericMetric.Value:F1}[/]");
            console.MarkupLine($"Reason: [orange3]{numericMetric.Reason}[/]");
            console.MarkupLine($"Pass / Fail: {(numericMetric.Interpretation!.Failed 
                ? $"[red]Fail - {numericMetric.Interpretation.Reason}[/]" 
                : "[green]Pass[/]")}");
        }
        else
        {
            console.MarkupLine("[red]Fluency evaluation failed or returned no score.[/]");
        }
    }
}
