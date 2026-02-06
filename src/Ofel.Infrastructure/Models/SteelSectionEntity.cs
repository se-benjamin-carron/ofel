namespace Ofel.Infrastructure.Models
{
    public class SteelSectionEntity
    {
        public int Id { get; set; }
        public string ProfileType { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;

        // Géométrie
        public double H { get; set; }
        public double B { get; set; }
        public double T_w { get; set; }
        public double T_f { get; set; }
        public double R_1 { get; set; }
        public double R_2 { get; set; }

        public double A { get; set; }
        public double A_y { get; set; }
        public double A_z { get; set; }
        public double I_y { get; set; }
        public double I_z { get; set; }
        public double I_t { get; set; }
        public double I_w { get; set; }
        public double W_el_y { get; set; }
        public double W_el_z { get; set; }
        public double W_pl_y { get; set; }
        public double W_pl_z { get; set; }
    }
}
