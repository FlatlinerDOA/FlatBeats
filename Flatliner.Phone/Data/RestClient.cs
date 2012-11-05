using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace Flatliner.Phone.Data
{
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Security.Cryptography;
    using System.Text;
    using System.Xml.Linq;
    using Microsoft.Phone.Reactive;


    /// <summary>
    /// Provides simplified asynchronous access to REST style HTTP endpoints.
    /// Uses automatic type inference to determine request and response content types, 
    /// To submit plain text use a string.
    /// To submit file data use a Stream or an enumerable of bytes (such as a byte array).
    /// To submit form data use an implementation of <see cref="IDictionary{TKey,TValue}"/>.
    /// To submit Xml data use a DataContract object or an <see cref="XElement"/>.
    /// </summary>
    public sealed class RestClient
    {
        #region Constants and Fields

        /// <summary>
        /// Authentication type that uses a secret key to hash the url which is added to as the Authorization header.
        /// </summary>
        public const string TokenAuthentication = "AuthToken";

        #endregion

        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the RestClient class.
        /// </summary>
        public RestClient()
        {
            this.RequestTimeout = TimeSpan.FromSeconds(60);
            this.Encoding = Encoding.UTF8;
        }

        #endregion


        /// <summary>
        /// Gets or sets the authentication credentials to use when 
        /// communicating with the REST service, it is recommended 
        /// Authentication credentials are only suppleid to HTTPS urls.
        /// </summary>
        public ICredentials Credentials
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the text encoding to use when sending requests 
        /// This defaults to UTF8 encoding.
        /// </summary>
        public Encoding Encoding
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the total time allotted for a standard GET, PUT, DELETE or POST, request.
        /// (Defaults to 100 seconds)
        /// </summary>
        public TimeSpan RequestTimeout
        {
            get;
            set;
        }

        /// <summary>
        /// Deletes a resource at the specified url.
        /// </summary>
        /// <param name="url">The Http Url to the resource to be deleted.</param>
        public IObservable<Unit> DeleteAsync(Uri url)
        {
            return this.WriteAsync(url, HttpMethods.Delete);
        }

        /// <summary>
        /// Deletes a resource at the specified url.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body to send.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="url">The Http Url to the resource to be deleted.</param>
        /// <param name="requestBody">The request body to send along with the delete.</param>
        public IObservable<Unit> DeleteAsync<TRequest>(Uri url, TRequest requestBody)
            where TRequest : class
        {
            return this.WriteAsync(url, HttpMethods.Delete, requestBody);
        }

        /// <summary>
        /// Gets a resource at the specified url.
        /// </summary>
        /// <typeparam name="TResponse">The type of response data to expect from the server.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="url">The Http Url to the resource to be retrieved.</param>
        public IObservable<TResponse> GetAsync<TResponse>(Uri url)
            where TResponse : class
        {
            return this.ReadAsync<TResponse>(url, HttpMethods.Get);
        }

        /// <summary>
        /// Gets the header of a resource at the specified url.
        /// </summary>
        /// <typeparam name="TResponse">The type of response data to expect from the server.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="url">The Http Url to the resource to retreive the header details of.</param>
        /// <returns>Returns the result from the web server.</returns>
        public TResponse Head<TResponse>(Uri url)
            where TResponse : class
        {
            return this.ReadAsync<TResponse>(url, HttpMethods.Head).First();
        }

        /// <summary>
        /// Gets the header of a resource at the specified url.
        /// </summary>
        /// <typeparam name="TResponse">The type of response data to expect from the server.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="url">The Http Url to the resource to retreive the header details of.</param>
        public IObservable<TResponse> HeadAsync<TResponse>(Uri url)
            where TResponse : class
        {
            return this.ReadAsync<TResponse>(url, HttpMethods.Head);
        }

        /// <summary>
        /// Posts a request to the specified Http url
        /// </summary>
        /// <param name="url">The Url to post the request to.</param>
        public IObservable<Unit> PostAsync(Uri url)
        {
            return this.WriteAsync(url, HttpMethods.Post);
        }

        /// <summary>
        /// Posts data to the specified Http url
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body to send.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="url">The Url to post the request to.</param>
        /// <param name="requestBody">The request data to post to the Url.</param>
        public IObservable<Unit> PostAsync<TRequest>(Uri url, TRequest requestBody)
            where TRequest : class
        {
            return this.WriteAsync(url, HttpMethods.Post, requestBody);
        }

        /// <summary>
        /// Posts data to the specified Http Url
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body to send.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <typeparam name="TResponse">The type of response data to expect from the server.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="url">The Url to post the request to.</param>
        /// <param name="requestBody">The request data to post to the Url.</param>
        public IObservable<TResponse> PostAsync<TRequest, TResponse>(Uri url, TRequest requestBody)
            where TRequest : class
            where TResponse : class
        {
            return this.WriteAndReadAsync<TRequest, TResponse>(url, HttpMethods.Post, requestBody);
        }

        /// <summary>
        /// Posts file data to the specified Http Url
        /// </summary>
        /// <param name="url">The url to post the file to.</param>
        /// <param name="data">The file data to post to the service.</param>
        /// <param name="contentType">The Mime content type of the file.</param>
        public IObservable<Unit> PostFileAsync(Uri url, Stream data, string contentType)
        {
            var upload = from webRequest in Observable.Return(this.CreateFileUploadRequest(url, contentType))
                         from requestStream in Observable.FromAsyncPattern<Stream>(webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream)().Do(rs => data.PipeTo(rs))
                         from response in Observable.FromAsyncPattern<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse)().Do(r => r.Close())
                         select new Unit();
            return upload;
        }

        /// <summary>
        /// Puts a resource at the specified Http url.
        /// </summary>
        /// <param name="url">The Url to put the resource.</param>
        public IObservable<Unit> PutAsync(Uri url)
        {
            return this.WriteAsync(url, HttpMethods.Put);
        }

        /// <summary>
        /// Puts a resource at the specified Http url.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body to send.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="url">The Url to put the resource.</param>
        /// <param name="requestBody">The request data to send to the Url.</param>
        public IObservable<Unit> PutAsync<TRequest>(Uri url, TRequest requestBody)
            where TRequest : class
        {
            return this.WriteAsync(url, HttpMethods.Put, requestBody);
        }

        /// <summary>
        /// Reads the response of a web request.
        /// </summary>
        /// <param name="url">The Http Url to send the request to.</param>
        /// <param name="method">The Http method (verb) to perform.</param>
        internal IObservable<TResponse> ReadAsync<TResponse>(
            Uri url,
            string method)
            where TResponse : class
        {
            return this.InvokeAsync(url, method, null).Select(response => GetResponseBodyFromStream<TResponse>(response.Item1, response.Item2));
        }

        /// <summary>
        /// Invokes a web request to the server with the specified parameters.
        /// </summary>
        /// <param name="url">The Http Url to send the request to.</param>
        /// <param name="method">The Http method (verb) to perform.</param>
        /// <param name="requestBody">The request body content to send.</param>
        internal IObservable<TResponse> WriteAndReadAsync<TRequest, TResponse>(
            Uri url,
            string method,
            TRequest requestBody)
            where TRequest : class
            where TResponse : class
        {
            var requestBodyContent = this.GetRequestBodyContent(requestBody);
            return this.InvokeAsync(url, method, requestBodyContent).Select(response => GetResponseBodyFromStream<TResponse>(response.Item1, response.Item2));
        }

        /// <summary>
        /// Writes a web request to the server with no message body.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="method"></param>
        /// <returns></returns>
        internal IObservable<Unit> WriteAsync(Uri url, string method)
        {
            return this.InvokeAsync(url, method, null).Do(r => r.Item1.Close()).Select(_ => new Unit());
        }

        /// <summary>
        /// Writes a web request to the server with the specified message body.
        /// </summary>
        /// <typeparam name="TRequest">The type of the request body to be serialized.</typeparam>
        /// <param name="url">The url to write the web request to.</param>
        /// <param name="method">The http method of the request.</param>
        /// <param name="requestBody">The object to be serialized and written to the request stream.</param>
        /// <returns>Returns an observable sequence of a Unit inidicating the request was successful.</returns>
        internal IObservable<Unit> WriteAsync<TRequest>(Uri url, string method, TRequest requestBody)
            where TRequest : class
        {
            var requestBodyContent = this.GetRequestBodyContent(requestBody);
            return this.InvokeAsync(url, method, requestBodyContent).Do(r => r.Item1.Close()).Select(_ => new Unit());
        }

        /// <summary>
        /// Gets the data from a Http response, decompressing if necessary,
        /// and converting it to the appropriate type.
        /// </summary>
        /// <typeparam name="TResponse">The type of response data to expect from the server.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="response">The HttpWebResponse returned from the server.</param>
        /// <param name="responseStream">The response stream to read from.</param>
        /// <returns>Returns a value of type <typeparamref name="TResponse"/>.</returns>
        private static TResponse GetResponseBodyFromStream<TResponse>(HttpWebResponse response, Stream responseStream)
            where TResponse : class
        {
            try
            {
                if (typeof(Stream).IsAssignableFrom(typeof(TResponse)))
                {
                    return responseStream as TResponse;
                }

                if (typeof(TResponse) == typeof(string))
                {
                    return new StreamReader(responseStream).ReadToEnd() as TResponse;
                }

                if (typeof(TResponse) == typeof(XElement))
                {
                    return XElement.Load(responseStream) as TResponse;
                }

                if (typeof(TResponse) == typeof(IDictionary<string, string>))
                {
                    var responseText = new StreamReader(responseStream).ReadToEnd();
                    var results = (from keyValue in responseText.Split('&')
                                   let parts = keyValue.Split(
                                       new []
                                           {
                                               '='
                                           }, StringSplitOptions.None)
                                   select
                                       new KeyValuePair<string, string>(
                                       parts.FirstOrDefault(), parts.Skip(1).FirstOrDefault())).ToDictionary(
                                           k => k.Key, v => v.Value);
                    return results as TResponse;
                }

                return responseStream.DeserializeAs<TResponse>();
            }
            finally
            {
                response.Close();
            }
        }

        /// <summary>
        /// Adds an AuthToken authentication header to the specified web request.
        /// The authentication is a Base64 encoded SHA1 hash of the url, 
        /// using the network credentials password as the key to encrypt it.
        /// </summary>
        /// <param name="webRequest">The web request to authenticate</param>
        /// <param name="destinationAddress">The url to authenticate access to.</param>
        /// <param name="tokenCredentials">The network credentials username and password to use to encrypt the url.</param>
        private void AddAuthenticationHeader(WebRequest webRequest, Uri destinationAddress, NetworkCredential tokenCredentials)
        {
            var url = destinationAddress.AbsoluteUri;
            var secretBytes = Encoding.UTF8.GetBytes(tokenCredentials.Password);
            var hmac = new HMACSHA1(secretBytes);
            var dataBytes = Encoding.UTF8.GetBytes(url);
            var computedHash = hmac.ComputeHash(dataBytes);
            var computedHashString = Convert.ToBase64String(computedHash);
            webRequest.Headers[HttpRequestHeader.Authorization] = tokenCredentials.UserName + ":" + computedHashString;
        }

        /// <summary>
        /// Creates a file upload web request.
        /// </summary>
        /// <param name="url">The url to post to.</param>
        /// <param name="contentType">The mime/content type of the file to be uploaded.</param>
        /// <returns>Returns a new http post web request instance.</returns>
        private HttpWebRequest CreateFileUploadRequest(Uri url, string contentType)
        {
            var request = WebRequest.Create(url) as HttpWebRequest;
            if (request == null)
            {
                return null;
            }

            request.Method = HttpMethods.Post;
            request.ContentType = contentType;
            return request;
        }

        /// <summary>
        /// Creates a new <see cref="HttpWebRequest"/> filling out all the necessary Http Headers.
        /// </summary>
        /// <param name="url">The url to be perform the web request against</param>
        /// <param name="method">The http verb to use when performing the request.</param>
        /// <param name="requestBodyContent">The combination of the request body content (Item1) and it's content type (Item2).</param>
        /// <returns>Returns an new instance of <see cref="HttpWebRequest"/>.</returns>
        private HttpWebRequest CreateWebRequest(Uri url, string method, Tuple<string, string> requestBodyContent)
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(url);

            // The Timeout property is ignored for asynchronous requests
            ////webRequest.Timeout = (int)this.RequestTimeout.TotalMilliseconds;

            ////webRequest.ReadWriteTimeout = (int)this.RequestTimeout.TotalMilliseconds;
            webRequest.Method = method;
            ////webRequest.MaximumAutomaticRedirections = 4;
            ////webRequest.MaximumResponseHeadersLength = 4; // 4kb.
            ////webRequest.Headers[HttpRequestHeader.AcceptEncoding] = "gzip,deflate";
            ////webRequest.Pipelined = false;
            ////webRequest.Headers[HttpRequestHeader.ContentLength] = requestBodyContent != null ? requestBodyContent.Item1.Length.ToString() : "0";
            if (method != HttpMethods.Get && method != HttpMethods.Head)
            {
                var contentType = requestBodyContent != null ? requestBodyContent.Item2 : MediaTypeNames.Text;
                webRequest.ContentType = contentType + "; charset=" + this.Encoding.WebName;
            }

            if (this.Credentials != null)
            {
                var tokenCredentials = this.Credentials.GetCredential(url, TokenAuthentication);
                if (tokenCredentials != null)
                {
                    this.AddAuthenticationHeader(webRequest, url, tokenCredentials);
                }
                else
                {
                    webRequest.Credentials = this.Credentials;
                }
            }

            return webRequest;
        }

        /// <summary>
        /// Gets the text content and it's Mime content type,
        /// </summary>
        /// <typeparam name="TRequest">The request body content data type.
        /// Supported types are: <see cref="string"/>, <see cref="XElement"/>, <see cref="IDictionary{String, String}"/> or a data contract class.</typeparam>
        /// <param name="requestBody">The request body content to be converted to plain text.</param>
        /// <returns>Returns a Tuple of the content (Item1) and the content type (Item2).</returns>
        private Tuple<string, string> GetRequestBodyContent<TRequest>(TRequest requestBody) where TRequest : class
        {
            if (requestBody == null)
            {
                return null;
            }

            if (typeof(Stream).IsAssignableFrom(typeof(TRequest)))
            {
                throw new InvalidOperationException("Streams are not supported by the RestClient as their content type cannot be determined dynamically.");
            }

            if (typeof(TRequest) == typeof(string))
            {
                return Tuple.Create(requestBody as string, MediaTypeNames.Text);
            }

            if (typeof(TRequest) == typeof(XElement))
            {
                var element = requestBody as XElement;

                if (element != null)
                {
                    return Tuple.Create(element.ToString(SaveOptions.DisableFormatting), MediaTypeNames.Xml);
                }

                return null;
            }

            // A dictionary is converted to form post data.
            if (requestBody is IDictionary<string, string>)
            {
                string postData = ((IDictionary<string, string>)requestBody).Aggregate(
                    string.Empty,
                    (d, kv) =>
                    d.Length != 0
                        ? d + "&" + kv.Key + "=" + Uri.EscapeDataString(kv.Value)
                        : kv.Key + "=" + Uri.EscapeDataString(kv.Value));
                return Tuple.Create(postData, "application/x-www-form-urlencoded; charset=" + this.Encoding.WebName);
            }

            string requestBodyText = requestBody.SerializeToString();
            return Tuple.Create(requestBodyText, MediaTypeNames.Xml);
        }

        /// <summary>
        /// Invokes a web request to the server with the specified parameters.
        /// </summary>
        /// <param name="url">The Http Url to send the request to.</param>
        /// <param name="method">The Http method (verb) to perform.</param>
        /// <param name="requestBodyContent">The request body content to send.</param>
        private IObservable<Tuple<HttpWebResponse, Stream>> InvokeAsync(Uri url, string method, Tuple<string, string> requestBodyContent)
        {
            var webRequest = this.CreateWebRequest(url, method, requestBodyContent);
            if (requestBodyContent != null)
            {
                return this.StartWebRequest(webRequest, requestBodyContent).Timeout(this.RequestTimeout);
            }

            return this.StartWebRequest(webRequest).Timeout(this.RequestTimeout);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webRequest"></param>
        /// <returns></returns>
        private IObservable<Tuple<HttpWebResponse, Stream>> StartWebRequest(HttpWebRequest webRequest)
        {
            return from response in Observable.FromAsyncPattern<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse)()
                   select Tuple.Create((HttpWebResponse)response, response.GetResponseStream());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="webRequest"></param>
        /// <param name="requestBodyContent"></param>
        /// <returns></returns>
        private IObservable<Tuple<HttpWebResponse, Stream>> StartWebRequest(HttpWebRequest webRequest, Tuple<string, string> requestBodyContent)
        {
            return from request in Observable.FromAsyncPattern<Stream>(webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream)().Do(r =>
            {
                var data = this.Encoding.GetBytes(requestBodyContent.Item1);
                r.Write(data, 0, data.Length);
            })
                   from response in Observable.FromAsyncPattern<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse)()
                   select Tuple.Create((HttpWebResponse)response, response.GetResponseStream());
        }
    }

    /*
    public class RestClient
    {
        public IObservable<TResult> Invoke<TRequest, TResult>(Uri address, TRequest requestBody, string method)
        {
            return from webRequest in Observable.Return(WebRequest.CreateHttp(address))
                   from request in Observable.FromAsyncPattern<Stream>(
                           webRequest.BeginGetRequestStream, webRequest.EndGetRequestStream)()
                           .Do(stream => WriteRequestBody(requestBody, stream))
                   from response in Observable.FromAsyncPattern<WebResponse>(webRequest.BeginGetResponse, webRequest.EndGetResponse)()
                   select ReadResponseBody<TResult>(response);
        }

        private void WriteRequestBody<TRequest>(TRequest request, Stream output)
        {
            if (typeof(TRequest) == typeof(Unit))
            {
                return;
            }

            var serializer = new DataContractSerializer(typeof(TRequest));
            serializer.WriteObject(output, request);
            output.Flush();
        }

        private TResponse ReadResponseBody<TResponse>(WebResponse response)
        {
            var serializer = new DataContractSerializer(typeof(TResponse));
            using (var stream = response.GetResponseStream())
            {
                return (TResponse)serializer.ReadObject(stream);
            }
        }
    }*/
}
