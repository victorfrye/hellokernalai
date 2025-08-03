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
        Table table = new Table().Title("Evaluation Results");
        table.AddColumns("Metric", "Value", "Reason");
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

            table.AddRow(kvp.Key, value, reason);
        }

        console.Write(table);
    }
}
