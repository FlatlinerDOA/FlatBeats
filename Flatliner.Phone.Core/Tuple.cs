namespace Flatliner.Phone
{
    public static class Tuple
    {
        public static Tuple<T1, T2> Create<T1, T2>(T1 arg1, T2 arg2)
        {
            return new Tuple<T1, T2>(arg1, arg2);
        }
    }

    public sealed class Tuple<T1, T2>
    {
        /// <summary>
        /// Initializes a new instance of the Tuple class.
        /// </summary>
        public Tuple(T1 arg1, T2 arg2)
        {
            this.Item1 = arg1;
            this.Item2 = arg2;
        }

        public T1 Item1
        {
            get;
            private set;
        }

        public T2 Item2
        {
            get;
            private set;
        }
    }
}
