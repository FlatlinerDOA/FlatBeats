namespace Flatliner.Functional
{
    using System;

    public interface IMaybe<out T>
    {
        bool HasValue { get; }

        T Value { get; }

        IMaybe<TResult> TransposeEmpty<TResult>();
    }
}