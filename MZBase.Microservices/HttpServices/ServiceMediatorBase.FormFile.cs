using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MZBase.Microservices.HttpServices
{
    public abstract partial class ServiceMediatorBase<T>
    {
        public async Task PostFormFileAsync(IFormFile content, string apiUrl)
        {
            await PostFormFileAsync(content, apiUrl, null, null, null);
        }

        public async Task PostFormFileAsync(IFormFile content, string apiUrl, RetryPolicy? retryPolicy)
        {
            await PostFormFileAsync(content, apiUrl, null, null, retryPolicy);
        }

        /// <summary>
        /// Post form file with optional headers, request options, and retry support
        /// </summary>
        public async Task PostFormFileAsync(IFormFile content, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PostFormFileAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);

            using (var target = new MemoryStream())
            {
                content.CopyTo(target);
                var fileBytes = target.ToArray();

                await ExecuteWithRetryAndResponseAsync(
                    async () =>
                    {
                        var multipartContent = new MultipartFormDataContent();
                        multipartContent.Add(new ByteArrayContent(fileBytes), "file", Path.GetFileName(content.FileName));

                        using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, multipartContent, headers, configureOptions: configureOptions))
                        {
                            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                            return (response, (object?)null);
                        }
                    },
                    "PostFormFileAsync",
                    apiUrl,
                    retryPolicy ?? DefaultMutatingOperationRetryPolicy);
            }

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                 , "PostFormFileAsync"
                 , _serviceUniqueName
                 , _httpClientBaseAddress + apiUrl);
        }

        public async Task<TOut> PostFormFileAsync<TOut>(IFormFile content, string apiUrl)
        {
            return await PostFormFileAsync<TOut>(content, apiUrl, null, null, null);
        }

        public async Task<TOut> PostFormFileAsync<TOut>(IFormFile content, string apiUrl, RetryPolicy? retryPolicy)
        {
            return await PostFormFileAsync<TOut>(content, apiUrl, null, null, retryPolicy);
        }

        /// <summary>
        /// Post form file with deserialization, optional headers, request options, and retry support
        /// </summary>
        public async Task<TOut> PostFormFileAsync<TOut>(IFormFile content, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PostFormFileAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);

            using (var target = new MemoryStream())
            {
                content.CopyTo(target);
                var fileBytes = target.ToArray();

                var result = await ExecuteWithRetryAndResponseAsync(
                    async () =>
                    {
                        var multipartContent = new MultipartFormDataContent();
                        multipartContent.Add(new ByteArrayContent(fileBytes), "file", Path.GetFileName(content.FileName));

                        using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, multipartContent, headers, configureOptions: configureOptions))
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
                                _logger.LogError(ex, "Failed to do deserialize output after successful post method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                                    + exMessage
                                 , "PostFormFileAsync"
                                 , _serviceUniqueName
                                 , _httpClientBaseAddress + apiUrl);

                                throw new Exception("Failed to deserialize the response:" + exMessage);
                            }

                            return (response, deserializedResult!);
                        }
                    },
                    "PostFormFileAsync",
                    apiUrl,
                    retryPolicy ?? DefaultMutatingOperationRetryPolicy);

                _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                     , "PostFormFileAsync"
                     , _serviceUniqueName
                     , _httpClientBaseAddress + apiUrl);

                return result;
            }
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPostFormFileAsync<TOut>(IFormFile content, string apiUrl)
        {
            return await TryPostFormFileAsync<TOut>(content, apiUrl, null, null, null);
        }

        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPostFormFileAsync<TOut>(IFormFile content, string apiUrl, RetryPolicy? retryPolicy)
        {
            return await TryPostFormFileAsync<TOut>(content, apiUrl, null, null, retryPolicy);
        }

        /// <summary>
        /// Posts a file with optional headers, request options, and retry policy.
        /// Does NOT throw on deserialization failure - caller can inspect the response content.
        /// </summary>
        public async Task<(HttpResponseMessage Response, TOut? Result)> TryPostFormFileAsync<TOut>(IFormFile content, string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "TryPostFormFileAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);

            using (var target = new MemoryStream())
            {
                content.CopyTo(target);
                var fileBytes = target.ToArray();

                var (response, result) = await ExecuteWithRetryAndResponseForTryAsync(
                    async () =>
                    {
                        var multipartContent = new MultipartFormDataContent();
                        multipartContent.Add(new ByteArrayContent(fileBytes), "file", Path.GetFileName(content.FileName));

                        using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, multipartContent, headers, configureOptions: configureOptions))
                        {
                            var httpResponse = await _httpClient.SendAsync(request).ConfigureAwait(false);
                            string responseContent = await httpResponse.Content.ReadAsStringAsync();

                            if (!httpResponse.IsSuccessStatusCode)
                            {
                                _logger.LogWarning("POST (form file) request returned non-success status {StatusCode} for '{RemoteAddress}'. Response: {ResponsePreview}",
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
                    "TryPostFormFileAsync",
                    apiUrl,
                    retryPolicy ?? DefaultMutatingOperationRetryPolicy);

                _logger.LogInformation("Completed method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'. StatusCode: {StatusCode}. HasResult: {HasResult}"
                     , "TryPostFormFileAsync"
                     , _serviceUniqueName
                     , _httpClientBaseAddress + apiUrl
                     , response.StatusCode
                     , result != null);

                return (response, result);
            }
        }
    }
}
