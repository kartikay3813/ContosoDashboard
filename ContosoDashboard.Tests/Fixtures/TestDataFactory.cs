using ContosoDashboard.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace ContosoDashboard.Tests.Fixtures;

public static class TestDataFactory
{
    public static (ApplicationDbContext Context, SqliteConnection Connection) CreateContext()
    {
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        var options = new DbContextOptionsBuilder<ApplicationDbContext>().UseSqlite(connection).Options;
        var context = new ApplicationDbContext(options);
        context.Database.EnsureCreated();
        return (context, connection);
    }
}
