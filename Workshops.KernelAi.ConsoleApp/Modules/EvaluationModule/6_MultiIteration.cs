namespace Workshops.KernelAi.ConsoleApp.Modules.EvaluationModule;

public class MultiIteration(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Reporting Multiple Iterations";

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
        string userInput = "In 50 words or less, tell me how to build a second moon around the Earth using only cheese?";

        // Display the user input and AI reply
        console.MarkupLine("[blue]Evaluating...[/]");
        ReportingConfiguration reportConfig = DiskBasedReportingConfiguration.Create(
            storageRootPath: Environment.CurrentDirectory,
            evaluators: [new RelevanceTruthAndCompletenessEvaluator()],
            chatConfiguration: new ChatConfiguration(chatClient),
            enableResponseCaching: true,
            executionName: $"{DateTime.Now:yyyyMMddTHHmmss}",
            tags: ["Cheese", "Moon", $"Eval_{evalSettings.Model}"]);

        const int NumIterations = 3;
        for (int i = 1; i <= NumIterations; i++)
        {
            console.Write(new Rule($"Iteration {i}/{NumIterations}"));

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

            // Evaluate the response using our reporting configuration. This will store the results on disk
            await using ScenarioRun run =
                await reportConfig.CreateScenarioRunAsync("Cheese Moon", 
                iterationName: i.ToString(), 
                additionalTags: [$"Model_{chatSettings.Model}"]);
            await run.EvaluateAsync(userInput, reply); // We could still work with the EvaluationResult here if needed
            await run.DisposeAsync(); // Ensure we dispose of the run to finalize writing the results
        }

        // Enumerate the last 5 executions and add them to our list we'll use for reporting
        List<ScenarioRunResult> results = [];
        await foreach (string executionName in reportConfig.ResultStore.GetLatestExecutionNamesAsync(count: 5))
        {
            await foreach (ScenarioRunResult result in reportConfig.ResultStore.ReadResultsAsync(executionName))
            {
                results.Add(result);
            }
        }

        // Generate the report from our results
        string reportFilePath = Path.Combine(Environment.CurrentDirectory, "report.html");
        IEvaluationReportWriter reportWriter = new HtmlReportWriter(reportFilePath);
        await reportWriter.WriteReportAsync(results);
        console.MarkupLine($"[green]Report generated at: {reportFilePath}[/]");

        // Open the report in the default browser
        Process.Start(new ProcessStartInfo
        {
            FileName = reportFilePath,
            UseShellExecute = true
        });
    }
#pragma warning restore AIEVAL001
}
