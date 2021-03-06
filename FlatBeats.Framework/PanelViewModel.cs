﻿namespace FlatBeats.ViewModels
{
    using System;
    using System.Net;
    using System.Net.Sockets;

    using FlatBeats.DataModel;

    using Flatliner.Phone;
    using Flatliner.Phone.ViewModels;

    using Microsoft.Phone.Reactive;
    using System.Windows;
    
    public abstract class PanelViewModel : Flatliner.Phone.ViewModels.PanelViewModel
    {
        /// <summary>
        /// Initializes a new instance of the PanelViewModel class.
        /// </summary>
        protected PanelViewModel()
        {
            this.Opacity = 1;
            this.RegisterErrorHandler<SocketException>(GetSocketErrorMessage);
            this.RegisterErrorHandler<ServiceException>(GetServiceErrorMessage);
            this.RegisterErrorHandler<WebException>(GetWebErrorMessage);
            this.RegisterErrorHandler<Exception>(GetUnknownErrorMessage);
        }

        internal static ErrorMessage GetUnknownErrorMessage(Exception error)
        {
            if (error.Message.Contains("AG_E_NETWORK_ERROR"))
            {
                return new ErrorMessage(Framework.StringResources.Error_ServerUnavailable_Title, Framework.StringResources.Error_ServerUnavailable_Message);
            }

            return new ErrorMessage(Framework.StringResources.Error_UnknownError_Title, Framework.StringResources.Error_UnknownError_Message)
                       {
                           IsCritical = true
                       };
        }

        internal static ErrorMessage GetSocketErrorMessage(SocketException ex)
        {
            return new ErrorMessage(Framework.StringResources.Error_NoNetwork_Title, Framework.StringResources.Error_NoNetwork_Message);
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
                            return new ErrorMessage(Framework.StringResources.Error_ServerUnavailable_Title, Framework.StringResources.Error_ServerUnavailable_Message);
                        case HttpStatusCode.RequestEntityTooLarge:
                        case HttpStatusCode.BadRequest:
                        case HttpStatusCode.InternalServerError:
                        case HttpStatusCode.RequestUriTooLong:
                        case HttpStatusCode.NotImplemented:
                        case HttpStatusCode.HttpVersionNotSupported:
                            return new ErrorMessage(Framework.StringResources.Error_BadRequest_Title, Framework.StringResources.Error_BadRequest_Message);
                    }
                }

                return new ErrorMessage(Framework.StringResources.Error_ServerUnavailable_Title, Framework.StringResources.Error_ServerUnavailable_Message);
        }

        private double opacity;

        public double Opacity
        {
            get
            {
                return this.opacity;
            }

            set
            {
                if (this.opacity == value)
                {
                    return;
                }

                this.opacity = value;
                this.OnPropertyChanged(() => this.Opacity);
            }
        }

        public void Display()
        {
            this.Opacity = 1;
        }


        public static ErrorMessage GetServiceErrorMessage(ServiceException ex)
        {
            return new ErrorMessage(Framework.StringResources.Error_ServerUnavailable_Title, ex.Message);
        }
    }
}