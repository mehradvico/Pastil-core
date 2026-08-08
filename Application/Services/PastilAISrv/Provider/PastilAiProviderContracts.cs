using Entities.Entities.PastilAIField;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.PastilAISrv.Provider
{
    public class PastilAiProviderOptions
    {
        public const string SectionName = "PastilAI";
        public int RequestTimeoutSeconds { get; set; } = 45;
        public string PublicMediaBaseUrl { get; set; }
        public List<PastilAiProviderDefinition> Providers { get; set; } = new();
    }

    public class PastilAiProviderDefinition
    {
        public string Name { get; set; }
        public string Kind { get; set; }
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public string ApiKeyEnvironmentVariable { get; set; }
        public string ChatCompletionsPath { get; set; } = "chat/completions";
        public string InstructionRole { get; set; } = "system";
        public string TextModel { get; set; }
        public string VisionModel { get; set; }
        public string ThinkingMode { get; set; }
        public int Order { get; set; }
        public bool Enabled { get; set; }
        public bool SupportsImage { get; set; }
        public bool SupportsAudio { get; set; }
        public bool SupportsVideo { get; set; }
        public bool UseJsonResponseFormat { get; set; } = true;
        public bool UseTemperature { get; set; } = true;
    }

    public static class PastilAiProviderDefinitionExtensions
    {
        public static string ResolveApiKey(this PastilAiProviderDefinition provider)
        {
            if (!string.IsNullOrWhiteSpace(provider?.ApiKey))
                return provider.ApiKey.Trim();

            if (string.IsNullOrWhiteSpace(provider?.ApiKeyEnvironmentVariable))
                return null;

            return Environment.GetEnvironmentVariable(provider.ApiKeyEnvironmentVariable)?.Trim();
        }
    }

    public class PastilAiProviderRequest
    {
        public string SystemPrompt { get; set; }
        public string UserMessage { get; set; }
        public string MediaDataUrl { get; set; }
        public string PreferredProvider { get; set; }
        public PastilAiInputType InputType { get; set; }
        public List<PastilAiProviderChatMessage> History { get; set; } = new();
    }

    public class PastilAiProviderChatMessage
    {
        public PastilAiMessageRole Role { get; set; }
        public string Content { get; set; }
    }

    public class PastilAiProviderResponse
    {
        public bool IsSuccess { get; set; }
        public string Answer { get; set; }
        public PastilAiScope Scope { get; set; }
        public bool IsEmergency { get; set; }
        public string Model { get; set; }
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? HttpStatusCode { get; set; }
        public string ErrorCode { get; set; }
        public string ErrorMessage { get; set; }
    }

    public interface IPastilAiProvider
    {
        string Name { get; }
        int Order { get; }
        bool CanHandle(bool hasImage);
        Task<PastilAiProviderResponse> CompleteAsync(PastilAiProviderRequest request, CancellationToken cancellationToken);
    }

    public class PastilAiRoutedResponse
    {
        public PastilAiProviderResponse Response { get; set; }
        public string Provider { get; set; }
        public List<PastilAiProviderAttemptResult> Attempts { get; set; } = new();
    }

    public class PastilAiProviderAttemptResult
    {
        public string Provider { get; set; }
        public string Model { get; set; }
        public int Order { get; set; }
        public DateTime StartDateUtc { get; set; }
        public DateTime EndDateUtc { get; set; }
        public PastilAiProviderResponse Response { get; set; }
    }

    public interface IPastilAiCompletionRouter
    {
        Task<PastilAiRoutedResponse> CompleteAsync(PastilAiProviderRequest request, CancellationToken cancellationToken);
    }
}
