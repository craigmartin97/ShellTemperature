using Microsoft.EntityFrameworkCore;

namespace ShellTemperature.Data
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

        #region Tables
        public DbSet<ShellTemp> ShellTemperatures { get; set; }
        public DbSet<SdCardShellTemp> SdCardShellTemperatures { get; set; }
        public DbSet<DeviceInfo> DevicesInfo { get; set; }
        public DbSet<ShellTemperatureComment> ShellTemperatureComments { get; set; }
        public DbSet<SdCardShellTemperatureComment> SdCardShellTemperatureComments { get; set; }
        public DbSet<ReadingComment> ReadingComments { get; set; }
        public DbSet<Positions> Positions { get; set; }
        public DbSet<ShellTemperaturePosition> ShellTemperaturePositions { get; set; }
        #endregion
    }
}