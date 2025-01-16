namespace WistBot
{
    public interface IAction<in T>
    {
        Task Execute(T obj, CancellationToken token);
    }
}
