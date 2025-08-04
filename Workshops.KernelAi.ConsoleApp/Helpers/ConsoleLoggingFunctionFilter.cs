using Spectre.Console.Json;
using System.Text.Json;

namespace Workshops.KernelAi.ConsoleApp.Helpers;

public class ConsoleLoggingFunctionFilter(IAnsiConsole console) : IAutoFunctionInvocationFilter
{
    private readonly Style _style = new(foreground: Color.MediumPurple3);

    public async Task OnAutoFunctionInvocationAsync(AutoFunctionInvocationContext context, 
        Func<AutoFunctionInvocationContext, Task> next)
    {
        // Trace the function call
        string toolName = $"{context.Function.PluginName}_{context.Function.Name}";
        console.DisplayToolCall(toolName, context.Arguments);

        // Call the next filter in the pipeline (or the function itself if this is the last filter)
        await next(context);

        // Log the result of the function invocation
        console.Markup($"[orange3]{toolName} Result:[/] ");
        if (context.Result is not null)
        {
            if (context.Result.ValueType == typeof(string))
            {
                console.WriteLine(context.Result.ToString(), _style);
            }
            else
            {
                console.WriteLine();
                string json = JsonSerializer.Serialize(context.Result.GetValue<object>());
                console.Write(new JsonText(json));
                console.WriteLine();
            }
        }
        else
        {
            console.WriteLine("No result", _style);
        }
    }
}
