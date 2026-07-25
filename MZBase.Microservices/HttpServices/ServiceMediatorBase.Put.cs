using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

namespace MZBase.Microservices.HttpServices
{
    public abstract partial class ServiceMediatorBase<T>
    {
        protected async Task<TOut> PutAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            return await PutAsync<TIn, TOut>(item, apiUrl, headers, null, null);
        }

        protected async Task<TOut> PutAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy) where TIn : class
        {
            return await PutAsync<TIn, TOut>(item, apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Put Async Method with optional headers, request options, and retry support
        /// </summary>
        protected async Task<TOut> PutAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PutAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            var result = await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Put, content, headers, configureOptions: configureOptions))
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
                                    if (stream != null && stream.Length > 0)
                                    {
                                        var apiResponseDto = await JsonSerializer.DeserializeAsync<TOut>(stream);
                                        deserializedResult = apiResponseDto;
                                    }
                                    else
                                    {
                                        deserializedResult = default!;
                                    }
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
                            _logger.LogError(ex, "Failed to do deserialize output after successful put method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                                + exMessage
                             , "PutAsync"
                             , _serviceUniqueName
                             , _httpClientBaseAddress + apiUrl);

                            throw new Exception("Failed to deserialize the response:" + exMessage);
                        }

                        return (response, deserializedResult!);
                    }
                },
                "PutAsync",
                apiUrl,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                  , "PutAsync"
                  , _serviceUniqueName
                  , _httpClientBaseAddress + apiUrl);

            return result;
        }

        protected async Task PutAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            await PutAsync(item, apiUrl, headers, null, null);
        }

        protected async Task PutAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy) where TIn : class
        {
            await PutAsync(item, apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Put Async Method with optional headers, request options, and retry support
        /// </summary>
        protected async Task PutAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PutAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Put, content, headers, configureOptions: configureOptions))
                    {
                        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                        return (response, (object?)null);
                    }
                },
                "PutAsync",
                apiUrl,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                  , "PutAsync"
                  , _serviceUniqueName
                  , _httpClientBaseAddress + apiUrl);
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPutAsync<TIn, TOut>(TIn item, string apiUrl) where TIn : class
        {
            return await TryPutAsync<TIn, TOut>(item, apiUrl, null, null, null);
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPutAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers) where TIn : class
        {
            return await TryPutAsync<TIn, TOut>(item, apiUrl, headers, null, null);
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPutAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy) where TIn : class
        {
            return await TryPutAsync<TIn, TOut>(item, apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Puts data with optional headers, request options, and retry policy.
        /// Does NOT throw on deserialization failure - caller can inspect the response content.
        /// </summary>
        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPutAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "TryPutAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            var (response, result) = await ExecuteWithRetryAndResponseForTryAsync(
                async () =>
                {
                    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Put, content, headers, configureOptions: configureOptions))
                    using (var httpResponse = await _httpClient.SendAsync(request).ConfigureAwait(false))
                    {
                        string responseContent = await httpResponse.Content.ReadAsStringAsync();

                        if (!httpResponse.IsSuccessStatusCode)
                        {
                            _logger.LogWarning("PUT request returned non-success status {StatusCode} for '{RemoteAddress}'. Response: {ResponsePreview}",
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
                "TryPutAsync",
                apiUrl,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Completed method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'. StatusCode: {StatusCode}. HasResult: {HasResult}"
                  , "TryPutAsync"
                  , _serviceUniqueName
                  , _httpClientBaseAddress + apiUrl
                  , response.StatusCode
                  , result != null);

            return (response, result);
        }
    }
}
