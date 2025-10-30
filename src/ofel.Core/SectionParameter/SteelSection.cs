using System;

using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System;
namespace ofel.Core
{
    /// <summary>
    /// Représente une section acier (valeurs lues depuis steel_section.csv)
    /// </summary>
    public class SteelSection : IGeometry
    {
        public string Type { get; } = "SteelSection";
        // profile type is the first column in the CSV (e.g., IPE)
        public string ProfileType { get; }
        public string Name { get; }
        public double H { get; }
        public double B { get; }
        public double T_w { get; }
        public double T_f { get; }
        public double R_1 { get; }
        public double R_2 { get; }
        public double A { get; }
        public double A_y { get; }
        public double A_z { get; }
        public double I_y { get; }
    public double I_z { get; }
    public double I_t { get; }
    public double I_w { get; }

    public SteelSection(string profileType, string name, double h, double b, double tw, double tf, double r1, double r2, double a, double ay, double az, double iy, double iz, double it, double iw)
        {
            ProfileType = profileType;
            Name = name;
            H = h; B = b; T_w = tw; T_f = tf; R_1 = r1; R_2 = r2; A = a; A_y = ay; A_z = az; I_y = iy; I_z = iz;
            I_t = it; I_w = iw;
        }

        public override string ToString() => $"SteelSection(Name={Name})";

        /// <summary>
        /// Loads steel sections from CSV. Expects header: type,name,h,b,tw,tf,r1,r2,a,a_y,a_z,i_y,i_z,i_t,...
        /// </summary>
        public static IReadOnlyList<SteelSection> LoadFromCsv(string csvPath)
        {
            var list = new List<SteelSection>();
            if (!File.Exists(csvPath)) throw new FileNotFoundException(csvPath);
            foreach (var line in File.ReadLines(csvPath))
            {
                var trimmed = line.Trim();
                if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                    continue;
                var parts = trimmed.Split(',');
                if (parts.Length < 14) continue;
                try {
                    string type = parts[0].Trim('"');
                    string name = parts[1].Trim('"');
                    double h = double.Parse(parts[2], CultureInfo.InvariantCulture);
                    double b = double.Parse(parts[3], CultureInfo.InvariantCulture);
                    double tw = double.Parse(parts[4], CultureInfo.InvariantCulture);
                    double tf = double.Parse(parts[5], CultureInfo.InvariantCulture);
                    double r1 = double.Parse(parts[6], CultureInfo.InvariantCulture);
                    double r2 = double.Parse(parts[7], CultureInfo.InvariantCulture);
                    double a = double.Parse(parts[8], CultureInfo.InvariantCulture);
                    double ay = double.Parse(parts[9], CultureInfo.InvariantCulture);
                    double az = double.Parse(parts[10], CultureInfo.InvariantCulture);
                    double iy = double.Parse(parts[11], CultureInfo.InvariantCulture);
                    double iz = double.Parse(parts[12], CultureInfo.InvariantCulture);
                    double it = double.Parse(parts[13], CultureInfo.InvariantCulture);
                    list.Add(new SteelSection(type, name, h, b, tw, tf, r1, r2, a, ay, az, iy, iz, it, 0));
                } catch {
                    // ignore malformed
                }
            }
            return list;
        }

        // Chemin CSV global paramétrable pour les sections acier
        public static string CsvPath { get; set; } = "data/steel_section.csv";

        /// <summary>
        /// Trouve une section par type de profil et nom, en utilisant CsvPath par défaut si non fourni.
        /// </summary>
        public static SteelSection GetByTypeAndName(string profileType, string name, string csvPath = "data/steel_section.csv")
        {
            var path = csvPath ?? CsvPath;
            var list = LoadFromCsv(path);
            var sec = list.FirstOrDefault(s => string.Equals(s.ProfileType, profileType, StringComparison.OrdinalIgnoreCase)
                                            && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (sec == null) throw new KeyNotFoundException($"SteelSection not found: {profileType}, {name}");
            return sec;
        }

        /// <summary>
        /// Interpolates geometry attributes between this and another section.
        /// </summary>
        public IGeometry Interpolate(IGeometry other, double t)
        {
            if (other is not SteelSection o) return this;
            // simple select by weight or blend numeric properties
            return t < 0.5 ? this : o;
        }
    }
}
