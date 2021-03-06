﻿namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Net;

    using SharpGIS;

    public sealed class DisposableWebRequest : WebRequest, IDisposable
    {
        private readonly HttpWebRequest request;

        private volatile bool disposed = false;

        public DisposableWebRequest(Uri url)
        {
            this.request = WebRequest.CreateHttp(url);
            this.request.Headers[HttpRequestHeader.AcceptEncoding] = "gzip"; 
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object state)
        {
            return this.request.BeginGetRequestStream(callback, state);
        }

        public override System.IO.Stream EndGetRequestStream(IAsyncResult asyncResult)
        {
            return this.request.EndGetRequestStream(asyncResult);
        }

        public override string Method
        {
            get
            {
                return this.request.Method;
            }
            set
            {
                this.request.Method = value;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.request.RequestUri;
            }
        }

        public override bool UseDefaultCredentials
        {
            get
            {
                return this.request.UseDefaultCredentials;
            }

            set
            {
                this.request.UseDefaultCredentials = value;
            }
        }

        public override ICredentials Credentials
        {
            get
            {
                return this.request.Credentials;
            }
            set
            {
                this.request.Credentials = value;
            }
        }

        public override IWebRequestCreate CreatorInstance
        {
            get
            {
                return this.request.CreatorInstance;
            }
        }

        public override string ContentType
        {
            get
            {
                return this.request.ContentType;
            }
            set
            {
                this.request.ContentType = value;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return this.request.Headers;
            }
            set
            {
                this.request.Headers = value;
            }
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object state)
        {
            return this.request.BeginGetResponse(callback, state);
        }

        public override WebResponse EndGetResponse(IAsyncResult asyncResult)
        {
            if (!this.disposed)
            {
                var response = this.request.EndGetResponse(asyncResult);
                // Response would be GZipWebResponse would be the case if WebRequestCreator was also used
                if (response != null && 
                    !(response is GZipWebClient.GZipWebResponse) && 
                    response.Headers[HttpRequestHeader.ContentEncoding] == "gzip")
                {
                    return new GZipWebClient.GZipWebResponse(response); //If gzipped response, uncompress
                }

                return response;
            }

            return null;
        }

        public override void Abort()
        {
            try
            {
                this.request.Abort();
            }
            catch (ObjectDisposedException)
            {
                // HACK: Ignore disposed Dispatcher.
            }
        }

        public void Dispose()
        {
            if (!this.disposed)
            {
                this.disposed = true;
                this.Abort();
            }
        }
    }
}
