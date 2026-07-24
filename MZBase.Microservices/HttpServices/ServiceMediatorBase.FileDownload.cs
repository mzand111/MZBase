using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Http.Headers;

namespace MZBase.Microservices.HttpServices
{
    public abstract partial class ServiceMediatorBase<T>
    {
        protected HttpRequestMessage CreateHttpRequest(string apiUrl, HttpMethod httpMethod, HttpContent content, Dictionary<string, string>? headers = null, string acceptType = "application/json", Action<HttpRequestOptions>? configureOptions = null)
        {
            var request = new HttpRequestMessage();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
            request.RequestUri = new Uri(apiUrl);
            request.Method = httpMethod;
            request.Content = content;

            if (headers != null)
            {
                foreach (var header in headers)
                {
                    if (string.Equals(header.Key, "revision", StringComparison.OrdinalIgnoreCase))
                    {
                        request.Headers.Add("If-Match", "\"" + header.Value + "\"");
                    }
                    else
                    {
                        request.Headers.TryAddWithoutValidation(header.Key, header.Value);
                    }
                }
            }

            if (configureOptions != null)
            {
                configureOptions(request.Options);
            }

            return request;
        }

        protected async Task ProcessNotSuccessfulResponse(HttpResponseMessage? response, string apiUrl, string methodName)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authorization error calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , methodName
               , _serviceUniqueName
               , _httpClientBaseAddress + apiUrl);

                throw new UnauthorizedAccessException("Service call not authorized: '" + _httpClientBaseAddress + apiUrl + "', Reason Phrase:" + response.ReasonPhrase);
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError("Not found error calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                    , methodName
                    , _serviceUniqueName
                    , _httpClientBaseAddress + apiUrl);
                throw new Exception("Service address not found: '" + _httpClientBaseAddress + apiUrl + "', Reason Phrase:" + response.ReasonPhrase);
            }

            string? responsContent = null;
            try
            {
                responsContent = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            }
            catch
            {

            }
            if (string.IsNullOrWhiteSpace(responsContent))
            {
                _logger.LogError("Failed calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' reading response content was not posible or no response:" + response.ReasonPhrase
                    , methodName
                    , _serviceUniqueName
                    , _httpClientBaseAddress + apiUrl);

                throw new Exception("Failed to do post method:'" + _httpClientBaseAddress + apiUrl + "', Reason Phrase:" + response.ReasonPhrase);
            }
            else
            {
                responsContent = responsContent.Replace("{", "[");
                responsContent = responsContent.Replace("}", "]");
                responsContent = responsContent.Replace("\"", "");

                _logger.LogError("Failed to do post method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' ReasonPhrase:" + response.ReasonPhrase
                    + ",responseContent:" + responsContent
                  , methodName
                  , _serviceUniqueName
                  , _httpClientBaseAddress + apiUrl);

                throw new Exception("Failed calling remote procedure:'" + _httpClientBaseAddress + apiUrl + "', Reason Phrase:" + response.ReasonPhrase + "," + responsContent);
            }
        }

        public async ValueTask<(byte[], string contentType, string fileName)> GetFileContentAsync(string address)
        {
            return await GetFileContentAsync(address, null, null);
        }

        public async ValueTask<(byte[], string contentType, string fileName)> GetFileContentAsync(string address, RetryPolicy? retryPolicy)
        {
            return await GetFileContentAsync(address, null, retryPolicy);
        }

        /// <summary>
        /// Get file content with optional headers and retry support
        /// </summary>
        public async ValueTask<(byte[], string contentType, string fileName)> GetFileContentAsync(string address, Dictionary<string, string>? headers, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "GetFileContentAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);

            var result = await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using var request = CreateHttpRequest(_httpClientBaseAddress + address, HttpMethod.Get, null, headers);
                    using var httpResponseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return (httpResponseMessage, default((byte[], string, string)));
                    }

                    using var ras = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var contentType = httpResponseMessage.Content.Headers.ContentType?.ToString() ?? "application/octet-stream";
                    MemoryStream outs = new MemoryStream();
                    string fileName = httpResponseMessage.Content.Headers.ContentDisposition?.FileName ?? "file";
                    ras.CopyTo(outs);

                    return (httpResponseMessage, (outs.ToArray(), contentType, fileName));
                },
                "GetFileContentAsync",
                address,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "GetFileContentAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + address);

            return result;
        }

        public async ValueTask<FileContentResult> GetFileContentResultAsync(string address)
        {
            return await GetFileContentResultAsync(address, null, null);
        }

        public async ValueTask<FileContentResult> GetFileContentResultAsync(string address, RetryPolicy? retryPolicy)
        {
            return await GetFileContentResultAsync(address, null, retryPolicy);
        }

        /// <summary>
        /// Get file content result with optional headers and retry support
        /// </summary>
        public async ValueTask<FileContentResult> GetFileContentResultAsync(string address, Dictionary<string, string>? headers, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "GetFileContentResultAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);

            var result = await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using var request = CreateHttpRequest(_httpClientBaseAddress + address, HttpMethod.Get, null, headers);
                    using var httpResponseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return (httpResponseMessage, default(FileContentResult)!);
                    }

                    var fileBytes = await httpResponseMessage.Content.ReadAsByteArrayAsync();
                    var contentType = httpResponseMessage.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    var fileContentResult = new FileContentResult(fileBytes, contentType);

                    return (httpResponseMessage, fileContentResult);
                },
                "GetFileContentResultAsync",
                address,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "GetFileContentResultAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + address);

            return result;
        }

        public async ValueTask<FileStreamResult> GetFileStreamAsync(string address)
        {
            return await GetFileStreamAsync(address, null, null);
        }

        public async ValueTask<FileStreamResult> GetFileStreamAsync(string address, RetryPolicy? retryPolicy)
        {
            return await GetFileStreamAsync(address, null, retryPolicy);
        }

        /// <summary>
        /// Get file stream with optional headers and retry support
        /// </summary>
        public async ValueTask<FileStreamResult> GetFileStreamAsync(string address, Dictionary<string, string>? headers, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "GetFileStreamAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);

            var result = await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using var request = CreateHttpRequest(_httpClientBaseAddress + address, HttpMethod.Get, null, headers);
                    using var httpResponseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false);

                    if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                    {
                        return (httpResponseMessage, default(FileStreamResult)!);
                    }

                    var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false);
                    var contentType = httpResponseMessage.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                    var fileStreamResult = new FileStreamResult(stream, contentType);

                    return (httpResponseMessage, fileStreamResult);
                },
                "GetFileStreamAsync",
                address,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                   , "GetFileStreamAsync"
                   , _serviceUniqueName
                   , _httpClientBaseAddress + address);

            return result;
        }
    }
}
