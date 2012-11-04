namespace Flatliner.Functional
{
    using System;

    public struct Error<T> : IMaybe<T>
    {
        private readonly Exception error;

        public Error(Exception error)
        {
            this.error = error;
        }

        public Exception Exception
        {
            get
            {
                return this.error;
            }
        }


        public bool HasValue
        {
            get { return false; }
        }

        public T Value
        {
            get { throw this.error; }
        }

        public IMaybe<TResult> TransposeEmpty<TResult>()
        {
            return new Error<TResult>(this.Exception);
        }

        public override string ToString()
        {
            return "Error<" + typeof(T).Name + ">(" + this.error + ")";
        }
    }
}