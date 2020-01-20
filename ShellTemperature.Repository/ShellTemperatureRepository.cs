using ShellTemperature.Models;

namespace ShellTemperature.Repository
{
    public class ShellTemperatureRepository : IRepository<Models.ShellTemperature>
    {
        private readonly ShellDb _context;

        public ShellTemperatureRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(Models.ShellTemperature model)
        {
            if (model != null)
            {
                _context.Add(model);
                _context.SaveChangesAsync();
                return true;
            }

            return false;
        }
    }
}
