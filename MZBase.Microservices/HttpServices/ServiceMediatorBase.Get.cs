using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MZBase.Microservices.HttpServices
{
    public abstract partial class ServiceMediatorBase<T>
    {
        protected async Task GetAsync(string address)
        {
            await GetAsync(address, null, null, null);
        }

        /// <summary>
        /// Performs a GET request with optional retry policy.
        /// </summary>
        protected async Task GetAsync(string address, RetryPolicy? retryPolicy)
        {
            await GetAsync(address, null, null, retryPolicy);
        }

        /// <summary>
        /// Performs a GET request with optional headers, request options, and retry policy.
        /// </summary>
        protected async Task GetAsync(string address, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "GetAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);

            await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using var request = CreateHttpRequest(_httpClientBaseAddress + address, HttpMethod.Get, null, headers, configureOptions: configureOptions);
                    var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                    return (response, (object?)null);
                },
                "GetAsync",
                address,
                retryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "GetAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + address);
        }

        protected async ValueTask<TOut> GetAsync<TOut>(string address)
        {
            return await GetAsync<TOut>(address, null, null, null);
        }

        /// <summary>
        /// Performs a GET request with deserialization and optional retry policy.
        /// </summary>
        protected async ValueTask<TOut> GetAsync<TOut>(string address, RetryPolicy? retryPolicy)
        {
            return await GetAsync<TOut>(address, null, null, retryPolicy);
        }

        /// <summary>
        /// Performs a GET request with deserialization, optional headers, request options, and retry policy.
        /// </summary>
        protected async ValueTask<TOut> GetAsync<TOut>(string address, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "GetAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);

            var result = await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using var request = CreateHttpRequest(_httpClientBaseAddress + address, HttpMethod.Get, null, headers, configureOptions: configureOptions);
                    using var httpResponseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return (httpResponseMessage, default(TOut)!);
                    }

                    TOut? deserializedResult;
                    try
                    {
                        if (typeof(TOut) == typeof(String))
                        {
                            string s = await httpResponseMessage.Content.ReadAsStringAsync();
                            deserializedResult = (TOut)(object)s;
                        }
                        else
                        {
                            using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                            {
                                var apiResponseDto = await JsonSerializer.DeserializeAsync<TOut>(stream, _defaultSerializerOptions);
                                deserializedResult = apiResponseDto;
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string exMessage = ex.Message;
                        if (ex.InnerException != null)
                        {
                            exMessage += ",Inner message" + ex.InnerException.Message;
                        }

                        _logger.LogError(ex, "Failed to do deserialize output after successful get method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                            + exMessage
                         , "GetAsync"
                         , _serviceUniqueName
                         , _httpClientBaseAddress + address);

                        throw new Exception("Error reading content and deserializing after successfully called remote procedure:" + exMessage);
                    }

                    return (httpResponseMessage, deserializedResult!);
                },
                "GetAsync",
                address,
                retryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "GetAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + address);

            return result;
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryGetAsync<TOut>(string address)
        {
            return await TryGetAsync<TOut>(address, null, null, null);
        }

        /// <summary>
        /// Performs a GET request with retry support and attempts to deserialize.
        /// Does NOT throw on deserialization failure - caller can inspect the response content.
        /// </summary>
        public async Task<(HttpResponseMessage Response, TOut? Result)> TryGetAsync<TOut>(string address, RetryPolicy? retryPolicy)
        {
            return await TryGetAsync<TOut>(address, null, null, retryPolicy);
        }

        /// <summary>
        /// Performs a GET request with optional headers, request options, retry support, and attempts to deserialize.
        /// Does NOT throw on deserialization failure - caller can inspect the response content.
        /// </summary>
        public async Task<(HttpResponseMessage Response, TOut? Result)> TryGetAsync<TOut>(string address, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "TryGetAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);

            var (response, result) = await ExecuteWithRetryAndResponseForTryAsync(
                async () =>
                {
                    using var request = CreateHttpRequest(_httpClientBaseAddress + address, HttpMethod.Get, null, headers, configureOptions: configureOptions);
                    using var httpResponseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return (httpResponseMessage, default(TOut?));
                    }

                    string responseContent = await httpResponseMessage.Content.ReadAsStringAsync();

                    if (!httpResponseMessage.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("GET request returned non-success status {StatusCode} for '{RemoteAddress}'. Response: {ResponsePreview}",
                            httpResponseMessage.StatusCode,
                            _httpClientBaseAddress + address,
                            TruncateContent(responseContent, 500));

                        var newResponse = new HttpResponseMessage(httpResponseMessage.StatusCode)
                        {
                            Content = new StringContent(responseContent, Encoding.UTF8, httpResponseMessage.Content.Headers.ContentType?.MediaType ?? "application/json"),
                            RequestMessage = httpResponseMessage.RequestMessage
                        };

                        foreach (var header in httpResponseMessage.Headers)
                        {
                            newResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
                        }

                        httpResponseMessage.Dispose();
                        return (newResponse, default(TOut?));
                    }

                    TOut? deserializedResult = await SafeDeserializeAsync<TOut>(
                        new StringContent(responseContent, Encoding.UTF8, httpResponseMessage.Content.Headers.ContentType?.MediaType ?? "application/json"),
                        responseContent);

                    return (httpResponseMessage, deserializedResult);
                },
                "TryGetAsync",
                address,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Completed method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'. StatusCode: {StatusCode}. HasResult: {HasResult}"
                   , "TryGetAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + address
                   , response.StatusCode
                   , result != null);

            return (response, result);
        }
    }
}
