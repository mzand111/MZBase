using Microsoft.Extensions.Logging;

namespace MZBase.Microservices.HttpServices
{
    public abstract partial class ServiceMediatorBase<T>
    {
        protected async Task DeleteAsync(string apiUrl, Dictionary<string, string>? headers = null)
        {
            await DeleteAsync(apiUrl, headers, null, null);
        }

        protected async Task DeleteAsync(string apiUrl, Dictionary<string, string>? headers, RetryPolicy? retryPolicy)
        {
            await DeleteAsync(apiUrl, headers, null, retryPolicy);
        }

        /// <summary>
        /// Delete route with optional headers, request options, and retry support
        /// </summary>
        protected async Task DeleteAsync(string apiUrl, Dictionary<string, string>? headers, Action<HttpRequestOptions>? configureOptions, RetryPolicy? retryPolicy)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
               , "DeleteAsync"
               , _serviceUniqueName
               , _httpClientBaseAddress + apiUrl);

            await ExecuteWithRetryAndResponseAsync(
                async () =>
                {
                    using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Delete, null, headers, configureOptions: configureOptions))
                    {
                        var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
                        return (response, (object?)null);
                    }
                },
                "DeleteAsync",
                apiUrl,
                retryPolicy ?? DefaultSafeOperationRetryPolicy);

            _logger.LogInformation("Successfully called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                      , "DeleteAsync"
                      , _serviceUniqueName
                      , _httpClientBaseAddress + apiUrl);
        }
    }
}
