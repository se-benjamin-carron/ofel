//using Microsoft.EntityFrameworkCore;
//using Ofel.Core.SectionParameter;
//using Ofel.Infrastructure.Data;
//using Ofel.Infrastructure.Repositories;

//namespace Ofel.Core.Utils
//{
//    public static class OfelDbFactory
//    {
//        /// <summary>
//        /// Crée un contexte SQLite en mémoire et un repository prêt.
//        /// </summary>
//        public static async Task<SteelRepository> CreateInMemoryDbWithSeedAsync()
//        {
//            var options = new DbContextOptionsBuilder<OfelDbContext>()
//                .UseSqlite("DataSource=:memory:")
//                .Options;

//            var context = new OfelDbContext(options);
//            context.Database.OpenConnection();
//            context.Database.EnsureCreated();

//            var repo = new SteelRepository(context);

//            // Seed de test minimal
//            if (!await repo.HasAnyAsync())
//            {
//                var materials = new[]
//                {
//                    new Ofel.Infrastructure.Models.SteelMaterialEntity
//                    {
//                        Name = "S235",
//                        Standard = "EN 10025",
//                        Fy = 235,
//                        Fu = 360,
//                        E = 210_000,
//                        G = 81_000,
//                        Rho = 7850
//                    }
//                };

//                var sections = new[]
//                {
//                    new Ofel.Infrastructure.Models.SteelSectionEntity
//                    {
//                        ProfileType = "IPE",
//                        Name = "IPE200",
//                        H = 200,
//                        B = 100,
//                        T_w = 5.6,
//                        T_f = 8.5,
//                        R_1 = 12,
//                        R_2 = 6,
//                        A = 26.2,
//                        A_y = 13.1,
//                        A_z = 8.2,
//                        I_y = 2060,
//                        I_z = 142,
//                        I_t = 8.5,
//                        I_w = 0
//                    }
//                };

//                await repo.SeedIfEmptyAsync(materials, sections);
//            }

//            return repo;
//        }

//        /// <summary>
//        /// Retourne toutes les matières SteelMaterial depuis le repository.
//        /// </summary>
//        public static async Task<List<SteelMaterial>> LoadSteelMaterialsAsync(SteelRepository repo)
//        {
//            var entities = await repo.GetAllMaterialsAsync(); // il faudra créer cette méthode
//            return entities.Select(e =>
//                new SteelMaterial(e.Name, e.Standard, e.Fy, e.Fu, e.E, e.G, e.Rho)
//            ).ToList();
//        }
//    }
//}
