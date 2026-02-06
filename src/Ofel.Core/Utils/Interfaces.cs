using Ofel.Core.SectionParameter;

namespace Ofel.Core.Interfaces
{
    public interface ISteelRepository
    {
        Task<bool> HasAnyAsync();
        Task SeedIfEmptyAsync(IEnumerable<SteelMaterial> materials, IEnumerable<SteelSection> sections);
        Task<SteelMaterial?> GetMaterialAsync(string name, string standard);
        Task<SteelSection?> GetSectionAsync(string profileType, string name);
    }
}
