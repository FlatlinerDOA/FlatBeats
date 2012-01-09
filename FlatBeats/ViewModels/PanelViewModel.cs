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
            this.RegisterErrorHandler<SocketException>(GetSocketErrorMessage);
            this.RegisterErrorHandler<WebException>(GetWebErrorMessage);
            this.RegisterErrorHandler<Exception>(GetUnknownErrorMessage);
        }

        internal static ErrorMessage GetUnknownErrorMessage(Exception error)
        {
            return new ErrorMessage(
                StringResources.Error_UnknownError_Title, StringResources.Error_UnknownError_Message) { IsCritical = true };
        }

        internal static ErrorMessage GetSocketErrorMessage(SocketException ex)
        {
            return new ErrorMessage(StringResources.Error_NoNetwork_Title, StringResources.Error_NoNetwork_Message);
        }

        internal static ErrorMessage GetWebErrorMessage(WebException webException)
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
                            return new ErrorMessage(StringResources.Error_ServerUnavailable_Title, StringResources.Error_ServerUnavailable_Message);
                        case HttpStatusCode.RequestEntityTooLarge:
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.RequestUriTooLong:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.NotImplemented:
                        case HttpStatusCode.HttpVersionNotSupported:
                            return new ErrorMessage(StringResources.Error_BadRequest_Title, StringResources.Error_BadRequest_Message);
                    }
                }

                return new ErrorMessage(StringResources.Error_ServerUnavailable_Title, StringResources.Error_ServerUnavailable_Message);
        }
    }
}