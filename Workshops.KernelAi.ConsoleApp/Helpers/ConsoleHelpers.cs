using System.Runtime.CompilerServices;

namespace Workshops.KernelAi.ConsoleApp.Helpers;

public static class ConsoleHelpers
{
    public static string GetUserMessage(this IAnsiConsole console)
    {
        return console.Prompt(new TextPrompt<string>("[Yellow]User:[/] "));
    }

    public static void StartAiResponse(this IAnsiConsole console)
    {
        console.Markup("[Blue]AI: [/]");
    }

    public static void EndAiResponse(this IAnsiConsole console, string? message = null)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            console.WriteLine();
        }
        else
        {
            console.WriteLine(message);
        }
    }

    public static void DisplayToolCall(this IAnsiConsole console, [CallerMemberName] string toolName = "")
    {
        console.MarkupLine($"[orange3]Calling tool: {toolName}...[/]");
    }

    public static void DisplayToolCall(this IAnsiConsole console, string? toolName, string? arguments)
    {
        console.MarkupLine($"[orange3]Calling tool: {toolName ?? "Unnamed"}{arguments ?? ""}[/]");
    }

    public static void DisplayToolCall(this IAnsiConsole console, string? toolName, KernelArguments? arguments)
    {
        if (arguments is null)
        {
            DisplayToolCall(console, toolName, "");
        } 
        else
        {
            string argsString = string.Join(", ", arguments.Select(kvp => $"{kvp.Key}: {kvp.Value}"));
            DisplayToolCall(console, toolName, $"{{{argsString}}}");
        }
    }

    public static void WriteUserMessage(this IAnsiConsole console, string message)
    {
        console.Markup("[Yellow]User: [/]");
        console.WriteLine(message);
    }

    public static void WriteModelInfo(this IAnsiConsole console, ModelSettings settings, string interactionType = "chat")
    {
        console.MarkupLine($"[orange3]Using {settings.Provider} for {interactionType} with model: {settings.Model}[/]\r\n");
    }

    public static void DisplayEvaluationResultsTable(this IAnsiConsole console, EvaluationResult evalResult)
    {
        Table table = new Table().Title("[blue]Evaluation Results[/]");
        table.AddColumns("[orange3]Metric[/]", "[orange3]Value[/]", "[orange3]Reason[/]");
        foreach (var kvp in evalResult.Metrics)
        {
            EvaluationMetric metric = kvp.Value;
            string reason = metric.Reason ?? "No Reason Provided";
            string value = metric.ToString() ?? "No Value";
            if (metric is NumericMetric num)
            {
                double? numValue = num.Value;
                if (numValue.HasValue)
                {
                    value = numValue.Value.ToString("F1");
                }
                else
                {
                    value = "No value";
                }
            }

            if (metric.Interpretation is not null)
            {
                reason = metric.Interpretation.Reason ?? reason;
                if (metric.Interpretation.Failed)
                {
                    value = $"[red]{value}[/]";
                    reason = $"[red]{reason}[/]";
                }
                else
                {
                    value = $"[green]{value}[/]";
                }
            }

            table.AddRow(metric.Name, value, reason);
        }

        console.Write(table);
    }

    public static T GetChoice<T>(this IAnsiConsole console, string title, IEnumerable<T> choices, Func<T, string>? converter = null) where T : notnull
    {
        // M1 Macs in VS Code do not typically report supporting interactive mode with Spectre.Console so this helps avoid issues in workshops
        while (!console.Profile.Capabilities.Interactive)
        {
            for (int i = 1; i <= choices.Count(); i++)
            {
                T choice = choices.ElementAt(i - 1);
                console.MarkupLine($"[yellow]{i}[/] {converter?.Invoke(choice) ?? choice.ToString()}");
            }

            console.WriteLine();
            console.Write("Select a choice by number: ");
            string input = Console.ReadLine() ?? string.Empty;
            
            if (int.TryParse(input, out int index) && index > 0 && index <= choices.Count())
            {
                return choices.ElementAt(index - 1);
            }

            console.MarkupLine("[red]Invalid selection.[/]");
        }
        
        return console.Prompt(new SelectionPrompt<T>()
            .Title(title)
            .AddChoices(choices)
            .UseConverter(c => (converter is null ? c.ToString() : converter(c))!));
    }
    
    public static bool SafeConfirm(this IAnsiConsole console, string message)
    {
        // M1 Macs in VS Code do not typically report supporting interactive mode with Spectre.Console so this helps avoid issues in workshops
        if (!console.Profile.Capabilities.Interactive)
        {
            while (true)
            {
                console.Write($"{message} (y/n): ");
                ConsoleKeyInfo input = Console.ReadKey();
                
                console.WriteLine();
                if (input.Key == ConsoleKey.Y)
                {
                    return true;
                }
                if (input.Key == ConsoleKey.N)
                {
                    return false;
                }
                console.MarkupLine("[red]Invalid input. Please enter 'y' or 'n'.[/]");
            }
        }

        return console.Confirm(message, defaultValue: true);
    }
}
