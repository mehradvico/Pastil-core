using Application.Services.CommonSrv.SearchSrv.Dto;
using Application.Services.CommonSrv.SearchSrv.Iface;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Services.CommonSrv.SearchSrv
{
    public class HttpSearchSemanticReranker : ISearchSemanticReranker
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SearchHybridOptions _options;
        private readonly ILogger<HttpSearchSemanticReranker> _logger;

        public HttpSearchSemanticReranker(
            IHttpClientFactory httpClientFactory,
            IOptions<SearchHybridOptions> options,
            ILogger<HttpSearchSemanticReranker> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<List<SearchItemDto>> RerankAsync(
            string query,
            List<SearchItemDto> items,
            CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled || string.IsNullOrWhiteSpace(_options.Endpoint) || items.Count == 0)
                return items;

            try
            {
                using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                timeout.CancelAfter(TimeSpan.FromSeconds(Math.Clamp(_options.TimeoutSeconds, 1, 10)));

                var client = _httpClientFactory.CreateClient();
                if (!string.IsNullOrWhiteSpace(_options.ApiKey))
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);

                var payload = new
                {
                    query,
                    items = items.Select(item => new
                    {
                        type = item.Type.ToString(),
                        item.Id,
                        item.Title,
                        item.SubTitle
                    })
                };

                using var response = await client.PostAsJsonAsync(_options.Endpoint, payload, timeout.Token);
                if (!response.IsSuccessStatusCode) return items;

                var result = await response.Content.ReadFromJsonAsync<SemanticRerankResponse>(cancellationToken: timeout.Token);
                var semanticScores = result?.Scores?.ToDictionary(
                    item => $"{item.Type}:{item.Id}",
                    item => Math.Clamp(item.Score, 0, 1),
                    StringComparer.OrdinalIgnoreCase) ?? new Dictionary<string, double>();

                var semanticWeight = Math.Clamp(_options.SemanticWeight, 0, 0.6);
                foreach (var item in items)
                {
                    if (!semanticScores.TryGetValue($"{item.Type}:{item.Id}", out var semanticScore)) continue;
                    item.Score = item.Score * (1 - semanticWeight) + semanticScore * 120 * semanticWeight;
                    item.MatchedBy = $"{item.MatchedBy},semantic";
                }

                return items.OrderByDescending(item => item.Score).ThenBy(item => item.Type).ToList();
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Semantic search reranker timed out; lexical results were used.");
                return items;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Semantic search reranker failed; lexical results were used.");
                return items;
            }
        }

        private sealed class SemanticRerankResponse
        {
            public List<SemanticScore> Scores { get; set; } = [];
        }

        private sealed class SemanticScore
        {
            public string Type { get; set; }
            public long Id { get; set; }
            public double Score { get; set; }
        }
    }
}
