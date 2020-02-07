namespace ShellTemperature.ViewModels.Interfaces
{
    public interface ISubject<in T>
    {
        void Attach(T observer);
    }
}