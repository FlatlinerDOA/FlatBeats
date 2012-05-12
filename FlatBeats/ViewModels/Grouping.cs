
namespace FlatBeats.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// A class used to expose the Key property on a dynamically-created Linq grouping.
    /// The grouping will be generated as an internal class, so the Key property will not
    /// otherwise be available to databind.
    /// </summary>
    /// <typeparam name="TKey">The type of the key.</typeparam>
    /// <typeparam name="TElement">The type of the items.</typeparam>
    public sealed class Grouping<TKey, TElement> : IGrouping<TKey, TElement>
    {
        private readonly IGrouping<TKey, TElement> internalGrouping;

        public Grouping(IGrouping<TKey, TElement> grouping)
        {
            this.internalGrouping = grouping;
        }

        public override bool Equals(object obj)
        {
            Grouping<TKey, TElement> that = obj as Grouping<TKey, TElement>;

            return (that != null) && (this.Key.Equals(that.Key));
        }

        public override int GetHashCode()
        {
            return Key.GetHashCode();
        }

        #region IGrouping<TKey,TElement> Members

        public TKey Key
        {
            get { return this.internalGrouping.Key; }
        }

        #endregion

        public bool HasItems { get
        {
            return this.internalGrouping.Any();
        } }
        #region IEnumerable<TElement> Members

        public IEnumerator<TElement> GetEnumerator()
        {
            return this.internalGrouping.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.internalGrouping.GetEnumerator();
        }

        #endregion
    }
}
