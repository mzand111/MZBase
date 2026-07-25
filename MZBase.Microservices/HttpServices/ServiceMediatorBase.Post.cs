using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MZBase.Microservices.HttpServices
{
    public abstract partial class ServiceMediatorBase<T>
    {
        protected async Task PostAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            await PostAsync(item, apiUrl, headers, null, null);
        }

        protected async Task PostAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy) where TIn : class
        {
            await PostAsync(item, apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Post Async Method with optional headers, request options, and retry support
        /// </summary>
        protected async Task PostAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'", "PostAsync", _serviceUniqueName, _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, content, headers, configureOptions: configureOptions))
                    {
                        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                        return (response, (object?)null);
                    }
                },
                "PostAsync",
                apiUrl,
                retryPolicy ?? DefaultMutatingOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "PostAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + apiUrl);
        }

        protected async Task<TOut> PostAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            return await PostAsync<TIn, TOut>(item, apiUrl, headers, null, null);
        }

        protected async Task<TOut> PostAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy) where TIn : class
        {
            return await PostAsync<TIn, TOut>(item, apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Post Async Method with optional headers, request options, and retry support
        /// </summary>
        protected async Task<TOut> PostAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'", "PostAsync", _serviceUniqueName, _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            var result = await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, content, headers, configureOptions: configureOptions))
                    using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        TOut? deserializedResult;
                        try
                        {
                            if (typeof(TOut) == typeof(String))
                            {
                                string s = await response.Content.ReadAsStringAsync();
                                if (s.StartsWith("\"") && s.EndsWith("\"") && s.Length > 1)
                                {
                                    s = s.Substring(1, s.Length - 2);
                                }
                                deserializedResult = (TOut)(object)s;
                            }
                            else
                            {
                                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
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
                            _logger.LogError(ex, "Failed to do deserialize output after successful post method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                                + exMessage
                             , "PostAsync"
                             , _serviceUniqueName
                             , _httpClientBaseAddress + apiUrl);

                            throw new Exception("Failed to deserialize the response:" + exMessage);
                        }

                        return (response, deserializedResult!);
                    }
                },
                "PostAsync",
                apiUrl,
                retryPolicy ?? DefaultMutatingOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "PostAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + apiUrl);

            return result;
        }

        protected async Task PostAsync(string apiUrl, Dictionary<string, string>? headers = null)
        {
            await PostAsync(apiUrl, headers, null, null);
        }

        protected async Task PostAsync(string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy)
        {
            await PostAsync(apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Post async with optional headers, request options, and retry support
        /// </summary>
        protected async Task PostAsync(string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'", "PostAsync", _serviceUniqueName, _httpClientBaseAddress + apiUrl);

            await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, null, headers, configureOptions: configureOptions))
                    {
                        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                        return (response, (object?)null);
                    }
                },
                "PostAsync",
                apiUrl,
                retryPolicy ?? DefaultMutatingOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "PostAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + apiUrl);
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPostAsync<TIn, TOut>(TIn item, string apiUrl) where TIn : class
        {
            return await TryPostAsync<TIn, TOut>(item, apiUrl, null, null, null);
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPostAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers) where TIn : class
        {
            return await TryPostAsync<TIn, TOut>(item, apiUrl, headers, null, null);
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPostAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy) where TIn : class
        {
            return await TryPostAsync<TIn, TOut>(item, apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Posts data with optional headers, request options, and retry policy.
        /// Does NOT throw on deserialization failure - caller can inspect the response content.
        /// </summary>
        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPostAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'", "TryPostAsync", _serviceUniqueName, _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            var (response, result) = await ExecuteWithRetryAndResponseForTryAsync(
                async () =>
                {
                    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, content, headers, configureOptions: configureOptions))
                    using (var httpResponse = await _httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        string responseContent = await httpResponse.Content.ReadAsStringAsync();

                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("POST request returned non-success status {StatusCode} for '{RemoteAddress}'. Response: {ResponsePreview}",
                                httpResponse.StatusCode,
                                _httpClientBaseAddress + apiUrl,
                                TruncateContent(responseContent, 500));

                            var newResponse = new HttpResponseMessage(httpResponse.StatusCode)
                            {
                                Content = new StringContent(responseContent, Encoding.UTF8, httpResponse.Content.Headers.ContentType?.MediaType ?? "application/json"),
                                RequestMessage = httpResponse.RequestMessage
                            };

                            foreach (var header in httpResponse.Headers)
                            {
                                newResponse.Headers.TryAddWithoutValidation(header.Key, header.Value);
                            }

                            httpResponse.Dispose();
                            return (newResponse, default(TOut?));
                        }

                        TOut? deserializedResult = await SafeDeserializeAsync<TOut>(
                            new StringContent(responseContent, Encoding.UTF8, httpResponse.Content.Headers.ContentType?.MediaType ?? "application/json"),
                            responseContent);

                        return (httpResponse, deserializedResult);
                    }
                },
                "TryPostAsync",
                apiUrl,
                retryPolicy ?? DefaultMutatingOperationRetryPolicy);

            _logger.LogInformation("Completed method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'. StatusCode: {StatusCode}. HasResult: {HasResult}"
                   , "TryPostAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + apiUrl
                   , response.StatusCode
                   , result != null);

            return (response, result);
        }
    }
}
