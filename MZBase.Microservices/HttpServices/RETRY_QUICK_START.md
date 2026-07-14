# Quick Start: Using Retry Policies in ServiceMediatorBase

## No Changes Needed for Existing Code
Your existing code continues to work without any modifications:

```csharp
// All existing calls work unchanged
await GetAsync<User>("users/123");
await PostAsync(newUser, "users");
await DeleteAsync($"users/{id}");
```

## Enable Retries (3 Steps)

### Step 1: Import the RetryPolicy class
```csharp
using MZBase.Microservices.HttpServices;
```

### Step 2: Choose a retry policy

**Option A: Use Default Retry Policies**
```csharp
// For GET/PUT/DELETE (safe, idempotent)
var policy = RetryPolicy.CreateDefaultForSafeOperations();

// For POST (potentially non-idempotent) 
var policy = RetryPolicy.CreateDefaultForPostOperations();

// For critical operations
var policy = RetryPolicy.CreateAggressivePolicy();

// Disable retries
var policy = RetryPolicy.NoRetry();
```

**Option B: Create Custom Policy**
```csharp
var policy = new RetryPolicy
{
	MaxRetries = 5,
	InitialDelayMs = 100,
	BackoffMultiplier = 2.0,
	UseJitter = true,
	MaxDelayMs = 30000
};
```

### Step 3: Pass policy to method
```csharp
// GET with retry
var user = await GetAsync<User>("users/123", policy);

// POST with retry
await PostAsync(newUser, "users", null, policy);

// PUT with retry
await PutAsync(updatedUser, "users/123", null, policy);

// DELETE with retry
await DeleteAsync($"users/{id}", null, policy);

// File operations with retry
await PostFormFileAsync(file, "upload", policy);
var content = await GetFileContentAsync("download/file.pdf", policy);
```

## Real-World Example

```csharp
public class PaymentService : ServiceMediatorBase<PaymentService>
{
	private readonly RetryPolicy _consistentPolicy = new RetryPolicy
	{
		MaxRetries = 3,
		InitialDelayMs = 100,
		BackoffMultiplier = 2.0,
		UseJitter = true
	};

	public PaymentService(HttpClient httpClient, ILogger<PaymentService> logger)
		: base(httpClient, logger, "https://api.payment.com", "PaymentService", requestTimeoutMs: 60000)
	{
	}

	public async Task<PaymentResult> ProcessPaymentAsync(Payment payment)
	{
		// Use custom retry policy for critical payment operations
		return await PostAsync<Payment, PaymentResult>(
			payment, 
			"payments/process", 
			null,
			_consistentPolicy
		);
	}

	public async Task<PaymentDetails> GetPaymentDetailsAsync(string paymentId)
	{
		// Safe operation uses default retry policy
		return await GetAsync<PaymentDetails>($"payments/{paymentId}");
	}
}
```

## Configuration by Scenario

### High-Load Environment
```csharp
var policy = new RetryPolicy
{
	MaxRetries = 2,        // Fewer retries
	InitialDelayMs = 50,   // Shorter delay
	BackoffMultiplier = 1.5,
	UseJitter = true,
	MaxJitterFraction = 0.2 // More jitter spread
};
```

### Unreliable Network
```csharp
var policy = new RetryPolicy
{
	MaxRetries = 5,         // More retries
	InitialDelayMs = 200,   // Longer initial wait
	BackoffMultiplier = 2.5, // Steeper backoff
	UseJitter = true
};
```

### Ultra-Low Latency Requirements
```csharp
var policy = RetryPolicy.NoRetry();
// Or minimal retries
var policy = new RetryPolicy(maxRetries: 1, initialDelayMs: 10, backoffMultiplier: 1.0);
```

## Monitoring Retries

Look for these log messages to monitor retry behavior:

```
Retry attempt 1 of 3 for operation 'GetAsync' after 100ms. Exception: HttpRequestException: Unable to connect
Retry attempt 2 of 3 for operation 'PostAsync' after 200ms. Status: 503
Successfully called remote procedure: Method 'GetAsync' from service 'UserService'...
```

## Common Questions

**Q: Will my existing code break?**
A: No! All existing method signatures are preserved exactly. You only get retries if you explicitly pass a RetryPolicy.

**Q: What's the default behavior if I don't specify a policy?**
A: 
- GET/PUT/DELETE: 3 retries with 100ms initial delay
- POST: 1 retry with 200ms initial delay

**Q: How do I disable retries?**
A: Pass `RetryPolicy.NoRetry()` or just use the original method signature without a retry parameter.

**Q: Can I change retry policies at runtime?**
A: Yes! Each method call accepts its own policy, so you can vary by scenario.

**Q: What happens if all retries fail?**
A: The original exception is thrown after all retries are exhausted.

**Q: Does jitter really matter?**
A: Yes! In distributed systems under load, jitter prevents multiple clients from retrying at exactly the same time (thundering herd).
