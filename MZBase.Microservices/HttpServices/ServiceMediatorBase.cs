using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace MZBase.Microservices.HttpServices
{
    /// <summary>
    /// Base class for HTTP microservice proxies with retry resilience and high-load support.
    /// Supports exponential backoff, jitter, and configurable retry policies for robust API calls.
    /// </summary>
    public abstract partial class ServiceMediatorBase<T> : IMicroServiceProxy
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _httpClientBaseAddress;
        protected readonly string _serviceUniqueName;
        protected readonly ILogger<T> _logger;

        /// <summary>
        /// Default retry policy for GET/DELETE operations (safe, idempotent)
        /// </summary>
        protected RetryPolicy DefaultSafeOperationRetryPolicy { get; set; } = RetryPolicy.CreateDefaultForSafeOperations();

        /// <summary>
        /// Default retry policy for POST/PUT operations (potentially non-idempotent)
        /// </summary>
        protected RetryPolicy DefaultMutatingOperationRetryPolicy { get; set; } = RetryPolicy.CreateDefaultForPostOperations();

        public ServiceMediatorBase(HttpClient httpClient, ILogger<T> logger, string serviceBaseAddress, string serviceUniqueName)
        {
            _httpClient = httpClient;
            _httpClientBaseAddress = serviceBaseAddress;
            _serviceUniqueName = serviceUniqueName;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_httpClientBaseAddress);
        }

        /// <summary>
        /// Constructor with timeout configuration for high-load scenarios.
        /// Timeout applies to individual HTTP requests.
        /// </summary>
        /// <param name="httpClient">The HttpClient to use for requests</param>
        /// <param name="logger">The logger instance</param>
        /// <param name="serviceBaseAddress">The base address of the service</param>
        /// <param name="serviceUniqueName">A unique name for this service</param>
        /// <param name="requestTimeoutMs">Timeout in milliseconds for individual HTTP requests (default: 30000ms = 30s)</param>
        public ServiceMediatorBase(HttpClient httpClient, ILogger<T> logger, string serviceBaseAddress, string serviceUniqueName, int requestTimeoutMs)
        {
            _httpClient = httpClient;
            _httpClientBaseAddress = serviceBaseAddress;
            _serviceUniqueName = serviceUniqueName;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_httpClientBaseAddress);
            _httpClient.Timeout = TimeSpan.FromMilliseconds(requestTimeoutMs);
        }

        #region Retry Engine

        /// <summary>
        /// Executes an async operation with retry logic using exponential backoff and jitter.
        /// </summary>
        protected async Task<TResult> ExecuteWithRetryAsync<TResult>(
            Func<Task<TResult>> operation,
            string operationName,
            RetryPolicy? retryPolicy = null)
        {
            retryPolicy ??= DefaultSafeOperationRetryPolicy;

            if (retryPolicy.MaxRetries == 0)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Operation '{OperationName}' failed with no retries configured.", operationName);
                    throw;
                }
            }

            Exception? lastException = null;

            for (int attempt = 0; attempt <= retryPolicy.MaxRetries; attempt++)
            {
                try
                {
                    return await operation();
                }
                catch (Exception ex) when (attempt < retryPolicy.MaxRetries && ShouldRetry(ex, retryPolicy))
                {
                    lastException = ex;
                    int delayMs = retryPolicy.CalculateDelayMs(attempt);

                    _logger.LogWarning(
                        "Retry attempt {RetryAttempt} of {MaxRetries} for operation '{OperationName}' after {DelayMs}ms. Exception: {ExceptionType}: {Message}",
                        attempt + 1,
                        retryPolicy.MaxRetries,
                        operationName,
                        delayMs,
                        ex.GetType().Name,
                        ex.Message);

                    await Task.Delay(delayMs);
                }
                catch (Exception ex) when (!ShouldRetry(ex, retryPolicy) || attempt >= retryPolicy.MaxRetries)
                {
                    _logger.LogError(ex, "Operation '{OperationName}' failed and will not be retried. Exception: {ExceptionType}: {Message}",
                        operationName,
                        ex.GetType().Name,
                        ex.Message);
                    throw;
                }
            }

            if (lastException != null)
            {
                _logger.LogError(lastException, "Operation '{OperationName}' failed after {Attempts} attempts.", operationName, retryPolicy.MaxRetries + 1);
                throw lastException;
            }

            throw new InvalidOperationException($"Retry logic failed for operation '{operationName}'");
        }

        /// <summary>
        /// Executes an async operation with retry logic and response validation.
        /// </summary>
        protected async Task<TResult> ExecuteWithRetryAndResponseAsync<TResult>(
            Func<Task<(HttpResponseMessage Response, TResult Result)>> operation,
            string operationName,
            string apiUrl,
            RetryPolicy? retryPolicy = null)
        {
            retryPolicy ??= DefaultSafeOperationRetryPolicy;

            if (retryPolicy.MaxRetries == 0)
            {
                try
                {
                    var (response, result) = await operation();
                    if (!response.IsSuccessStatusCode)
                    {
                        await ProcessNotSuccessfulResponse(response, apiUrl, operationName);
                    }
                    response.Dispose();
                    return result;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Operation '{OperationName}' for API '{ApiUrl}' failed with no retries configured.", operationName, _httpClientBaseAddress + apiUrl);
                    throw;
                }
            }

            Exception? lastException = null;

            for (int attempt = 0; attempt <= retryPolicy.MaxRetries; attempt++)
            {
                try
                {
                    var (response, result) = await operation();

                    if (!response.IsSuccessStatusCode)
                    {
                        if (attempt < retryPolicy.MaxRetries && retryPolicy.IsRetryable(response.StatusCode))
                        {
                            int delayMs = retryPolicy.CalculateDelayMs(attempt);
                            _logger.LogWarning(
                                "Retry attempt {RetryAttempt} of {MaxRetries} for operation '{OperationName}' after {DelayMs}ms. Status: {StatusCode}",
                                attempt + 1,
                                retryPolicy.MaxRetries,
                                operationName,
                                delayMs,
                                response.StatusCode);

                            response.Dispose();
                            await Task.Delay(delayMs);
                            continue;
                        }

                        await ProcessNotSuccessfulResponse(response, apiUrl, operationName);
                        response.Dispose();
                    }

                    response.Dispose();
                    return result;
                }
                catch (Exception ex) when (attempt < retryPolicy.MaxRetries && ShouldRetry(ex, retryPolicy))
                {
                    lastException = ex;
                    int delayMs = retryPolicy.CalculateDelayMs(attempt);

                    _logger.LogWarning(
                        "Retry attempt {RetryAttempt} of {MaxRetries} for operation '{OperationName}' after {DelayMs}ms. Exception: {ExceptionType}: {Message}",
                        attempt + 1,
                        retryPolicy.MaxRetries,
                        operationName,
                        delayMs,
                        ex.GetType().Name,
                        ex.Message);

                    await Task.Delay(delayMs);
                }
                catch (Exception ex) when (!ShouldRetry(ex, retryPolicy) || attempt >= retryPolicy.MaxRetries)
                {
                    _logger.LogError(ex, "Operation '{OperationName}' for API '{ApiUrl}' failed and will not be retried. Exception: {ExceptionType}: {Message}",
                                         operationName,
                                         _httpClientBaseAddress + apiUrl,
                                         ex.GetType().Name,
                                         ex.Message);
                                     throw;
                                 }
                             }

                             if (lastException != null)
                             {
                                 _logger.LogError(lastException, "Operation '{OperationName}' for API '{ApiUrl}' failed after {Attempts} attempts.", operationName, _httpClientBaseAddress + apiUrl, retryPolicy.MaxRetries + 1);
                                 throw lastException;
                             }

                             throw new InvalidOperationException($"Retry logic failed for operation '{operationName}'");
                         }

        /// <summary>
        /// Executes an operation with retry support, returning both the HttpResponseMessage and result.
        /// Unlike ExecuteWithRetryAndResponseAsync, this returns the response without disposing it.
        /// Used for Try* methods that need to return raw response to caller.
        /// </summary>
        protected async Task<(HttpResponseMessage Response, TResult Result)> ExecuteWithRetryAndResponseForTryAsync<TResult>(
            Func<Task<(HttpResponseMessage Response, TResult Result)>> operation,
            string operationName,
            string apiUrl,
            RetryPolicy? retryPolicy = null)
        {
            retryPolicy ??= DefaultSafeOperationRetryPolicy;

            if (retryPolicy.MaxRetries == 0)
            {
                try
                {
                    var (response, result) = await operation();
                    return (response, result);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Operation '{OperationName}' for API '{ApiUrl}' failed with no retries configured.", operationName, _httpClientBaseAddress + apiUrl);

                    var errorResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent($"Request failed: {ex.Message}", Encoding.UTF8, "application/json")
                    };
                    return (errorResponse, default(TResult)!);
                }
            }

            Exception? lastException = null;
            (HttpResponseMessage Response, TResult Result) lastResult = (null!, default(TResult)!);
            bool hasResult = false;

            for (int attempt = 0; attempt <= retryPolicy.MaxRetries; attempt++)
            {
                try
                {
                    var (response, result) = await operation();
                    hasResult = true;
                    lastResult = (response, result);

                    if (!response.IsSuccessStatusCode)
                    {
                        if (attempt < retryPolicy.MaxRetries && retryPolicy.IsRetryable(response.StatusCode))
                        {
                            int delayMs = retryPolicy.CalculateDelayMs(attempt);
                            _logger.LogWarning(
                                "Retry attempt {RetryAttempt} of {MaxRetries} for operation '{OperationName}' after {DelayMs}ms. Status: {StatusCode}",
                                attempt + 1,
                                retryPolicy.MaxRetries,
                                operationName,
                                delayMs,
                                response.StatusCode);

                            await Task.Delay(delayMs);
                            continue;
                        }

                        return (response, result);
                    }

                    return (response, result);
                }
                catch (Exception ex) when (attempt < retryPolicy.MaxRetries && ShouldRetry(ex, retryPolicy))
                {
                    lastException = ex;
                    int delayMs = retryPolicy.CalculateDelayMs(attempt);

                    _logger.LogWarning(
                        "Retry attempt {RetryAttempt} of {MaxRetries} for operation '{OperationName}' after {DelayMs}ms. Exception: {ExceptionType}: {Message}",
                        attempt + 1,
                        retryPolicy.MaxRetries,
                        operationName,
                        delayMs,
                        ex.GetType().Name,
                        ex.Message);

                    await Task.Delay(delayMs);
                }
                catch (Exception ex) when (!ShouldRetry(ex, retryPolicy) || attempt >= retryPolicy.MaxRetries)
                {
                    _logger.LogError(ex, "Operation '{OperationName}' for API '{ApiUrl}' failed and will not be retried. Exception: {ExceptionType}: {Message}",
                        operationName,
                        _httpClientBaseAddress + apiUrl,
                        ex.GetType().Name,
                        ex.Message);

                    var errorResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                    {
                        Content = new StringContent($"Request failed: {ex.Message}", Encoding.UTF8, "application/json")
                    };
                    return (errorResponse, default(TResult)!);
                }
            }

            if (hasResult)
            {
                return lastResult;
            }

            if (lastException != null)
            {
                _logger.LogError(lastException, "Operation '{OperationName}' for API '{ApiUrl}' failed after {Attempts} attempts.", operationName, _httpClientBaseAddress + apiUrl, retryPolicy.MaxRetries + 1);
                var errorResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
                {
                    Content = new StringContent($"Request failed after retries: {lastException.Message}", Encoding.UTF8, "application/json")
                };
                return (errorResponse, default(TResult)!);
            }

            var failResponse = new HttpResponseMessage(System.Net.HttpStatusCode.InternalServerError)
            {
                Content = new StringContent("Request failed after retries", Encoding.UTF8, "application/json")
            };
            return (failResponse, default(TResult)!);
        }

        /// <summary>
        /// Determines whether an exception should trigger a retry.
        /// </summary>
        private bool ShouldRetry(Exception ex, RetryPolicy retryPolicy)
        {
            if (ex is HttpRequestException httpEx)
            {
                return retryPolicy.IsRetryable(ex) || 
                       (retryPolicy.RetryOnTimeout && httpEx.InnerException is TimeoutException);
            }

            return retryPolicy.IsRetryable(ex);
        }

        #endregion

        #region Helpers

        /// <summary>
        /// Safely deserializes response content, returning null on failure instead of throwing.
        /// </summary>
        private async Task<TOut?> SafeDeserializeAsync<TOut>(HttpContent content, string? contentAsString = null)
        {
            try
            {
                if (typeof(TOut) == typeof(String))
                {
                    contentAsString ??= await content.ReadAsStringAsync();
                    if (contentAsString.StartsWith("\"") && contentAsString.EndsWith("\"") && contentAsString.Length > 1)
                    {
                        contentAsString = contentAsString.Substring(1, contentAsString.Length - 2);
                    }
                    return (TOut)(object)contentAsString;
                }
                else
                {
                    using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        var result = await JsonSerializer.DeserializeAsync<TOut>(stream, new JsonSerializerOptions() 
                        { 
                            MaxDepth = 5, 
                            IncludeFields = true, 
                            PropertyNameCaseInsensitive = true 
                        });
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to deserialize response content to type '{TargetType}'. Content preview: {ContentPreview}",
                    typeof(TOut).Name,
                    TruncateContent(contentAsString ?? "N/A", maxLength: 500));
                return default;
            }
        }

        /// <summary>
        /// Truncates content string for safe logging, adding ellipsis if truncated.
        /// </summary>
        private string TruncateContent(string content, int maxLength = 500)
        {
            if (string.IsNullOrEmpty(content))
                return content;

            if (content.Length <= maxLength)
                return content;

            return content.Substring(0, maxLength) + "... [truncated]";
        }

        #endregion
    }
}
