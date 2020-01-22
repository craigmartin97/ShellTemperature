using ShellTemperature.Models;

namespace ShellTemperature.Repository
{
    public class ShellTemperatureRepository : IRepository<Models.ShellTemp>
    {
        private readonly ShellDb _context;

        public ShellTemperatureRepository(ShellDb context)
        {
            _context = context;
        }

        public bool Create(Models.ShellTemp model)
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
