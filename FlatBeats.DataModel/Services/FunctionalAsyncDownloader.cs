namespace FlatBeats.DataModel.Services
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Net;

    using Flatliner.Phone;

    using Microsoft.Phone.Reactive;
    using Flatliner.Functional;

    public sealed class FunctionalAsyncDownloader
    {
        private readonly FunctionalAsyncStorage storage;

        private readonly object syncRoot = new object();

        /// <summary>
        /// </summary>
        private string userToken;

        public bool IsAuthenticated
        {
            get
            {
                return this.UserToken != null;
            }
        }


        public string UserToken
        {
            get
            {
                lock (this.syncRoot)
                {
                    return this.userToken;
                }
            }

            set
            {
                lock (this.syncRoot)
                {
                    this.userToken = value;
                }
            }
        }

        public FunctionalAsyncDownloader()
            : this(FunctionalAsyncStorage.Instance)
        {
        }

        public FunctionalAsyncDownloader(FunctionalAsyncStorage storage)
        {
            this.storage = storage;
        }

        public Observe<PortableUnit> GetAndSaveFileAsync(Uri url, string fileName, bool overwrite)
        {
            if (!overwrite && this.storage.Exists(fileName))
            {
                return Observe.Return(new PortableUnit());
            }

            return this.GetStreamAsync(url, false).Select(
                        stream =>
                        {
                            using (stream)
                            {
                                this.storage.Save(fileName, stream);
                            }

                            return new PortableUnit();
                        });
        }

        public Observe<T> GetDeserializedCachedAndRefreshedAsync<T>(Uri url, string cacheFile) where T : class
        {
            Observe<T> sequence = Observe.Empty<T>();
            if (this.storage.Exists(cacheFile))
            {
                sequence = this.storage.LoadJsonAsync<T>(cacheFile);
            }

            return sequence.Concat(
                from cache in this.GetStreamAsync(url, false).Select(Json<T>.Instance.DeserializeFromStream)
                from _ in this.storage.SaveJsonAsync(cacheFile, cache)
                select cache);
        }

        public Observe<T> GetDeserializedCachedAsync<T>(Uri url, string cacheFile) where T : class
        {
            if (this.storage.Exists(cacheFile))
            {
                return this.storage.LoadJsonAsync<T>(cacheFile);
            }

            return from cache in this.GetStreamAsync(url, false).Select(Json<T>.Instance.DeserializeFromStream)
                   from _ in this.storage.SaveJsonAsync<T>(cacheFile, cache)
                   select cache;
        }

        public Observe<Stream> GetStreamAsync(Uri url, bool disableCache)
        {
            return this.WebRequestAsync(url, disableCache).Select(
                r =>
                {
                    Stream c = new MemoryStream();
                    using (var s = r.GetResponseStream())
                    {
                        s.CopyTo(c);
                    }

                    c.Position = 0;
                    return c;
                });
        }

        public Observe<T> GetDeserializedAsync<T>(Uri url) where T : class
        {
            throw new NotImplementedException();
        }

        public Observe<TResponse> PostAndGetDeserializedAsync<TRequest, TResponse>(Uri url, TRequest postData) where TRequest : class where TResponse : class
        {
            throw new NotImplementedException();
        }

        public Observe<string> PostAndGetStringAsync(Uri url, string postData)
        {
            throw new NotImplementedException();
        }

        public Observe<TResponse> PostStringAndGetDeserializedAsync<TResponse>(Uri url, string postData) where TResponse : class
        {
            throw new NotImplementedException();
        }


        private Observe<WebResponse> WebRequestAsync(Uri address, bool noCache)
        {
            return Observe.Using(
                () => this.CreateRequest(address, noCache),
                r =>
                {
                    Debug.WriteLine("GET " + address.OriginalString);
                    return Observe.FromAsyncPattern(r.BeginGetResponse, r.EndGetResponse)();
                });
        }

        /// <summary>
        /// </summary>
        /// <returns>
        /// </returns>
        private DisposableWebRequest CreateRequest(Uri address, bool noCache)
        {
            var request = new DisposableWebRequest(address);
            request.Headers["X-Api-Key"] = "9abd1c4181d59dbece062455b941e64da474e5c7";

            if (this.IsAuthenticated)
            {
                request.Headers["X-User-Token"] = this.UserToken;
            }

            if (noCache)
            {
                request.Headers[HttpRequestHeader.Pragma] = "no-cache";
            }

            return request;
        }
    }
}