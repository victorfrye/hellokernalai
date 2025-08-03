namespace Workshops.KernelAi.ConsoleApp.Modules.EvaluationModule;

public class PromptTuning(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "System Prompt Tuning";

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

        const string SystemPrompt = """
            You are an AI assistant that specializes in creative problem-solving using humor and absurdity.
            Your task is to provide imaginative and humorous solutions to outlandish questions.
            Keep your responses to only a few sentences, and always include a touch of humor.
            """;

        const string userInput = "Help, I'm trapped in a closet with only a wristwatch, shoelaces, and a rabid ferret.";
        List<ChatMessage> messages = [
            new ChatMessage(ChatRole.System, SystemPrompt),
            new ChatMessage(ChatRole.User, userInput)
        ];

        // Display the user input and AI reply
        console.MarkupLine("[blue]Evaluating...[/]");
        ReportingConfiguration reportConfig = DiskBasedReportingConfiguration.Create(
            storageRootPath: Environment.CurrentDirectory,
            evaluators: [new RelevanceTruthAndCompletenessEvaluator()],
            chatConfiguration: new ChatConfiguration(chatClient),
            enableResponseCaching: true,
            executionName: $"{DateTime.Now:yyyyMMddTHHmmss}",
            tags: ["Ferret", $"Eval_{evalSettings.Model}"]);

        console.MarkupLine($"[yellow]User:[/] {userInput}");
        console.StartAiResponse();
        List<ChatResponseUpdate> updates = [];
        await foreach (var update in chatClient.GetStreamingResponseAsync(messages))
        {
            console.Write(update.Text);
            updates.Add(update);
        }
        console.EndAiResponse();
        ChatResponse response = updates.ToChatResponse();

        // Evaluate the response using our reporting configuration. This will store the results on disk
        await using ScenarioRun run =
            await reportConfig.CreateScenarioRunAsync("Trapped in a Closet",
            additionalTags: [$"Model_{chatSettings.Model}"]);
        await run.EvaluateAsync(messages, response);
        await run.DisposeAsync(); // Ensure we dispose of the run to finalize writing the results

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
