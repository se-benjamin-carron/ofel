using Microsoft.EntityFrameworkCore;
using Ofel.Core.Interfaces;
using Ofel.Core.SectionParameter;
using Ofel.Infrastructure.Data;
using Ofel.Infrastructure.Models;

namespace Ofel.Infrastructure.Repositories
{
    public class SteelRepository : ISteelRepository
    {
        private readonly OfelDbContext _db;
        public SteelRepository(OfelDbContext db) => _db = db;

        public async Task<bool> HasAnyAsync()
        {
            return await _db.SteelMaterials.AnyAsync()
                || await _db.SteelSections.AnyAsync();
        }

        public async Task SeedIfEmptyAsync(List<SteelMaterialEntity> materials, List<SteelSectionEntity> sections)
        {
            if (!await _db.SteelMaterials.AnyAsync())
            {
                var entities = materials.Select(m => new SteelMaterialEntity
                {
                    Name = m.Name,
                    Standard = m.Standard,
                    Fy = m.Fy,
                    Fu = m.Fu,
                    E = m.E,
                    G = m.G,
                    Rho = m.Rho,
                    Alpha = m.Alpha
                });
                await _db.SteelMaterials.AddRangeAsync(entities);
            }

            if (!await _db.SteelSections.AnyAsync())
            {
                var entities = sections.Select(s => new SteelSectionEntity
                {
                    ProfileType = s.ProfileType,
                    Name = s.Name,
                    H = s.H,
                    B = s.B,
                    T_w = s.T_w,
                    T_f = s.T_f,
                    R_1 = s.R_1,
                    R_2 = s.R_2,
                    A = s.A,
                    A_y = s.A_y,
                    A_z = s.A_z,
                    I_y = s.I_y,
                    I_z = s.I_z,
                    I_t = s.I_t,
                    I_w = s.I_w,
                    W_el_y = s.W_el_y,
                    W_el_z = s.W_el_z,
                    W_pl_y = s.W_pl_y,
                    W_pl_z = s.W_pl_z
                });
                await _db.SteelSections.AddRangeAsync(entities);
            }

            await _db.SaveChangesAsync();
        }

        public async Task<SteelMaterial?> GetMaterialAsync(string name, string standard)
        {
            var ent = await _db.SteelMaterials
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Name == name && m.Standard == standard);

            if (ent == null) return null;

            return new SteelMaterial(ent.Name, ent.Standard, ent.Fy, ent.Fu, ent.E, ent.G, ent.Rho, ent.Alpha);
        }

        public async Task<SteelSection?> GetSectionAsync(string profileType, string name)
        {
            var ent = await _db.SteelSections
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.ProfileType == profileType && s.Name == name);

            if (ent == null) return null;

            return new SteelSection(
                ent.ProfileType,
                ent.Name,
                ent.H, ent.B, ent.T_w, ent.T_f, ent.R_1, ent.R_2,
                ent.A, ent.A_y, ent.A_z,
                ent.I_y, ent.I_z, ent.I_t, ent.I_w,
                ent.W_el_y, ent.W_el_z, ent.W_pl_y, ent.W_pl_z
            );
        }

        public async Task SeedIfEmptyAsync(IEnumerable<SteelMaterial> materials, IEnumerable<SteelSection> sections)
        {
            if (!await _db.SteelMaterials.AnyAsync())
            {
                var entities = materials.Select(m => new SteelMaterialEntity
                {
                    Name = m.Name,
                    Standard = m.Standard,
                    Fy = m.Fy,
                    Fu = m.Fu,
                    E = m.E,
                    G = m.G,
                    Rho = m.Rho,
                    Alpha = m.Alpha
                });
                await _db.SteelMaterials.AddRangeAsync(entities);
            }

            if (!await _db.SteelSections.AnyAsync())
            {
                var entities = sections.Select(s => new SteelSectionEntity
                {
                    ProfileType = s.ProfileType,
                    Name = s.Name,
                    H = s.H,
                    B = s.B,
                    T_w = s.T_w,
                    T_f = s.T_f,
                    R_1 = s.R_1,
                    R_2 = s.R_2,
                    A = s.A,
                    A_y = s.A_y,
                    A_z = s.A_z,
                    I_y = s.I_y,
                    I_z = s.I_z,
                    I_t = s.I_t,
                    I_w = s.I_w
                });
                await _db.SteelSections.AddRangeAsync(entities);
            }

            await _db.SaveChangesAsync();
        }

    }
}
