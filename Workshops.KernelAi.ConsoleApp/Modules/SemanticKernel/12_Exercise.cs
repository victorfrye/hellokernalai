namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class Module3Exercise(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Module 3 Exercise";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public class BankAccount(string username, decimal initialBalance = 0)
    {
        public string Username { get; } = username;
        public decimal Balance { get; private set; } = initialBalance;

        public void Withdraw(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Withdrawal amount must be greater than zero.", nameof(amount));
            }
            if (amount > Balance)
            {
                throw new InvalidOperationException($"Insufficient funds for this withdrawal. Current balance is {Balance}");
            }
            Balance -= amount;
        }

        public void Deposit(decimal amount)
        {
            if (amount <= 0)
            {
                throw new ArgumentException("Deposit amount must be greater than zero.", nameof(amount));
            }
            Balance += amount;
        }
    }
    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings, allowFunctionCalls: true)
            .Build();

        kernel.AutoFunctionInvocationFilters.Add(new ConsoleLoggingFunctionFilter(console));

        // Create any plugins, methods, objects, etc. you need for the exercise.

        // Create a chat history to hold the conversation

        // System message / few-shot prompt would go here if you want them.

        // Get the service you need for chat completions

        // Loop over a conversation with the user, calling the service and passing it your kernel

        await Task.CompletedTask; // Remove once you have at least one await statement
    }
}