// Read settings
WorkshopSettings settings = ConfigurationHelper.LoadWorkshopSettings(args);
IAnsiConsole console = AnsiConsole.Console;

ServiceCollection services = new();
services.AddSingleton(settings);
services.AddSingleton(console);

// Register all IExample implementations using Scrutor
services.Scan(scan => scan
    .FromEntryAssembly()
    .AddClasses(classes => classes.AssignableTo<IExample>())
    .AsImplementedInterfaces()
    .WithSingletonLifetime());

ServiceProvider sp = services.BuildServiceProvider();

try
{
    // Welcome message using Spectre.Console
    console.Write(new FigletText("Kernel AI"));
    console.MarkupLine("[cyan]AI Examples using MEAI, MEAI Evaluation, Semantic Kernel, and Kernel Memory[/]");
    console.WriteLine();

    // Select a module to run
    WorkshopModule module = console.Prompt(new SelectionPrompt<WorkshopModule>()
        .Title("Select a module to run")
        .AddChoices(Enum.GetValues<WorkshopModule>())
        .UseConverter(m => m.ToString()));

    // Select an example in that module to run
    IExample selectedExample = console.Prompt(new SelectionPrompt<IExample>()
        .Title("Select an example to run")
        .AddChoices(sp.GetServices<IExample>().Where(e => e.Module == module))
        .UseConverter(e => e.Name));

    console.MarkupLine($"Starting [yellow]{selectedExample.Name}[/]");
    await selectedExample.RunAsync();
}
catch (Exception ex)
{
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
}

console.WriteLine();
console.WriteLine("Press any key to exit...");
console.Input.ReadKey(intercept: true);
