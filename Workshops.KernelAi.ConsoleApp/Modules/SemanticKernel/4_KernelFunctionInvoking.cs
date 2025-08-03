namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class KernelFunctionInvoke(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Invoking Kernel Functions";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings)
            .Build();

        KernelFunction func = KernelFunctionFactory.CreateFromMethod(
                    functionName: "GetFavoriteColor",
                    method: () =>
                    {
                        console.DisplayToolCall("GetFavoriteColor");
                        return "Dark Clear";
                    },
                    description: "Gets the user's favorite color");

        string? directResult = await func.InvokeAsync<string>(kernel);
        console.WriteLine($"Direct Function Result: {directResult}");

        FunctionResult result = await kernel.InvokeAsync(func);
        console.WriteLine($"Kernel Function Result: {result.GetValue<string>()}");
    }
}