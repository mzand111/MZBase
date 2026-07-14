using System.Net;

namespace MZBase.Microservices.HttpServices
{
    /// <summary>
    /// Configures retry behavior for HTTP service calls with exponential backoff and jitter.
    /// Designed for high-load scenarios with resilient backoff strategies.
    /// </summary>
    public class RetryPolicy
    {
        /// <summary>
        /// Maximum number of retry attempts. Default: 3
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Initial delay in milliseconds before first retry. Default: 100ms
        /// Grows exponentially with each retry.
        /// </summary>
        public int InitialDelayMs { get; set; } = 100;

        /// <summary>
        /// Multiplier for exponential backoff. Default: 2.0
        /// Each retry delay = previous delay * this multiplier
        /// </summary>
        public double BackoffMultiplier { get; set; } = 2.0;

        /// <summary>
        /// Maximum delay in milliseconds between retries. Default: 30000ms (30 seconds)
        /// Prevents excessive delays in backoff strategy.
        /// </summary>
        public int MaxDelayMs { get; set; } = 30000;

        /// <summary>
        /// Whether to add random jitter to delays. Default: true
        /// Jitter prevents thundering herd problem in distributed scenarios.
        /// </summary>
        public bool UseJitter { get; set; } = true;

        /// <summary>
        /// Maximum jitter percentage (0-1). Default: 0.1 (10%)
        /// Actual jitter = delay * random(0, MaxJitterFraction)
        /// </summary>
        public double MaxJitterFraction { get; set; } = 0.1;

        /// <summary>
        /// HTTP status codes that trigger a retry. Default: transient errors and timeouts
        /// Common transient errors: 408, 429, 500, 502, 503, 504
        /// </summary>
        public HashSet<HttpStatusCode> RetryableStatusCodes { get; set; } = new()
        {
            HttpStatusCode.RequestTimeout,           // 408
            HttpStatusCode.TooManyRequests,         // 429
            HttpStatusCode.InternalServerError,     // 500
            HttpStatusCode.BadGateway,              // 502
            HttpStatusCode.ServiceUnavailable,      // 503
            HttpStatusCode.GatewayTimeout           // 504
        };

        /// <summary>
        /// Exception types that trigger a retry. Default: transient network errors
        /// </summary>
        public HashSet<Type> RetryableExceptionTypes { get; set; } = new()
        {
            typeof(HttpRequestException),
            typeof(TimeoutException),
            typeof(IOException),
            typeof(OperationCanceledException)
        };

        /// <summary>
        /// Whether to retry on timeout exceptions. Default: true
        /// </summary>
        public bool RetryOnTimeout { get; set; } = true;

        /// <summary>
        /// Creates a new RetryPolicy with default settings optimized for high-load scenarios.
        /// </summary>
        public RetryPolicy()
        {
        }

        /// <summary>
        /// Creates a new RetryPolicy with custom max retries.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts</param>
        public RetryPolicy(int maxRetries) : this()
        {
            MaxRetries = maxRetries;
        }

        /// <summary>
        /// Creates a new RetryPolicy with custom settings.
        /// </summary>
        /// <param name="maxRetries">Maximum number of retry attempts</param>
        /// <param name="initialDelayMs">Initial delay in milliseconds</param>
        /// <param name="backoffMultiplier">Exponential backoff multiplier</param>
        public RetryPolicy(int maxRetries, int initialDelayMs, double backoffMultiplier) : this()
        {
            MaxRetries = maxRetries;
            InitialDelayMs = initialDelayMs;
            BackoffMultiplier = backoffMultiplier;
        }

        /// <summary>
        /// Calculates the delay for a given retry attempt number using exponential backoff with jitter.
        /// </summary>
        /// <param name="retryAttempt">Zero-based retry attempt number</param>
        /// <returns>Delay in milliseconds</returns>
        public int CalculateDelayMs(int retryAttempt)
        {
            if (retryAttempt < 0)
                throw new ArgumentException("Retry attempt must be >= 0", nameof(retryAttempt));

            // Calculate exponential backoff
            double delayMs = InitialDelayMs * Math.Pow(BackoffMultiplier, retryAttempt);

            // Cap at maximum delay
            delayMs = Math.Min(delayMs, MaxDelayMs);

            // Add jitter if enabled
            if (UseJitter)
            {
                var random = new Random();
                double jitter = delayMs * random.NextDouble() * MaxJitterFraction;
                delayMs += jitter;
            }

            return (int)Math.Ceiling(delayMs);
        }

        /// <summary>
        /// Determines if the given HTTP status code should trigger a retry.
        /// </summary>
        public bool IsRetryable(HttpStatusCode statusCode) => RetryableStatusCodes.Contains(statusCode);

        /// <summary>
        /// Determines if the given exception type should trigger a retry.
        /// </summary>
        public bool IsRetryable(Exception ex) => RetryableExceptionTypes.Any(type => type.IsAssignableFrom(ex.GetType()));

        /// <summary>
        /// Creates a default retry policy optimized for non-idempotent operations (POSTs).
        /// Uses fewer retries than GET operations since retries may cause duplicate data.
        /// </summary>
        public static RetryPolicy CreateDefaultForPostOperations()
        {
            return new RetryPolicy(maxRetries: 1, initialDelayMs: 200, backoffMultiplier: 2.0);
        }

        /// <summary>
        /// Creates a default retry policy optimized for safe, idempotent operations (GETs, PUTs, DELETEs).
        /// </summary>
        public static RetryPolicy CreateDefaultForSafeOperations()
        {
            return new RetryPolicy(maxRetries: 3, initialDelayMs: 100, backoffMultiplier: 2.0);
        }

        /// <summary>
        /// Creates a retry policy with no retries (disable retry logic).
        /// </summary>
        public static RetryPolicy NoRetry()
        {
            return new RetryPolicy { MaxRetries = 0 };
        }

        /// <summary>
        /// Creates an aggressive retry policy for critical operations.
        /// Use with caution in high-load scenarios.
        /// </summary>
        public static RetryPolicy CreateAggressivePolicy()
        {
            return new RetryPolicy(maxRetries: 5, initialDelayMs: 50, backoffMultiplier: 1.5);
        }
    }
}
