namespace FlatBeats.ViewModels
{
    using System.Collections.Generic;

    public class Page<T>
    {
        /// <summary>
        /// Initializes a new instance of the Page class.
        /// </summary>
        public Page()
        {
            
        }

        /// <summary>
        /// Initializes a new instance of the Page class.
        /// </summary>
        public Page(IList<T> items, int pageNumber, int pageSize)
        {
            this.Items = items;
            this.PageNumber = pageNumber;
            this.PageSize = pageSize;
        }

        public IList<T> Items { get; set; }

        public int PageNumber { get; set; }

        public int PageSize { get; set; }
    }
}