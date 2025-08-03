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

// Welcome message using Spectre.Console
try
{
    console.Write(new FigletText("MEAI Chat"));
    console.MarkupLine("[bold green]Chat examples using Microsoft.Extensions.AI[/]");
    console.WriteLine();

    // Select an example to run
    IExample selectedExample = console.Prompt(new SelectionPrompt<IExample>()
        .Title("Select an example to run")
        .AddChoices(sp.GetServices<IExample>())
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
