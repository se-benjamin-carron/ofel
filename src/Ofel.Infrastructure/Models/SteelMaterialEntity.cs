namespace Ofel.Infrastructure.Models
{
    public class SteelMaterialEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Standard { get; set; } = string.Empty;

        // Propriétés matérielles
        public double Fy { get; set; }
        public double Fu { get; set; }
        public double E { get; set; }
        public double G { get; set; }
        public double Rho { get; set; }
        public double Alpha { get; set; } = 12e-6;
    }
}
