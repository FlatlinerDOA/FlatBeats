namespace FlatBeats.ViewModels
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using Flatliner.Phone;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;
    
    public abstract class PanelViewModel : Flatliner.Phone.ViewModels.PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the PanelViewModel class.
        /// </summary>
        protected PanelViewModel()
        {
            this.RegisterErrorHandler<SocketException>(ex => StringResources.Error_NoNetwork);
            this.RegisterErrorHandler<WebException>(GetWebExceptionMessage);
            this.RegisterErrorHandler<Exception>(ex => StringResources.Error_UnknownError);
        }

        internal static string GetWebExceptionMessage(WebException webException)
        {
                var webResponse = webException.Response as HttpWebResponse;
                if (webResponse != null)
                {
                    var statusCode = webResponse.StatusCode;
                    switch (statusCode)
                    {
                        case HttpStatusCode.MovedPermanently:
                        case HttpStatusCode.Unauthorized:
                        case HttpStatusCode.PaymentRequired:
                        case HttpStatusCode.Forbidden:
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.MethodNotAllowed:
                        case HttpStatusCode.Gone:
                        case HttpStatusCode.ExpectationFailed:
                        case HttpStatusCode.BadGateway:
                        case HttpStatusCode.ServiceUnavailable:
                        case HttpStatusCode.GatewayTimeout:
                            return StringResources.Error_ServerUnavailable;
                        case HttpStatusCode.RequestEntityTooLarge:
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.RequestUriTooLong:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.NotImplemented:
                        case HttpStatusCode.HttpVersionNotSupported:
                            return StringResources.Error_BadRequest;
                    }
                }

                return StringResources.Error_ServerUnavailable;
        }

    }
}