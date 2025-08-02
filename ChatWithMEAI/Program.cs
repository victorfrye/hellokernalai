// Read settings
WorkshopSettings settings = ConfigurationHelper.LoadWorkshopSettings(args);

// Welcome message using Spectre.Console
IAnsiConsole console = AnsiConsole.Console;
try
{
    console.Write(new FigletText("MEAI Chat"));
    console.MarkupLine("[bold green]Chat examples using Microsoft.Extensions.AI[/]");
    console.WriteLine();

    // Run the HelloWorld example
    HelloWorld helloWorld = new(console, settings);
    await helloWorld.RunAsync();
}
catch (Exception ex)
{
    console.WriteException(ex, ExceptionFormats.ShortenEverything);
}

console.WriteLine();
console.WriteLine("Press any key to exit...");
console.Input.ReadKey(intercept: true);
