namespace Flatliner.Phone.ViewModels
{
    using System;

    public class ErrorMessage
    {
        /// <summary>
        /// Initializes a new instance of the ErrorMessage class.
        /// </summary>
        public ErrorMessage()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the ErrorMessage class.
        /// </summary>
        public ErrorMessage(string title, string message)
        {
            this.Title = title;
            this.Message = message;
        }

        public string Title { get; set; }

        public string Message { get; set; }

        public bool IsCritical { get; set; }
    }
}
