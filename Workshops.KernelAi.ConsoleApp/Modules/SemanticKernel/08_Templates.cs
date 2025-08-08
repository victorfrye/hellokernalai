namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class InlineTemplates(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Inline Templates";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings)
            .Build();

        // Template with simple variable substitution
        string simpleTemplate = """
            You are a creative writing assistant.
            Write a {{$genre}} story about {{$character}} in {{$setting}}.
            The story should be {{$length}} and have a {{$mood}} tone.
            Keep it engaging and appropriate for all audiences.
            """;

        // Create kernel function from templates
        KernelFunction storyGenerator = kernel.CreateFunctionFromPrompt(simpleTemplate, functionName: "StoryGenerator");

        KernelArguments storyArgs = new()
        {
            ["genre"] = "technothriller",
            ["character"] = "a software developer",
            ["setting"] = "the Beer City Code tech conference in Grand Rapids",
            ["length"] = "short (2-3 sentences)",
            ["mood"] = "eerie"
        };

        Table table = new Table()
            .Title("Story Generator Parameters")
            .AddColumns("Parameter", "Value");
        
        foreach (var arg in storyArgs)
        {
            table.AddRow(arg.Key, arg.Value?.ToString() ?? "null");
        }
        console.Write(table);
        console.WriteLine();

        console.StartAiResponse();
        FunctionResult story = await storyGenerator.InvokeAsync(kernel, storyArgs);
        console.EndAiResponse(story.ToString()); 
        console.WriteLine();
    }
}