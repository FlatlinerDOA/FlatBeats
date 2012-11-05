namespace Flatliner.Phone.Data
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ListGrouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly IGrouping<TKey, TElement> grouping;

        public ListGrouping(IGrouping<TKey, TElement> unit)
        {
            grouping = unit;
        }

        public TKey Key
        {
            get { return grouping.Key; }
        }

        public IEnumerator<TElement> GetEnumerator()
        {
            return grouping.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return grouping.GetEnumerator();
        }
    }
}
