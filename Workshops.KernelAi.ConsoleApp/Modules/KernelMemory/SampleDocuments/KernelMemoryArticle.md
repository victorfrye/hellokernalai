+++
author = "Matt Eland"
title = "Document Search in .NET with Kernel Memory"
subtitle = "Simple web scraping, document indexing, RAG search, and chat"
description = "Kernel Memory gives us a simple and flexible way of indexing and searching documents and websites using our own models and vector storage solutions using .NET."
date = "2025-05-20"
tags = [
    "ai",
    "rag",
    "search",
    "llm",
    "dotnet"
]
thumbnail = "RAGQuestionAnswering.png"
usePageBundles = true
draft = false
series = ".NET AI / ML Highlights"
+++

I recently discovered the [Kernel Memory](https://github.com/microsoft/kernel-memory) library for document indexing, web scraping, semantic search, and LLM-based question answering. The capabilities, flexibility, and simplicity of this library are so fantastic that it's quickly ascended my list of favorite AI libraries to work with for RAG search, document search, or AI-based question answering.

In this article I'll walk you through what Kernel Memory is and how you can use the C# version of this library to quickly index, search, and chat with knowledge stored in documents or web pages.

## Kernel Memory, a flexible document indexing and RAG search library

At its core, Kernel Memory is all about ingesting information in various sources, indexing it, storing it in a vector storage solution, and providing a means for searching and question answering with this indexed knowledge.

![Indexing data in different formats using Kernel Memory](Indexing.png)

We'll walk through a full small application in this article, but here's a simple implementation to help orient you:

```cs
IKernelMemory memory = new KernelMemoryBuilder()
	.WithOpenAI(openAiConfig)
	.Build();

await memory.ImportDocumentAsync("TheGuide.pdf");

string question = "What is the answer to the question of life, the universe, and everything?"
MemoryAnswer answer = await memory.AskAsync(question);

string reply = answer.Result;
console.WriteLine(reply);
```

In this snippet we see that:

- Kernel Memory uses a standard builder API allowing you to add in various sources that are relevant to you (here an OpenAI text and embedding model)
- Kernel Memory provides `Import` methods allowing you to index documents, text, and web pages and store them in its current vector store
- Kernel Memory provides a convenient way of asking questions to an LLM and providing your information as a RAG data source

In this short example we're using the default volatile memory vector store which is built into Kernel Memory for demonstration purposes, but you could just as easily use an existing vector storage provider such as Qdrant, Azure AI Search, Postgres, Redis, or others.

Likewise, Kernel Memory supports a wide range of LLMs and other ingestion data sources including OpenAI, Anthropic, ONNX, and even locally-running Ollama models.

This last point has me particularly excited because I can now use locally hosted LLMs and on-network vector storage solutions to ingest and search documents without needing to worry about data leaving my network or per-usage cloud hosting costs. This opens up new usage scenarios for me for experimentation, workshops at conferences, and business scenarios.

Let's drill into a larger Kernel Memory app and see how it flows.

## Creating a Kernel Memory instance in C#

Through the rest of this article we'll walk through a small C# console application from start to finish. This project is [available on GitHub](https://github.com/IntegerMan/DocumentSearchWithKernelMemory) if you'd like to clone it locally and experiment with it as well, though you'll need to provide your own API keys.

Next we use some fairly ordinary C# code using the `Microsoft.Extensions.Configuration` mechanism for reading settings:

```cs
IConfiguration config = new ConfigurationBuilder()
	.AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
	.AddEnvironmentVariables()
	.AddUserSecrets<Program>()
	.AddCommandLine(args)
	.Build();
DocSearchDemoSettings settings = config.Get<DocSearchDemoSettings>()!;
```

This reads information from a local JSON file, command-line arguments, user secrets, and environment variables and stores them into a settings object that looks like this:

```cs
public class DocSearchDemoSettings
{
	public required string OpenAIEndpoint { get; init; }
	public required string OpenAIKey { get; init; }
	public required string TextModelName { get; init; }
	public required string EmbeddingModelName { get; init; }
}
```

Most of these settings are set in the `appsettings.json` file, though you should store your endpoint and key in user secrets or environment variables if you plan on working with your own keys and managing things in source control.

```json
{
    "OpenAIEndpoint": "YourEndpoint",
    "OpenAIKey": "YourApiKey",
    "TextModelName": "gpt-4o-mini",
    "EmbeddingModelName": "text-embedding-3-small"
}
```

With our configuration loaded, we now jump into creating our `IKernelMemory` instance, where we'll need to provide information on what model, endpoints and keys to use:

```cs
OpenAIConfig openAiConfig = new()
{
	APIKey = settings.OpenAIKey,
	Endpoint = settings.OpenAIEndpoint,
	EmbeddingModel = settings.EmbeddingModelName,
	TextModel = settings.TextModelName,
};
IKernelMemory memory = new KernelMemoryBuilder()
	.WithOpenAI(openAiConfig)
	.Build();

IAnsiConsole console = AnsiConsole.Console;
console.MarkupLine("[green]KernelMemory initialized.[/]");
```

This creates and configures our Kernel Memory instance using an OpenAI text and embeddings model. The text completion model will be used for conversations with our data using the `AskAsync` method while the embeddings model is used to generate a vector representing different chunks of documents that are indexed as well as the search queries when the memory instance is searched.

By default Kernel Memory is using a volatile in-memory vector store that gets completely discarded and recreated every time the application runs. This is not a production-level solution, but is fine for quick demonstrations on low volumes of data. For larger-scale scenarios or production usage you would use a dedicated vector storage solution and connect it to Kernel Memory when building your `IKernelMemory` instance.

Also note the use of `AnsiConsole`. This is part of `Spectre.Console`, a library I frequently use alongside .NET console apps for enhanced input and output capabilities. We'll see more of this later.

## Indexing Documents and Web Scraping with Kernel Memory

With our Kernel Memory instance set up and running an empty vector store, we should ingest some data before we continue on.

Kernel Memory supports importing data in the following formats:

- Raw strings
- Web pages via web scraping
- Documents in a supported format (PDF, Images, Word, PowerPoint, Excel, Markdown, text files, and JSON)

The API for importing each of these sources is exceptionally simple as well:

```cs
// Index documents and web content
console.MarkupLine("[yellow]Importing documents...[/]");

await memory.ImportTextAsync("KernelMemory allows you to import web pages, documents, and text");
await memory.ImportTextAsync("KernelMemory supports PDF, md, txt, docx, pptx, xlsx, and other formats", "Doc-Id");

await memory.ImportDocumentAsync("Facts.txt", "Repository-Facts");

await memory.ImportWebPageAsync("https://LeadingEDJE.com", "Leading-EDJE-Web-Page");
await memory.ImportWebPageAsync("https://microsoft.github.io/kernel-memory/",
                                "KernelMemory-Web-Page", 
                                new TagCollection { "GitHub"});

console.MarkupLine("[green]Documents imported.[/]");
```

This code indexes a pair of raw strings, a Facts.txt file included with the repository, and a pair of web pages: Leading EDJE's web site (my employer, an IT services consultancy in Columbus, Ohio) and the GitHub repository for Kernel Memory.

Note that as we index anything we can just give it a data source, or we could give it a data source and a document Id, or we could provide additional tag or index metadata as well.

Using tags and indexes you can annotate the documents you insert as belonging to certain collections. This allows you to filter down to certain groups of documents later on when searching or asking questions which supports critical scenarios such as restricting information available to different users based on which organization they're in or their security role.

Most everything about Kernel Memory is customizable as well, so you can change how documents are decoded and partitioned and you can substitute in your own web scraping provider in lieu of Kernel Memory's default one, for example.

These `Import` calls will take a few seconds to complete, based on the size of the data, your text embeddings model, and your choice of vector storage solution. Once it completes, your data will be available for search.

## Searching Documents with Kernel Memory and Text Embeddings

With our data ingested, we can now query Kernel Memory for specific questions. This can come in one of two ways:

1. `SearchAsync` which provides raw search results to be handled programmatically
2. `AskAsync` which performs a search and then has an LLM respond to the question asked given the search results.

While the search results are more complex than the ask results, we should start by exploring search as this helps us understand what Kernel Memory is doing under the hood.

![Searching Text Embeddings with Kernel Memory](Search.png)

The code to conduct the search itself is straightforward and intuitive:

```cs
string search = console.Ask<string>("What do you want to search for?");
console.MarkupLineInterpolated($"[yellow]Searching for '{search}'...[/]");

SearchResult results = await memory.SearchAsync(search);
```

The `SearchResult` object organizes its results into different citations representing different documents searched. Within each citation will be different sets of partitions which represent different chunks of the document which are indexed and stored for data retrieval. This is important because documents and web pages can be very long and you want to match only on the most relevant portions of a document when performing a search.

Each partition has a relevance score stored as a decimal percentage value ranging from 0 to 1.

Using `Spectre.Console`, you can loop over these citations and partitions and create a display table using the following code:

```cs
Table table = new Table()
  .AddColumns("Document", "Partition", "Section", "Score", "Text");

foreach (var citation in results.Results)
{
  foreach (var part in citation.Partitions)
  {
    string snippet = part.Text;
    if (part.Text.Length > 100)
    {
      snippet = part.Text[..100] + "...";
    }

    table.AddRow(citation.DocumentId, part.PartitionNumber.ToString(), part.SectionNumber.ToString(), part.Relevance.ToString("P2"), snippet);
  }
}

table.Expand();
console.Write(table);
console.WriteLine();
```

This produces a formatted table resembling the following image:

![Search results for 'Kernel Memory' displayed in a table](SearchTable.png)

As you can see, each match will have a document, partition, section within that partition, relevance score, and some associated text. Individual tags and source URLs will also be available. Note how document names are not mandatory, but Kernel Memory generates its own random Ids if you don't provide an id yourself.

You can use `SearchAsync` to manually identify the most relevant documents and pieces of documents from your vector store. This can be useful for providing semantic search capabilities across your site, or for identifying text to inject into prompts as a form of Retrieval Augmetnation Generation (RAG). However, if you're working with RAG, there's a chance you might be better off using the `AskAsync` method instead, as we'll see next.

## Question answering with KernelMemory and LLM

If your end goal is to provide a reply to the user from a query they sent you, you should consider using Kernel Memory's `AskAsync` method.

`AskAsync` uses the text model to summarize the result of a search and provide that string back to the user.


![RAG Search with Kernel Memory](RAGQuestionAnswering.png)


The code for this is extremely straightforward:

```cs
string question = console.Ask<string>("What do you want to ask?");
console.MarkupLineInterpolated($"[yellow]Asking '{question}'...[/]");

MemoryAnswer answer = await memory.AskAsync(question);

console.WriteLine(answer.Result);
```

This provides a text output from your LLM as you would expect:

![Kernel Memory fielding a question on the difference between Kernel Memory and Semantic Kernel](QuestionAnswering.png)

As you might imagine, the `AskAsync` method takes significantly longer than `SearchAsync` because it effectively is performing the search as a RAG search and then using the results to chat with the underlying LLM.

If you need information about the sources cited, those are also available in the `MemoryAnswer`, which can be helpful for diagnostic / logging purposes or simply to let the user know what was used in answering their question - or giving them additional links to investigate.

## Kernel Memory Extensions and Integrations

Kernel Memory is a very powerful and flexible library with a simple API and good behaviors out of the box. It has a high degree of customizability in terms of those default behaviors for the times when you need additional control.

For example, you can customize the prompts Kernel Memory uses for fact extraction and summarization, giving you more control of its behavior as a chat partner.

Additionally, Kernel Memory has a number of different deployment models, ranging from in-process `MemoryServerless` implementations like the one described in this article to pre-built Docker containers, to web services.

Kernel Memory was also built with [Semantic Kernel](https://learn.microsoft.com/en-us/semantic-kernel/overview/) at least partially in mind. While Semantic Kernel has its own built-in vector store capabilities, they're harder to use than Kernel Memory's options and don't have as many ingestion options. As a result, you can connect Kernel Memory into a Semantic Kernel instance, providing a RAG data source for your AI orchestration solution. In fact, there's even a [pre-built SemanticKernelPlugin NuGet package](https://www.nuget.org/packages/Microsoft.KernelMemory.SemanticKernelPlugin/) built just for this purpose.

## Conclusion

I'm absolutely enamored with the Kernel Memory library and see a lot of uses for this technology including:

- Simple RAG search and question answering for web applications
- Indexing existing knowledge sources like Confluence or Obsidian vaults
- Providing a cost-effective and secure option for document ingestion, ensuring document data never leaves the network

If you're curious about Kernel Memory, I'd encourage you to take a look at the [GitHub Repository](https://github.com/IntegerMan/DocumentSearchWithKernelMemory) containing this article's code and try things yourself.

I'll be writing more about Kernel Memory in a chapter in my next technical book which releases in Q3 2025, and I'm looking at revising my [Digital Dungeon Master](https://blog.leadingedje.com/post/semantickerneldnd.html) solution to take advantage of the Kernel Memory / Semantic Kernel integration. I can also see some very real ways where Kernel Memory can help offer some of [Leading EDJE's](https://LeadingEDJE.com) current and prospective clients additional value, capabilities, and cost savings so I'm excited to share this technology with the broader technical community.

It's a great time to be doing AI and ML in .NET and I'm elated to have Kernel Memory as a tool in my toolbox.
