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

    do
    {     // Select a module to run
        WorkshopModule module = console.GetChoice("Select a module to run",
            Enum.GetValues<WorkshopModule>());

        // Select an example in that module to run
        IExample selectedExample = console.GetChoice("Select an example to run",
            sp.GetServices<IExample>().Where(e => e.Module == module),
            e => e.Name);

        console.MarkupLine($"Starting [yellow]{selectedExample.Name}[/]");
        await selectedExample.RunAsync();
    }
    while (console.SafeConfirm("Do you want to run another example?"));
}
catch (Exception ex)
{
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
}

console.WriteLine();
console.WriteLine("Press any key to exit...");
console.Input.ReadKey(intercept: true);
