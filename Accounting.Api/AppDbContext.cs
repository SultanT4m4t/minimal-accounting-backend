using Accounting.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Accounting.Api
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<Account> Accounts => Set<Account>();
        public DbSet<Transaction> Transactions => Set<Transaction>();
    }
}
