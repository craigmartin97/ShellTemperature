using ShellTemperature.Data;

namespace ShellTemperature.Repository
{
    /// <summary>
    /// Base repository class that all repositories can extend from
    /// </summary>
    public abstract class BaseRepository
    {
        /// <summary>
        /// Database context to access database
        /// </summary>
        protected readonly ShellDb Context;

        protected BaseRepository(ShellDb context)
        {
            this.Context = context;
        }
    }
}