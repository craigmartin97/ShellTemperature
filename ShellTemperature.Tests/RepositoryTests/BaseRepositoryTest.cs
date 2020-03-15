using Microsoft.EntityFrameworkCore;
using ShellTemperature.Data;
using System;

namespace ShellTemperature.Tests.RepositoryTests
{
    public class BaseRepositoryTest
    {
        protected ShellDb Context;

        protected ShellDb GetShellDb()
        {
            var options = new DbContextOptionsBuilder<ShellDb>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ShellDb(options);
        }
    }
}