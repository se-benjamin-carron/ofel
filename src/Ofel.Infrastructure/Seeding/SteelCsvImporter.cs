using Ofel.Infrastructure.Models;
using Ofel.Infrastructure.Repositories;
using Ofel.Infrastructure.IO; // <-- ton OfelPaths
using System.Globalization;

namespace Ofel.Infrastructure.Seeding;

public class SteelCsvImporter
{
    private readonly SteelRepository _repo;

    public SteelCsvImporter(SteelRepository repo)
    {
        _repo = repo;
    }

    public async Task ImportIfEmptyAsync()
    {
        if (await _repo.HasAnyAsync())
            return;

        // Utilise le DataDirectory défini globalement
        var materialsPath = Path.Combine(OfelPaths.DataDirectory, "steel_material.csv");
        var sectionsPath = Path.Combine(OfelPaths.DataDirectory, "steel_section.csv");

        var materials = LoadMaterials(materialsPath);
        var sections = LoadSections(sectionsPath);

        await _repo.SeedIfEmptyAsync(materials, sections);
    }

    private static List<SteelMaterialEntity> LoadMaterials(string path)
    {
        return File.ReadAllLines(path)
            .Skip(1)
            .Select(l => l.Split(','))
            .Select(c => new SteelMaterialEntity
            {
                Name = c[0],
                Standard = c[1],
                Fy = double.Parse(c[2], CultureInfo.InvariantCulture),
                Fu = double.Parse(c[3], CultureInfo.InvariantCulture),
                E = double.Parse(c[4], CultureInfo.InvariantCulture),
                G = double.Parse(c[5], CultureInfo.InvariantCulture),
                Rho = double.Parse(c[6], CultureInfo.InvariantCulture),
                Alpha = double.Parse(c[7], CultureInfo.InvariantCulture)
            })
            .ToList();
    }

    private static List<SteelSectionEntity> LoadSections(string path)
    {
        return File.ReadAllLines(path)
            .Skip(1)
            .Select(l => l.Split(','))
            .Select(c => new SteelSectionEntity
            {
                ProfileType = c[0],
                Name = c[1],
                H = double.Parse(c[2], CultureInfo.InvariantCulture),
                B = double.Parse(c[3], CultureInfo.InvariantCulture),
                T_w = double.Parse(c[4], CultureInfo.InvariantCulture),
                T_f = double.Parse(c[5], CultureInfo.InvariantCulture),
                R_1 = double.Parse(c[6], CultureInfo.InvariantCulture),
                R_2 = double.Parse(c[7], CultureInfo.InvariantCulture),
                A = double.Parse(c[8], CultureInfo.InvariantCulture),
                A_y = double.Parse(c[9], CultureInfo.InvariantCulture),
                A_z = double.Parse(c[10], CultureInfo.InvariantCulture),
                I_y = double.Parse(c[11], CultureInfo.InvariantCulture),
                I_z = double.Parse(c[12], CultureInfo.InvariantCulture),
                I_t = double.Parse(c[13], CultureInfo.InvariantCulture),
                I_w = double.Parse(c[14], CultureInfo.InvariantCulture),
                W_el_y = double.Parse(c[15], CultureInfo.InvariantCulture),
                W_el_z = double.Parse(c[16], CultureInfo.InvariantCulture),
                W_pl_y = double.Parse(c[17], CultureInfo.InvariantCulture),
                W_pl_z = double.Parse(c[18], CultureInfo.InvariantCulture)
            })
            .ToList();
    }
}
