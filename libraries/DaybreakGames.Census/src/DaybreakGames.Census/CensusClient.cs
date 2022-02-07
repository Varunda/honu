using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using DaybreakGames.Census.Exceptions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DaybreakGames.Census.Operators;
using DaybreakGames.Census.JsonConverters;

namespace DaybreakGames.Census
{
    public class CensusClient : ICensusClient
    {
        private readonly IOptions<CensusOptions> _options;
        private readonly ILogger<CensusClient> _logger;
        private readonly HttpClient _client;
        private readonly JsonSerializer _deserializer;

        public CensusClient(IOptions<CensusOptions> options, ILogger<CensusClient> logger)
        {
            _options = options;
            _logger = logger;

            _client = new HttpClient();

            var settings = new JsonSerializerSettings
            {
                ContractResolver = new UnderscorePropertyNamesContractResolver(),
                Converters = new JsonConverter[]
                {
                    new BooleanJsonConverter(),
                    new DateTimeJsonConverter()
                }
            };
            _deserializer = JsonSerializer.Create(settings);
        }

        public Task<IEnumerable<T>> ExecuteQueryList<T>(CensusQuery query)
        {
            return ExecuteQuery<IEnumerable<T>>(query);
        }

        public async Task<T> ExecuteQuery<T>(CensusQuery query)
        {
            var requestUri = CreateRequestUri(query);
            _logger.LogTrace(85400, $"Getting Census request for: \"{ requestUri}\"");

            try
            {
                HttpResponseMessage result;

                try
                {
                    result = await _client.GetAsync(requestUri);
                }
                catch (HttpRequestException ex)
                {
                    var exMessage = ex.InnerException?.Message ?? ex.Message;
                    var errorMessage = $"Census query failed for query: {requestUri}: {exMessage}";

                    throw new CensusConnectionException(errorMessage);
                }

                _logger.LogTrace(85401, $"Census Request completed with status code: {result.StatusCode}");

                if (!result.IsSuccessStatusCode)
                {
                    throw new CensusConnectionException($"Census returned status code {result.StatusCode}");
                }

                JToken jResult;

                try
                {
                    var serializedString = await result.Content.ReadAsStringAsync();
                    jResult = JsonConvert.DeserializeObject<JToken>(serializedString, new JsonSerializerSettings
                    {
                        ContractResolver = new UnderscorePropertyNamesContractResolver()
                    });
                }
                catch (JsonReaderException)
                {
                    throw new CensusException("Failed to read JSON. Endpoint may be in maintence mode.");
                }

                var error = jResult.Value<string>("error");
                var errorCode = jResult.Value<string>("errorCode");

                if (error != null)
                {
                    if (error == "service_unavailable")
                    {
                        throw new CensusServiceUnavailableException();
                    }
                    else
                    {
                        throw new CensusServerException(error);
                    }
                }
                else if (errorCode != null)
                {
                    var errorMessage = jResult.Value<string>("errorMessage");

                    throw new CensusServerException($"{errorCode}: {errorMessage}");
                }

                var jBody = jResult.SelectToken($"{query.ServiceName}_list");
                return Convert<T>(jBody);
            }
            catch(Exception ex)
            {
                HandleCensusExceptions(ex, requestUri);
                throw ex;
            }
        }

        public async Task<IEnumerable<T>> ExecuteQueryBatch<T>(CensusQuery query)
        {
            var count = 0;
            List<JToken> batchResult = new List<JToken>();

            if (query.Limit == null)
            {
                query.SetLimit(Constants.DefaultBatchLimit);
            }

            if (query.Start == null)
            {
                query.SetStart(count);
            }

            var result = await ExecuteQueryList<JToken>(query);

            if (result.Count() < Constants.DefaultBatchLimit)
            {
                return result.Select(r => Convert<T>(r));
            }

            do
            {
                batchResult.AddRange(result);

                if (result.Count() < Constants.DefaultBatchLimit)
                {
                    return batchResult.Select(r => Convert<T>(r));
                }

                count += result.Count();
                query.SetStart(count);

                result = await ExecuteQuery<JToken>(query);
            } while (result.Any());

            return batchResult.Select(r => Convert<T>(r));
        }

        public Uri CreateRequestUri(CensusQuery query)
        {
            var sId = query.ServiceId ?? _options.Value.CensusServiceId;
            var ns = query.ServiceNamespace ?? _options.Value.CensusServiceNamespace;

            var encArgs = query.ToString();
            return new Uri($"http://{Constants.CensusEndpoint}/s:{sId}/get/{ns}/{encArgs}");
        }

        private void HandleCensusExceptions(Exception ex, Uri query)
        {
            if (!_options.Value.LogCensusErrors)
            {
                return;
            }

            switch(ex)
            {
                case CensusServiceUnavailableException cex:
                    _logger.LogError(84531, cex, "Census service unavailable during query: {0}", query);
                    break;
                case CensusServerException cex:
                    _logger.LogError(84532, cex, "Census server failed for query: {0}", query);
                    break;
                case CensusConnectionException cex:
                    _logger.LogError(84533, cex, "Census connection failed for query: {0}", query);
                    break;
                default:
                    _logger.LogError(84530, ex, "Unknown exception throw when processing census query: {0}", query);
                    break;
            }
        }

        private T Convert<T>(JToken content)
        {
            return content.ToObject<T>(_deserializer);
        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
