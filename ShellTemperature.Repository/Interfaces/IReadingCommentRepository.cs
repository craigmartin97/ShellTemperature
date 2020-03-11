namespace ShellTemperature.Repository.Interfaces
{
    public interface IReadingCommentRepository<T> : IRepository<T>
    {
        T GetItem(string comment);
    }
}