
namespace FlatBeats.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Net;
    using System.Net.Sockets;

    public class ViewModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (null != handler)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        protected string GetMessageForException(Exception exception)
        {
            if (exception is SocketException)
            {
                return StringResources.Error_NoNetwork;
            }

            if (exception is WebException)
            {
                var statusCode = ((HttpWebResponse)((WebException)exception).Response).StatusCode;
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
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            LittleWatson.ReportException(exception, this.GetType().FullName);
            return StringResources.Error_UnknownError;
        }
    }
}
