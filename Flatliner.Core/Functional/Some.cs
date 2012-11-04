namespace Flatliner.Functional
{
    using System;

    public struct Some<T> : IMaybe<T>
    {
        private readonly T value;

        public Some(T value)
        {
            this.value = value;
        }

        public bool HasValue
        {
            get { return true; }
        }

        public T Value
        {
            get { return this.value; }
        }

        public IMaybe<TResult> TransposeEmpty<TResult>()
        {
            return new Error<TResult>(new InvalidOperationException("No conversion from " + typeof(T).Name + " to " + typeof(TResult).Name + "."));
        }

        public override string ToString()
        {
            return "Some<" + typeof(T).Name + ">(" + (ReferenceEquals(this.value, null) ? "null" : this.value.ToString()) + ")";
        }
    }
}