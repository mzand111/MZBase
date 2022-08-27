using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MZBase.Microservices.HttpServices
{
    public abstract class ServiceMediatorBase<T> : IMicroServiceProxy
    {
        protected readonly HttpClient _httpClient;
        protected readonly string _httpClientBaseAddress;
        protected readonly string _serviceUniqueName;
        protected readonly ILogger<T> _logger;
        public ServiceMediatorBase(HttpClient httpClient, ILogger<T> logger, string serviceBaseAddress, string serviceUniqeName)
        {
            _httpClient = httpClient;
            _httpClientBaseAddress = serviceBaseAddress;
            _serviceUniqueName = serviceUniqeName;
            _logger = logger;

            _httpClient.BaseAddress = new Uri(_httpClientBaseAddress);
        }
        protected async Task GetAsync(string address)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "GetAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);

            using (var httpResponseMessage = await _httpClient.GetAsync(address))
            {

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    await processNotSuccessfullResponse(httpResponseMessage, address, "GetAsync");
                }               

                _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                       , "GetAsync"
                       , _serviceUniqueName
                       , _httpClientBaseAddress + address);               

            }
        }
        protected async ValueTask<TOut> GetAsync<TOut>(string address)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "GetAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + address);
            
            using (var httpResponseMessage = await _httpClient.GetAsync(address))
            {

                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    await processNotSuccessfullResponse(httpResponseMessage, address, "GetAsync");
                }

                if (httpResponseMessage.StatusCode == HttpStatusCode.NoContent)
                {
                    //var t = typeof(TOut);
                    //if (Nullable.GetUnderlyingType(t) == null)
                    //{
                    //    
                    //    // T is a Nullable<>
                    //}
                    return default;
                }

                _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                       , "GetAsync"
                       , _serviceUniqueName
                       , _httpClientBaseAddress + address);

                using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    try
                    {
                        var apiResponseDto = await JsonSerializer.DeserializeAsync<TOut>(stream, new JsonSerializerOptions() { MaxDepth = 5, IncludeFields = true, PropertyNameCaseInsensitive = true });
                        return apiResponseDto;
                    }
                    catch (Exception ex)
                    {

                        string exMessage = ex.Message; ;
                        if (ex.InnerException != null)
                        {
                            exMessage += ",Inner message" + ex.InnerException.Message;
                        }
                        _logger.LogError(ex, "Failed to do deserialize output after successfull get method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                            + exMessage
                         , "GetAsync"
                         , _serviceUniqueName
                         , _httpClientBaseAddress + address);

                        throw new Exception("Error reading content and deserializing after successfully called remote procedure:" + exMessage);
                    }
                }

            }
        }
        //protected async ValueTask<TOut> GetAsync<TIn, TOut>(TIn item, string apiUrl)
        //{
        //    _httpClient.BaseAddress = new Uri(_httpClientBaseAddress);
        //    var jsonPayload = JsonSerializer.Serialize(item);
        //    using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
        //    {
        //        content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //        using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Get, content))
        //        using (var httpResponseMessage = await _httpClient.SendAsync(request).ConfigureAwait(false))
        //        {

        //            if (httpResponseMessage.IsSuccessStatusCode)
        //            {
        //                using var contentStream =
        //                    await httpResponseMessage.Content.ReadAsStreamAsync();

        //                var outp = await JsonSerializer.DeserializeAsync
        //                     <TOut>(contentStream);
        //                return outp;

        //            }
        //            if (httpResponseMessage.StatusCode == HttpStatusCode.Unauthorized)
        //            {
        //                throw new UnauthorizedAccessException("Service call not authorized");
        //            }
        //            if (httpResponseMessage.StatusCode == HttpStatusCode.MethodNotAllowed)
        //            {
        //                throw new Exception("Method not allowed");
        //            }
        //            if (httpResponseMessage.StatusCode == HttpStatusCode.NotFound)
        //            {
        //                throw new Exception("Service address not found");
        //            }

        //            string s = await httpResponseMessage.Content.ReadAsStringAsync();
        //            throw new Exception(s);
        //        }
        //    }
        //}

        /// <summary>
        /// Post Async Method
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="item"></param>
        /// <param name="apiUrl"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected async Task PostAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'", "PostAsync", _serviceUniqueName, _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
            using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, content, headers))
            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await processNotSuccessfullResponse(response, apiUrl, "PostAsync");
                }
                _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                       , "PostAsync"
                       , _serviceUniqueName
                       , _httpClientBaseAddress + apiUrl);
            }
        }

        /// <summary>
        /// Post Async Method
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="item"></param>
        /// <param name="apiUrl"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected async Task<TOut> PostAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'", "PostAsync", _serviceUniqueName, _httpClientBaseAddress + apiUrl);

            var jsonPayload = JsonSerializer.Serialize(item);
            using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
            using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, content, headers))
            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await processNotSuccessfullResponse(response, apiUrl, "PostAsync");
                }
                _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                       , "PostAsync"
                       , _serviceUniqueName
                       , _httpClientBaseAddress + apiUrl);


                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    try
                    {
                        var apiResponseDto = await JsonSerializer.DeserializeAsync<TOut>(stream, new JsonSerializerOptions() { MaxDepth = 5, IncludeFields = true, PropertyNameCaseInsensitive = true });
                        return apiResponseDto;
                    }
                    catch (Exception ex)
                    {

                        string exMessage = ex.Message; ;
                        if (ex.InnerException != null)
                        {
                            exMessage += ",Inner message" + ex.InnerException.Message;
                        }
                        _logger.LogError(ex, "Failed to do deserialize output after successfull post method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                            + exMessage
                         , "PostAsync"
                         , _serviceUniqueName
                         , _httpClientBaseAddress + apiUrl);

                        throw new Exception("Failed to deserialize the response:" + exMessage);
                    }
                }
            }
        }

        /// <summary>
        /// Put Async Method
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <typeparam name="TOut"></typeparam>
        /// <param name="item"></param>
        /// <param name="apiUrl"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected async Task<TOut> PutAsync<TIn, TOut>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PutAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);
            var jsonPayload = JsonSerializer.Serialize(item);
            using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
            using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Put, content, headers))
            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await processNotSuccessfullResponse(response, apiUrl, "PutAsync");
                }
                _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                      , "PutAsync"
                      , _serviceUniqueName
                      , _httpClientBaseAddress + apiUrl);
                using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                {
                    if (stream != null && stream.Length > 0)
                    {
                        try
                        {
                            var apiResponseDto = await JsonSerializer.DeserializeAsync<TOut>(stream);
                            return apiResponseDto;
                        }
                        catch (Exception ex)
                        {

                            string exMessage = ex.Message; ;
                            if (ex.InnerException != null)
                            {
                                exMessage += ",Inner message" + ex.InnerException.Message;
                            }
                            _logger.LogError(ex, "Failed to do deserialize output after successfull post method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                                + exMessage
                             , "PutAsync"
                             , _serviceUniqueName
                             , _httpClientBaseAddress + apiUrl);

                            throw new Exception("Failed to deserialize the response:" + exMessage);
                        }
                    }
                    return default;
                }
            }
        }

        /// <summary>
        /// Put Async Method
        /// </summary>
        /// <typeparam name="TIn"></typeparam>
        /// <param name="item"></param>
        /// <param name="apiUrl"></param>
        /// <param name="headers"></param>
        /// <returns></returns>
        protected async Task PutAsync<TIn>(TIn item, string apiUrl, Dictionary<string, string>? headers = null) where TIn : class
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PutAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);
            var jsonPayload = JsonSerializer.Serialize(item);
            using (var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json"))
            using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Put, content, headers))
            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await processNotSuccessfullResponse(response, apiUrl, "PutAsync");
                }
                _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                      , "PutAsync"
                      , _serviceUniqueName
                      , _httpClientBaseAddress + apiUrl);
               
            }
        }

        public async Task PostFormFileAsync(IFormFile content, string apiUrl)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PostFormFileAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);
            using (var target = new MemoryStream())
            {
                content.CopyTo(target);
                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new ByteArrayContent(target.ToArray()), "file", Path.GetFileName(content.FileName));

                using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, multipartContent))
                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        await processNotSuccessfullResponse(response, apiUrl, "PutAsync");
                    }

                    _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                     , "PostFormFileAsync"
                     , _serviceUniqueName
                     , _httpClientBaseAddress + apiUrl);
                }
            }
        }

        public async Task<TOut> PostFormFileAsync<TOut>(IFormFile content, string apiUrl)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , "PostFormFileAsync"
                , _serviceUniqueName
                , _httpClientBaseAddress + apiUrl);
            using (var target = new MemoryStream())
            {
                content.CopyTo(target);
                var multipartContent = new MultipartFormDataContent();
                multipartContent.Add(new ByteArrayContent(target.ToArray()), "file", Path.GetFileName(content.FileName));

                using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Post, multipartContent))
                using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
                {
                    if (!response.IsSuccessStatusCode)
                    {
                        await processNotSuccessfullResponse(response, apiUrl, "PutAsync");
                    }

                    _logger.LogInformation("Successfully called remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                     , "PostFormFileAsync"
                     , _serviceUniqueName
                     , _httpClientBaseAddress + apiUrl);

                    using (var stream = await response.Content.ReadAsStreamAsync().ConfigureAwait(false))
                    {
                        if (stream != null && stream.Length > 0)
                        {
                            try
                            {
                                var apiResponseDto = await JsonSerializer.DeserializeAsync<TOut>(stream);
                                return apiResponseDto;
                            }
                            catch (Exception ex)
                            {

                                string exMessage = ex.Message; ;
                                if (ex.InnerException != null)
                                {
                                    exMessage += ",Inner message" + ex.InnerException.Message;
                                }
                                _logger.LogError(ex, "Failed to do deserialize output after successfull post method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' exception:"
                                    + exMessage
                                 , "PostFormFileAsync"
                                 , _serviceUniqueName
                                 , _httpClientBaseAddress + apiUrl);

                                throw new Exception("Failed to deserialize the response:" + exMessage);
                            }
                        }
                        return default;
                    }
                }
            }
        }

        /// <summary>
        /// Delete route with headers
        /// </summary>
        /// <param name="apiUrl">resource url to delete</param>
        /// <returns></returns>
        protected async Task DeleteAsync(string apiUrl, Dictionary<string, string>? headers = null)
        {
            _logger.LogInformation("Called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
               , "DeleteAsync"
               , _serviceUniqueName
               , _httpClientBaseAddress + apiUrl);
            using (var request = CreateHttpRequest(_httpClientBaseAddress + apiUrl, HttpMethod.Delete, null, headers))
            using (var response = await _httpClient.SendAsync(request).ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    await processNotSuccessfullResponse(response, apiUrl, "DeleteAsync");
                }
                else
                {
                    _logger.LogInformation("Seccessfully called method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                              , "DeleteAsync"
                              , _serviceUniqueName
                              , _httpClientBaseAddress + apiUrl);
                }
            }
        }

        protected HttpRequestMessage CreateHttpRequest(string apiUrl, HttpMethod httpMethod, HttpContent content, Dictionary<string, string>? headers = null, string acceptType = "application/json")
        {
            var request = new HttpRequestMessage();
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue(acceptType));
            //request.Headers.Add(_transactionContext.TransactionIdKey, _transactionContext.TransactionId.ToString());
            request.RequestUri = new Uri(apiUrl);
            request.Method = httpMethod;
            request.Content = content;

            if (headers != null)
            {
                if (headers.ContainsKey("revision"))
                {
                    request.Headers.Add("If-Match", "\"" + headers["revision"] + "\"");
                }
            }
            //request.Headers.Add("Authorization", _transactionContext.AuthToken);
            return request;
        }

        protected async Task processNotSuccessfullResponse(HttpResponseMessage? response, string apiUrl, string methodName)
        {
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                _logger.LogError("Authorization error calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                , methodName
               , _serviceUniqueName
               , _httpClientBaseAddress + apiUrl);

                throw new UnauthorizedAccessException("Service call not authorized: " + response.ReasonPhrase);
            }
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogError("Not found error calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}'"
                    , methodName
                    , _serviceUniqueName
                    , _httpClientBaseAddress + apiUrl);
                throw new Exception("Service address not found: " + response.ReasonPhrase);
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
                    , "PutAsync"
                    , _serviceUniqueName
                    , _httpClientBaseAddress + apiUrl);

                throw new Exception("Failed to do post method:" + response.ReasonPhrase);
            }
            else
            {
                responsContent = responsContent.Replace("{", "[");
                responsContent = responsContent.Replace("}", "]");
                responsContent = responsContent.Replace("\"", "");

                _logger.LogError("Failed to do post method calling remote procedure: Method '{ServiceMethod}' from service '{Category}' for remote address '{RemoteAddress}' ReasonPhrase:" + response.ReasonPhrase
                    + ",responsContent:" + responsContent
                  , "PutAsync"
                  , _serviceUniqueName
                  , _httpClientBaseAddress + apiUrl);

                throw new Exception("Failed calling remote procedure:" + response.ReasonPhrase + "," + responsContent);
            }
        }
    }
}
