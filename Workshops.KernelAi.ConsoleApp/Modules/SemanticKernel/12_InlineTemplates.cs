namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class InlineTemplates(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Semantic Kernel Inline Templates";

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

        // Template with conditional logic
        string conditionalTemplate = """
            You are a helpful travel assistant.
            {{#if hasPassport}}
            Great! Since you have a passport, here are international travel recommendations for {{$destination}}.
            {{else}}
            Since you don't have a passport, here are domestic travel recommendations for {{$destination}}.
            {{/if}}
            
            Budget: {{$budget}}
            Duration: {{$duration}}
            Interests: {{$interests}}
            
            Provide 3-4 specific recommendations with brief descriptions.
            """;

        // Template with loops
        string listTemplate = """
            You are a meal planning expert.
            Create a weekly meal plan based on these preferences:
            
            Dietary restrictions: {{$dietary}}
            Cooking skill level: {{$skillLevel}}
            Budget per week: {{$budget}}
            
            Available ingredients:
            {{#each ingredients}}
            - {{this}}
            {{/each}}
            
            Please provide a 7-day meal plan with breakfast, lunch, and dinner.
            Include simple recipes for dishes that use the available ingredients.
            """;

        // Create kernel functions from templates
        KernelFunction storyGenerator = kernel.CreateFunctionFromPrompt(simpleTemplate, functionName: "StoryGenerator");
        KernelFunction travelPlanner = kernel.CreateFunctionFromPrompt(conditionalTemplate, functionName: "TravelPlanner");
        KernelFunction mealPlanner = kernel.CreateFunctionFromPrompt(listTemplate, functionName: "MealPlanner");

        console.MarkupLine("[cyan]Semantic Kernel Inline Templates Demo[/]");
        console.WriteLine();

        // Demo 1: Story Generator
        console.MarkupLine("[yellow]1. Creative Story Generator[/]");
        var storyArgs = new KernelArguments
        {
            ["genre"] = "science fiction",
            ["character"] = "a young engineer",
            ["setting"] = "a space station orbiting Mars",
            ["length"] = "short (2-3 paragraphs)",
            ["mood"] = "optimistic"
        };

        console.MarkupLine("[dim]Generating story with parameters:[/]");
        foreach (var arg in storyArgs)
        {
            console.MarkupLine($"[dim]  {arg.Key}: {arg.Value}[/]");
        }
        console.WriteLine();

        console.StartAiResponse();
        var storyResult = await storyGenerator.InvokeAsync(kernel, storyArgs);
        console.EndAiResponse(storyResult.ToString());

        console.WriteLine();
        console.WriteLine("Press any key to continue to travel planning...");
        console.Input.ReadKey(intercept: true);
        console.WriteLine();

        // Demo 2: Travel Planner with conditionals
        console.MarkupLine("[yellow]2. Travel Planner with Conditionals[/]");
        var travelArgs = new KernelArguments
        {
            ["hasPassport"] = true,
            ["destination"] = "Europe",
            ["budget"] = "$3000",
            ["duration"] = "2 weeks",
            ["interests"] = "history, museums, local cuisine"
        };

        console.MarkupLine("[dim]Planning travel with parameters:[/]");
        foreach (var arg in travelArgs)
        {
            console.MarkupLine($"[dim]  {arg.Key}: {arg.Value}[/]");
        }
        console.WriteLine();

        console.StartAiResponse();
        var travelResult = await travelPlanner.InvokeAsync(kernel, travelArgs);
        console.EndAiResponse(travelResult.ToString());

        console.WriteLine();
        console.WriteLine("Press any key to continue to meal planning...");
        console.Input.ReadKey(intercept: true);
        console.WriteLine();

        // Demo 3: Meal Planner with loops
        console.MarkupLine("[yellow]3. Meal Planner with Ingredient Lists[/]");
        var mealArgs = new KernelArguments
        {
            ["dietary"] = "vegetarian",
            ["skillLevel"] = "intermediate",
            ["budget"] = "$75",
            ["ingredients"] = new[] { "pasta", "tomatoes", "cheese", "spinach", "garlic", "onions", "bell peppers", "mushrooms", "beans", "rice" }
        };

        console.MarkupLine("[dim]Creating meal plan with parameters:[/]");
        console.MarkupLine($"[dim]  dietary: {mealArgs["dietary"]}[/]");
        console.MarkupLine($"[dim]  skillLevel: {mealArgs["skillLevel"]}[/]");
        console.MarkupLine($"[dim]  budget: {mealArgs["budget"]}[/]");
        console.MarkupLine($"[dim]  ingredients: {string.Join(", ", (string[])mealArgs["ingredients"]!)}[/]");
        console.WriteLine();

        console.StartAiResponse();
        var mealResult = await mealPlanner.InvokeAsync(kernel, mealArgs);
        console.EndAiResponse(mealResult.ToString());

        console.WriteLine();
        console.MarkupLine("[green]Demo completed! These examples show how to use Semantic Kernel's template system with:[/]");
        console.MarkupLine("[green]- Variable substitution ({{$variable}})[/]");
        console.MarkupLine("[green]- Conditional logic ({{#if condition}})[/]");
        console.MarkupLine("[green]- Loop iteration ({{#each array}})[/]");
    }
}