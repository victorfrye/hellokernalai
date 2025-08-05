using System.Text.Json;

namespace Workshops.KernelAi.ConsoleApp.Modules.KernelMemory;

public class KernelMemorySearchText(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Searching Text with Kernel Memory";

    public WorkshopModule Module => WorkshopModule.KernelMemory;

    public async Task RunAsync()
    {
        ModelSettings embeddingSettings = settings.Embedding;
        console.WriteModelInfo(embeddingSettings, "Embedding");

        IKernelMemory kernelMemory = new KernelMemoryBuilder()
            .WithWorkshopTextEmbeddingGeneration(embeddingSettings)
            .WithoutTextGenerator()
            .Build();

        console.MarkupLine("[yellow]Adding text to Kernel Memory...[/]");
        await kernelMemory.ImportTextAsync("""
            Squirrels are small, furry animals that are known for their bushy tails and playful behavior. 
            They are often found in parks and gardens, where they can be seen climbing trees and foraging 
            for nuts and seeds. Squirrels are also known for their agility and speed, making them a common 
            sight in urban areas.
            """);

        await kernelMemory.ImportTextAsync("""
            Beer City Code is an annual conference for software developers held in Grand Rapids, MI, 
            also known as Beer City, USA. Software creators of all types are welcome, even those who 
            don't care for beer.

            The main conference is held Saturday, August 9, with optional day-long workshops the day before, 
            Friday, August 8. You can attend just the Saturday conference, add on an all-day workshop on 
            Friday, or even upgrade to a ticket that includes a VIP party with our speakers, sponsors, 
            and organizers. How cool is that?!

            The Beer City Code conference grew our of the success of its predecessor, Grand Rapids DevDay, 
            but was renamed in 2017 to embrace its much more broad appeal outside the Grand Rapids area.
            """);

        await kernelMemory.ImportTextAsync("""
            At Leading EDJE, we develop technology to help you positively disrupt the status quo
            and achieve unprecedented growth. From custom software development to digital transformation 
            and AI-driven innovation, our holistic approach unlocks the full potential of your business. 
            Explore how our expertise in each solution category can help you drive growth and create lasting 
            """);

        await kernelMemory.ImportTextAsync("""
            Matt Eland is an AI Specialist and Wizard at Leading EDJE who is known to teach software engineering, AI, and 
            data science concepts in the most ridiculous ways possible. Matt has used machine learning to settle 
            debates over whether Die Hard is a Christmas movie, reinforcement learning to drive the behavior of 
            digital squirrels, data analytics to suggest improvements to his favorite TV show, and AI agents to 
            play board games and create an AI agent with the personality of a dog. Matt is the author of 
            "Data Science in .NET with Polyglot Notebooks" and "Refactoring with C#" as well as several 
            LinkedIn Learning courses. Matt helps organize the Central Ohio .NET Developer Group, runs several 
            blogs and a YouTube channel, has a Master’s of Science in Data Analytics, and is a current 
            Microsoft MVP in AI and .NET.
            """);

        await kernelMemory.ImportTextAsync("""
            Victor Frye is a developer who thrives at the intersection of technical problem-solving and creative 
            innovation. As a Senior Software Engineer and DevOps specialist at Leading EDJE, he specializes in 
            .NET application development, Azure cloud infrastructure, and DevOps automation, with a focus on 
            modernization and developer productivity. The self-proclaimed number one Clippy fan, he brings a 
            playful yet efficient approach to his work. Beyond the keyboard, Victor is an active member of the 
            Grand Rapids developer community, always eager to share insights and explore emerging technologies. 
            During his free time, he can be found exploring fantasy worlds—in pages or pixels—or spending quality 
            time with his wife and two dogs. Fueled by coffee and curiosity, Victor is always ready for the next 
            challenge.
            """);

        console.MarkupLine("[green]Text added to Kernel Memory.[/]");
        string searchText = console.GetUserMessage();

        console.StartAiResponse();
        SearchResult result = await kernelMemory.SearchAsync(searchText);
        console.EndAiResponse($"Search completed. Found {result.Results.Count} matches.");

        string json = result.ToJson();
        console.Write(new JsonText(json));
    }
}
