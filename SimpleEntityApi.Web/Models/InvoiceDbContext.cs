using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using SimpleEntityApi.Web.Migrations;

namespace SimpleEntityApi.Web.Models
{
    public class InvoiceDbContext : DbContext
    {
        public DbSet<Invoice> Invoices { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<InvoiceDbContext, Configuration>());
            base.OnModelCreating(modelBuilder);
            
        }
    }

    
}