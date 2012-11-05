namespace Flatliner.Functional
{
    public delegate void Observation<in T>(IMaybe<T> result);
}