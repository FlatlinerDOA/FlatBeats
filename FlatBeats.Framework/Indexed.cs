namespace FlatBeats.ViewModels
{
    public class Indexed<T>
    {
        /// <summary>
        /// Initializes a new instance of the Index class.
        /// </summary>
        public Indexed(T item, int index)
        {
            this.Item = item;
            this.Index = index;
        }

        public T Item { get; private set; }
        public int Index { get; private set; }
    }
}