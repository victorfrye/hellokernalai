namespace Workshops.KernelAi.ConsoleApp.Modules.SemanticKernel;

public class ExternalTemplates(IAnsiConsole console, WorkshopSettings settings) : IExample
{
    public string Name => "Semantic Kernel External Templates";

    public WorkshopModule Module => WorkshopModule.SemanticKernel;

    public async Task RunAsync()
    {
        ModelSettings chatSettings = settings.Chat;
        console.WriteModelInfo(chatSettings);

        Kernel kernel = Kernel.CreateBuilder()
            .AddWorkshopChatCompletion(chatSettings)
            .Build();

        console.MarkupLine("[cyan]Semantic Kernel External Templates Demo[/]");
        console.WriteLine();

        // Load templates from files
        string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Content", "Templates");
        
        if (!Directory.Exists(templatePath))
        {
            console.MarkupLine("[red]Error: Templates directory not found![/]");
            console.MarkupLine($"[red]Expected path: {templatePath}[/]");
            return;
        }

        // Load all template files
        var templates = new Dictionary<string, KernelFunction>();
        
        foreach (string templateFile in Directory.GetFiles(templatePath, "*.txt"))
        {
            string templateName = Path.GetFileNameWithoutExtension(templateFile);
            string templateContent = await File.ReadAllTextAsync(templateFile);
            
            templates[templateName] = kernel.CreateFunctionFromPrompt(
                templateContent, 
                functionName: templateName);
                
            console.MarkupLine($"[dim]Loaded template: {templateName}[/]");
        }

        console.WriteLine();

        // Demo 1: Email Template
        if (templates.ContainsKey("EmailTemplate"))
        {
            console.MarkupLine("[yellow]1. Professional Email Generator[/]");
            
            var emailArgs = new KernelArguments
            {
                ["type"] = "follow-up",
                ["subject"] = "Project Status Update Required",
                ["recipient"] = "Project Manager",
                ["context"] = "Need to provide weekly status update for the customer portal redesign project",
                ["isUrgent"] = false,
                ["includeAttachments"] = true,
                ["tone"] = "professional and collaborative",
                ["length"] = "medium (2-3 paragraphs)"
            };

            console.MarkupLine("[dim]Email parameters:[/]");
            foreach (var arg in emailArgs)
            {
                console.MarkupLine($"[dim]  {arg.Key}: {arg.Value}[/]");
            }
            console.WriteLine();

            console.StartAiResponse();
            var emailResult = await templates["EmailTemplate"].InvokeAsync(kernel, emailArgs);
            console.EndAiResponse(emailResult.ToString());

            console.WriteLine();
            console.WriteLine("Press any key to continue to documentation generation...");
            console.Input.ReadKey(intercept: true);
            console.WriteLine();
        }

        // Demo 2: Documentation Template
        if (templates.ContainsKey("DocumentationTemplate"))
        {
            console.MarkupLine("[yellow]2. Technical Documentation Generator[/]");
            
            var docArgs = new KernelArguments
            {
                ["componentName"] = "UserAuthenticationService",
                ["componentType"] = "REST API Service",
                ["technology"] = "C# ASP.NET Core",
                ["hasParameters"] = true,
                ["parameters"] = new[] { "username (string): User's login name", "password (string): User's password", "rememberMe (bool): Whether to keep user logged in" },
                ["hasExamples"] = true,
                ["includeErrorHandling"] = true,
                ["audience"] = "backend developers",
                ["style"] = "technical with code examples"
            };

            console.MarkupLine("[dim]Documentation parameters:[/]");
            console.MarkupLine($"[dim]  componentName: {docArgs["componentName"]}[/]");
            console.MarkupLine($"[dim]  componentType: {docArgs["componentType"]}[/]");
            console.MarkupLine($"[dim]  technology: {docArgs["technology"]}[/]");
            console.MarkupLine($"[dim]  audience: {docArgs["audience"]}[/]");
            console.WriteLine();

            console.StartAiResponse();
            var docResult = await templates["DocumentationTemplate"].InvokeAsync(kernel, docArgs);
            console.EndAiResponse(docResult.ToString());

            console.WriteLine();
            console.WriteLine("Press any key to continue to marketing content...");
            console.Input.ReadKey(intercept: true);
            console.WriteLine();
        }

        // Demo 3: Marketing Template
        if (templates.ContainsKey("MarketingTemplate"))
        {
            console.MarkupLine("[yellow]3. Marketing Content Generator[/]");
            
            var marketingArgs = new KernelArguments
            {
                ["productName"] = "DevTools Pro",
                ["industry"] = "Software Development",
                ["targetAudience"] = "professional developers and development teams",
                ["features"] = new[] { "AI-powered code completion", "Real-time collaboration", "Integrated debugging", "Cloud deployment tools" },
                ["benefits"] = new[] { "50% faster development", "Reduced bugs and errors", "Seamless team collaboration", "One-click deployment" },
                ["hasDiscount"] = true,
                ["discountDetails"] = "30% off first year for new customers",
                ["contentType"] = "landing page hero section",
                ["tone"] = "confident and professional",
                ["callToAction"] = "Start your free trial today"
            };

            console.MarkupLine("[dim]Marketing parameters:[/]");
            console.MarkupLine($"[dim]  productName: {marketingArgs["productName"]}[/]");
            console.MarkupLine($"[dim]  industry: {marketingArgs["industry"]}[/]");
            console.MarkupLine($"[dim]  targetAudience: {marketingArgs["targetAudience"]}[/]");
            console.MarkupLine($"[dim]  contentType: {marketingArgs["contentType"]}[/]");
            console.WriteLine();

            console.StartAiResponse();
            var marketingResult = await templates["MarketingTemplate"].InvokeAsync(kernel, marketingArgs);
            console.EndAiResponse(marketingResult.ToString());
        }

        console.WriteLine();
        console.MarkupLine("[green]Demo completed! This example shows how to:[/]");
        console.MarkupLine("[green]- Load templates from external files[/]");
        console.MarkupLine("[green]- Organize templates in a content folder structure[/]");
        console.MarkupLine("[green]- Create reusable template libraries[/]");
        console.MarkupLine("[green]- Maintain separation between code and content[/]");
    }
}