namespace Flatliner.Portable
{
    public interface IFactory<T>
    {
        T Create();
    }

    public interface IFactory<TArg, T>
    {
        T Create(TArg arg);
    }
}