namespace Flatliner.Phone.Data
{
    using System;

    /// <summary>
    /// Provides constants for standard Http methods
    /// </summary>
    public static class HttpMethods
    {
        #region Constants and Fields

        /// <summary>
        /// Http method for deleting a resource (DELETE)
        /// </summary>
        public const string Delete = "DELETE";

        /// <summary>
        /// Http method for getting a resource. (GET)
        /// </summary>
        public const string Get = "GET";

        /// <summary>
        /// Http method for getting just the header information for a resource. (HEAD)
        /// </summary>
        public const string Head = "HEAD";

        /// <summary>
        /// Http method for posting a message to the resource. (POST)
        /// </summary>
        public const string Post = "POST";

        /// <summary>
        /// Http method for creatting or updating a resource. (PUT)
        /// </summary>
        public const string Put = "PUT";

        #endregion
    }
}
