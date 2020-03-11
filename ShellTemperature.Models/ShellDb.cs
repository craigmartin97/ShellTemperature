using Microsoft.EntityFrameworkCore;

namespace ShellTemperature.Models
{
    /// <summary>
    /// A class representinng the ladle shell database
    /// </summary>
    public class ShellDb : DbContext
    {
        #region Constructors
        public ShellDb()
        {

        }

        public ShellDb(DbContextOptions<ShellDb> dbContext) : base(dbContext)
        {

        }
        #endregion

        //protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        //{
        //    base.OnConfiguring(optionsBuilder);
        //    optionsBuilder.UseSqlServer(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=ShellDb;Integrated Security=True");
        //}

        public DbSet<ShellTemp> ShellTemperatures { get; set; }

        public DbSet<DeviceInfo> Devices { get; set; }

        public DbSet<ShellTemperatureComment> ShellTemperatureComments { get; set; }
    }
}
