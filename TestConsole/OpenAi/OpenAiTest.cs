using System.Collections.Concurrent;
using System.ComponentModel.DataAnnotations;
using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace TestConsole.OpenAi;

internal class OpenAiTest
{
    private static readonly JsonSerializerSettings s_serializerSettings = new()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
    };

    private readonly IOptions<OpenAiConfig> _config;

    public OpenAiTest(IOptions<OpenAiConfig> config)
    {
        _config = config;
    }

    public async Task Test(CancellationToken cancellationToken)
    {
        var client = new OpenAIClient(new Uri(_config.Value.Endpoint), new AzureKeyCredential(_config.Value.Key));

        const string outputPath = @"F:\Workarea\Documents\open-ai\timeline-output-5.json";

        //var result = await GetSocialDeterminants(client, cancellationToken);
        //var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented, s_serializerSettings);
        //File.WriteAllText(outputPath, resultJson);

        var result = await GetEvents(client, cancellationToken);
        var resultJson = JsonConvert.SerializeObject(result, Formatting.Indented, s_serializerSettings);
        File.WriteAllText(outputPath, resultJson);

        //var eventSetJson = File.ReadAllText(fullPath);
    }

    private async Task<TemplateResult<Event>> GetEvents(OpenAIClient client, CancellationToken cancellationToken)
    {
        var systemMessage = File.ReadAllText(@"F:\Workarea\Documents\open-ai\timeline-template.txt");
        var examples = File.ReadAllText(@"F:\Workarea\Documents\open-ai\timeline-examples.json");

        return await GetResponse<Event>(client, systemMessage, examples, cancellationToken);
    }

    private async Task<TemplateResult<SocialDeterminant>> GetSocialDeterminants(OpenAIClient client, CancellationToken cancellationToken)
    {
        var systemMessage = File.ReadAllText(@"F:\Workarea\Documents\open-ai\social-template.txt");
        var examples = File.ReadAllText(@"F:\Workarea\Documents\open-ai\social-examples.json");

        return await GetResponse<SocialDeterminant>(client, systemMessage, examples, cancellationToken);
    }

    private async Task<TemplateResult<T>> GetResponse<T>(OpenAIClient client, string systemMessage, string exampleJson, CancellationToken cancellationToken)
    {
        var notesJson = File.ReadAllText(@"F:\Workarea\Documents\open-ai\notes-west.json");
        var notes = JsonConvert.DeserializeObject<List<NoteData>>(notesJson)!;
        var examples = JsonConvert.DeserializeObject<List<OneShotExample<T>>>(exampleJson)!;

        var results = new ConcurrentBag<ItemSet<T>>();

        await notes.ForEachAsync(10, cancellationToken, async (note, cancellationToken) =>
        {
            if (string.IsNullOrWhiteSpace(note.Text))
                return;

            var response = await GetResponse(client, systemMessage, examples, note.Text, cancellationToken);

            if (!string.IsNullOrEmpty(response))
            {
                var items = ParseItems<T>(response);

                results.Add(new ItemSet<T>
                {
                    NoteId = note.NoteId,
                    Recorded = note.Recorded,
                    Items = items,
                });
            }
        });

        return new TemplateResult<T>
        {
            Template = systemMessage,
            Examples = examples,
            Results = results.ToList(),
        };
    }

    private async Task<string> GetResponse<T>(OpenAIClient client, string systemMessage, IList<OneShotExample<T>> examples, string requestText, CancellationToken cancellationToken)
    {
        var request = new ChatCompletionsOptions
        {
            Temperature = 1,
            MaxTokens = 800,
            NucleusSamplingFactor = (float)0.95,
            FrequencyPenalty = 0,
            PresencePenalty = 0,
        };

        request.Messages.Add(new ChatMessage(ChatRole.System, systemMessage));

        foreach (var example in examples)
        {
            request.Messages.Add(new ChatMessage(ChatRole.User, example.Request));
            request.Messages.Add(new ChatMessage(ChatRole.User, JsonConvert.SerializeObject(example.Response, Formatting.Indented, s_serializerSettings)));
        }

        request.Messages.Add(new ChatMessage(ChatRole.User, requestText));

        try
        {
            var response = await client.GetChatCompletionsAsync("chatgpt-playground", request, cancellationToken);
            var completions = response.Value;

            foreach (var choice in response.Value.Choices)
            {
                if (choice.Message.Role == ChatRole.Assistant)
                    return choice.Message.Content;
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex);
        }

        return string.Empty;
    }

    private List<T> ParseItems<T>(string content)
    {
        try
        {
            return JsonConvert.DeserializeObject<List<T>>(content) ?? new();
        }
        catch
        {
            Console.WriteLine("Failed to parse the following json.");
            Console.WriteLine(content);
            return new List<T>();
        }
    }

    private class TemplateResult<T>
    {
        public required string Template { get; init; }

        public List<OneShotExample<T>> Examples { get; init; } = new List<OneShotExample<T>>();

        public List<ItemSet<T>> Results { get; init; } = new List<ItemSet<T>>();
    }

    private class OneShotExample<T>
    {
        public required string Request { get; init; }
        public required List<T> Response { get; init; }
    }

    private class NoteData
    {
        public required string NoteId { get; set; }

        public required string Recorded { get; set; }

        public required string Text { get; set; }
    }

    private class SocialDeterminant
    {
        public required string Determinant { get; set; }
        public required string Source { get; set; }
        public required string Category { get; set; }
        public required bool Negative { get; set; }
        public required bool DocumentationRelated { get; set; }
    }

    private class ItemSet<T>
    {
        public required string NoteId { get; init; }

        public required string Recorded { get; init; }

        public required List<T> Items { get; init; }
    }

    private class Event
    {
        public required string Outcome { get; init; }
        public required string OutcomeImpact { get; init; }
        public required string Importance { get; init; }
        public required string Description { get; init; }
        public required string Source { get; init; }

        public List<string> Tags { get; init; } = new List<string>();
    }
}
