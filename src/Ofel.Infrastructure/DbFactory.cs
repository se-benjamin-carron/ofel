using Microsoft.EntityFrameworkCore;
using Ofel.Infrastructure.Data;
using Ofel.Infrastructure.Repositories;
using Ofel.Infrastructure.Seeding;
using Ofel.Core.Interfaces;

namespace Ofel.Infrastructure.Utils
{
    public static class OfelDbFactory
    {
        /// <summary>
        /// Crée un contexte SQLite en mémoire avec la DB complète depuis CSV.
        /// </summary>
        public static async Task<ISteelRepository> CreateFullDbFromCsvAsync()
        {
            var options = new DbContextOptionsBuilder<OfelDbContext>()
                .UseSqlite("DataSource=:memory:")
                .Options;

            var context = new OfelDbContext(options);
            context.Database.OpenConnection();
            context.Database.EnsureCreated();

            var repo = new SteelRepository(context);

            var importer = new SteelCsvImporter(repo);
            await importer.ImportIfEmptyAsync();

            return repo;
        }
    }
}
