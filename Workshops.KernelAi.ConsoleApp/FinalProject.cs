namespace Workshops.KernelAi.ConsoleApp;

public class FinalProject(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Self-Guided Project";

    public WorkshopModule Module => WorkshopModule.FinalProject;

    public async Task RunAsync()
    {
        // Here you can implement your final project.
        // Your goal is to create a cohesive experience that plays with data, plugins, and prompts to achieve something
        // meaningful to you.

        // A few ideas to get you started:
        // - Create a chatbot that can answer questions about a specific topic (e.g., a hobby, a book, etc.)
        // - Build a knowledge base that can be queried using natural language (e.g., a FAQ system)
        // - Implement a personal assistant that can help you with daily tasks (e.g., reminders, to-do lists, etc.)
        // - Create a game that uses AI to generate content (e.g., a text-based adventure game, a trivia game, etc.)
        // - Do something crazy like building an AI and convincing it is Alfred from Batman

        // You should build something that uses:
        // - Semantic Kernel for AI Orchestration
        // - Kernel Memory for storing and retrieving information
        // - Custom plugins to extend the functionality of your application (Weather API, Random generators, etc.)

        // The point of this exercise is to apply what you've learned and see what obstacles you encounter.
        // While there's a lot of value in asking for help in person, if you want to reach out later, that's fine too.

        await Task.CompletedTask;
    }
}
