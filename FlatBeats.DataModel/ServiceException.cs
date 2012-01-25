namespace FlatBeats.DataModel
{
    using System;
    using System.Linq;

    public class ServiceException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

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

        public int StatusCode { get; private set; }
    }
}