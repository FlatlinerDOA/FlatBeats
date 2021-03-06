namespace FlatBeats.DataModel.Services
{
    using System;
    using System.IO;
    using System.Net;

    using Flatliner.Functional;

    using Microsoft.Phone.Reactive;

    using SharpGIS.ZLib;

    internal static class DownloadExtensions
    {
        /// <summary>
        /// </summary>
        /// <param name = "items">
        /// </param>
        /// <param name = "selector">
        /// </param>
        /// <typeparam name = "T">
        /// </typeparam>
        /// <typeparam name = "TResult">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public static IObservable<TResult> TrySelect<T, TResult>(this IObservable<T> items, Func<T, TResult> selector)
        {
            return Observable.CreateWithDisposable<TResult>(
                d => items.Subscribe(
                    item =>
                        {
                            try
                            {
                                TResult result = selector(item);
                                d.OnNext(result);
                            }
                            catch (WebException webException)
                            {
                                Exception newError = ConvertWebException(webException);
                                d.OnError(newError);
                            }
                            catch (ZlibException zlibException)
                            {
                                var zlibError = new ServiceException("The response from the server was interrupted, please try again later", zlibException, 444);
                                d.OnError(zlibError);
                            }
                            catch (Exception ex)
                            {
                                d.OnError(ex);
                            }
                        },
                    d.OnError,
                    d.OnCompleted));
        }


        /// <summary>
        /// </summary>
        /// <param name = "items">
        /// </param>
        /// <param name = "selector">
        /// </param>
        /// <typeparam name = "T">
        /// </typeparam>
        /// <typeparam name = "TResult">
        /// </typeparam>
        /// <returns>
        /// </returns>
        public static IObservable<PortableUnit> Try<T>(this IObservable<T> items, Action<T> selector)
        {
            return Observable.CreateWithDisposable<PortableUnit>(
                d => items.Subscribe(
                    item =>
                    {
                        try
                        {
                            selector(item);
                            d.OnNext(new PortableUnit());
                        }
                        catch (WebException webException)
                        {
                            Exception newError = ConvertWebException(webException);
                            d.OnError(newError);
                        }
                        catch (ZlibException zlibException)
                        {
                            var zlibError = new ServiceException("The response from the server was interrupted, please try again later", zlibException, 444);
                            d.OnError(zlibError);
                        }
                        catch (Exception ex)
                        {
                            d.OnError(ex);
                        }
                    },
                    d.OnError,
                    d.OnCompleted));
        }

        public static IObservable<T> HandleWebException<T>(WebException ex)
        {
            return Observable.Throw<T>(ConvertWebException(ex));
        }

        private static Exception ConvertWebException(WebException webException)
        {
            Exception newError = webException;
            try
            {
                if (webException.Response != null)
                {
                    using (var s = webException.Response.GetResponseStream())
                    {
                        var b = new MemoryStream();
                        s.CopyTo(b);
                        b.Position = 0;
                        using (var sr = new StreamReader(b))
                        {
                            var response = Json<ResponseContract>.Instance.DeserializeFromString(sr.ReadToEnd());
                            if (response != null)
                            {
                                newError = new ServiceException(response.Errors, webException, response.ResponseStatus);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Ignore all at this point
            }

            return newError;
        }   
    }
}