using Microsoft.EntityFrameworkCore;
using System;
using WorkforceManager.Data;

namespace WorkforceManager.UnitTests.Mocks
{
    public class DatabaseMock
    {
        public static WorkforceManagerDbContext Instance
        {
            get
            {
                var dbContextOptions = new DbContextOptionsBuilder<WorkforceManagerDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options;

                return new WorkforceManagerDbContext(dbContextOptions);
            }
        }
    }
}
