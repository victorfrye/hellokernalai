namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class FakeBank(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Bank of Gotham";

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

    public class FakeBankPlugin(BankAccount account)
    {

        [KernelFunction]
        public async Task<string> GetBalance()
        {
            // Simulate a delay to mimic an external API call
            await Task.Delay(500);

            return $"Your Bank Balance is currently {account.Balance}";
        }

        [KernelFunction]
        public async Task<BankAccount> GetAccountDetails()
        {
            // Simulate a delay to mimic an external API call
            await Task.Delay(500);

            return account;
        }

        [KernelFunction]
        public async Task<string> Deposit(decimal amount)
        {
            try
            {
                // Simulate a delay to mimic an external API call
                await Task.Delay(500);

                account.Deposit(amount);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return ex.Message;
            }

            return $"Deposit complete. Your Bank Balance is now {account.Balance}";
        }

        [KernelFunction]
        public async Task<string> Withdrawal(decimal amount)
        {
            try
            {
                // Simulate a delay to mimic an external API call
                await Task.Delay(500);

                account.Withdraw(amount);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return ex.Message;
            }

            return $"Withdrawal complete. Your Bank Balance is now {account.Balance}";
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

        BankAccount account = new("BruceWayne", 1000);
        kernel.Plugins.AddFromObject(new FakeBankPlugin(account));

        ChatHistory history = [];
        history.AddSystemMessage("""
            You are are an AI assistant that can interact with a banking plugin for the Bank of Gotham.
            The user is already authenticated as Bruce Wayne.
            Perform operations as needed to deposit, withdraw, or check balance.
            """);

        IChatCompletionService chat = kernel.GetRequiredService<IChatCompletionService>();
        string userInput = console.GetUserMessage();

        // Loop until the user enters "exit" or an empty message
        while (!string.IsNullOrWhiteSpace(userInput) && userInput != "exit")
        {
            history.AddUserMessage(userInput);

            PromptExecutionSettings execSettings = new()
            {
                FunctionChoiceBehavior = FunctionChoiceBehavior.Auto()
            };

            console.StartAiResponse();
            StringBuilder sb = new();
            await foreach (var update in chat.GetStreamingChatMessageContentsAsync(history, execSettings, kernel))
            {
                console.Write(update.Content ?? "");
                sb.Append(update.Content);
            }
            console.EndAiResponse();
            history.AddAssistantMessage(sb.ToString());

            // Get the next user input
            userInput = console.GetUserMessage();
        }
    }
}