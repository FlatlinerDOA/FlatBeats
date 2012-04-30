namespace FlatBeats.DataModel
{
    using System;
    using System.Net;

    public sealed class DisposableWebRequest : WebRequest, IDisposable
    {
        private readonly WebRequest request;

        private volatile bool disposed = false;

        public DisposableWebRequest(Uri url)
        {
            this.request = HttpWebRequest.CreateHttp(url);
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
                return this.request.EndGetResponse(asyncResult);
            }
            else
            {
                return null;
            }
        }

        public override void Abort()
        {
            this.request.Abort();
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
