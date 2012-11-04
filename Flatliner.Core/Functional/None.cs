namespace Flatliner.Functional
{
    using System;

    public struct None<T> : IMaybe<T>
    {
        public bool HasValue
        {
            get
            {
                return false;
            }
        }

        public T Value
        {
            get
            {
                throw new InvalidOperationException("No value of " + typeof(T).Name);
            }
        }

        public IMaybe<TResult> TransposeEmpty<TResult>()
        {
            return new None<TResult>();
        }

        public override string ToString()
        {
            return "None<" + typeof(T).Name + ">()";
        }
    }
}