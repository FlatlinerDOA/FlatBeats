namespace FlatBeats.DataModel
{
    using System;
    using System.Linq;

    /// <summary>
    /// Thrown when the 8tracks web service has given us an error.
    /// </summary>
    public class ServiceException : Exception
    {
        public ServiceException()
        {
        }

        public ServiceException(string message)
            : base(message)
        {
        }

        public ServiceException(string message, Exception inner, string statusCode)
            : base(message, inner)
        {
            int status;
            if (int.TryParse(statusCode.Split(' ').FirstOrDefault(), out status))
            {
                this.StatusCode = status;
            }
        }

        public ServiceException(string message, Exception inner, int status)
            : base(message, inner)
        {
            this.StatusCode = status;
        }

        public int StatusCode { get; private set; }
    }
}