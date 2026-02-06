using System;

using System.Collections.Generic;
using System.IO;
using System.Globalization;
using System.Linq;
using System;
using Ofel.Core.StructureDesign.Eurocode3;
using System.Reflection.Metadata.Ecma335;
namespace Ofel.Core.SectionParameter
{
    public enum SteelManufacturingType
    {
        HotRolled,
        ColdFormed,
        Built,
        Default
    }

    public enum BucklingCurve
    {
        _A,
        _B,
        _C,
        _D
    }

    /// <summary>
    /// Représente une section acier (valeurs lues depuis steel_section.csv)
    /// </summary>
    public class SteelSection : IGeometry
    {
        // Propriétés communes
        public string Name { get; }
        public double A { get; }
        public double A_y { get; }
        public double A_z { get; }
        public double I_y { get; }
        public double I_z { get; }
        public double I_t { get; }
        public double I_w { get; }
        public double W_el_y { get; }
        public double W_el_z { get; }
        public double W_pl_y { get; }
        public double W_pl_z { get; }
        public string MaterialType { get; } = "steel_section";
        public SteelManufacturingType ManufacturingType { get; set; }

        // Propriétés spécifiques à la section
        public string ProfileType { get; }
        public double H { get; }
        public double B { get; }
        public double T_w { get; }
        public double T_f { get; }
        public double R_1 { get; }
        public double R_2 { get; }

        public double GetInertia(Axis axis)
        {
            return axis switch
            {
                Axis.Y => I_y,
                Axis.Z => I_z,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be Y or Z")
            };
        }

        public double GetInertiaModulus(Axis axis, ClassSection SectionClass)
        {
            return axis switch
            {
                Axis.Y => SectionClass.DesignClass < 3 ? W_pl_y : W_el_y,
                Axis.Z => SectionClass.DesignClass < 3 ? W_pl_z : W_el_z,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be Y or Z")
            };
        }
        public double GetInertiaModulusByInt(Axis axis, int SectionClass)
        {
            return axis switch
            {
                Axis.Y => SectionClass < 3 ? W_pl_y : W_el_y,
                Axis.Z => SectionClass < 3 ? W_pl_z : W_el_z,
                _ => throw new ArgumentOutOfRangeException(nameof(axis), "Axis must be Y or Z")
            };
        }

        public double GetLambdaLT0()
        {
            return ProfileType switch
            {
                "PRS" => 0.3 * B / H,
                "IPE" or "HEA" => 0.2 + 0.1 * B / H,
                _ => 0.2,
            };
        }
        public double GetAlphaLT0(double lambdaLT)
        {
            return ProfileType switch
            {
                "PRS" => Math.Max(0.5 - 0.25 * B / H * Math.Pow(lambdaLT, 2), 0),
                "IPE" or "HEA" => Math.Max(0.4 - 0.2 * B / H * Math.Pow(lambdaLT, 2), 0),
                _ => 0.76,
            };
        }

        public double GetI0()
        {
            return I_y + I_z;
        }
        public double GetIw()
        {
            if (I_w != 0.0)
            {
                return I_w;
            }
            else
            {
                return 1.0;
            }
        }

        public SteelSection(
            string profileType, string name,
            double h, double b, double tw, double tf, double r1, double r2,
            double a, double ay, double az, double iy, double iz, double it, double iw, double w_el_y, double w_el_z, double w_pl_y, double w_pl_z)
        {
            ProfileType = profileType;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            H = h;
            B = b;
            T_w = tw;
            T_f = tf;
            R_1 = r1;
            R_2 = r2;

            A = a;
            A_y = ay;
            A_z = az;
            I_y = iy;
            I_z = iz;
            I_t = it;
            I_w = iw;
            W_el_y = w_el_y;
            W_el_z = w_el_z;
            W_pl_y = w_pl_y;
            W_pl_z = w_pl_z;
            ManufacturingType = this.GetManufacturingType();
        }

        public BucklingCurve GetLateralBucklingCurve()
        {
            return ProfileType switch
            {
                "IPE" or "HEA" => H / B <= 2 ? BucklingCurve._A : BucklingCurve._B,
                "CAE" => BucklingCurve._D,
                "TCAR" => BucklingCurve._D,
                "PRS" => H / B <= 2 ? BucklingCurve._C : BucklingCurve._D,
                _ => BucklingCurve._C
            };
        }

        private SteelManufacturingType GetManufacturingType()
        {
            var hotRolledTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "IPE", "HEA", "CAE"
            };
            var coldFormedTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "TCAR"
            };
            var BuiltTypes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "PRS"
            };
            if (hotRolledTypes.Contains(ProfileType))
                return SteelManufacturingType.HotRolled;
            else if (coldFormedTypes.Contains(ProfileType))
                return SteelManufacturingType.ColdFormed;
            else if (BuiltTypes.Contains(ProfileType))
                return SteelManufacturingType.Built;
            else
                return SteelManufacturingType.Default;
        }
        public BucklingCurve GetBucklingCurve(Axis axis)
        {
            double hOverB = H / B;
            bool slender = hOverB >= 1.2;

            return ProfileType switch
            {
                "IPE" or "HEA" => GetIProfileCurve(axis, slender),
                "CAE" => BucklingCurve._B,
                "TCAR" => ManufacturingType switch
                {
                    SteelManufacturingType.ColdFormed => BucklingCurve._C,
                    SteelManufacturingType.HotRolled => BucklingCurve._A,
                    _ => BucklingCurve._C
                },
                "PRS" => GetPRSProfileCurve(axis),
                _ => BucklingCurve._C
            };
        }
        private BucklingCurve GetIProfileCurve(Axis axis, bool slender)
        {
            if (axis == Axis.Y)
            {
                return slender
                    ? (T_f <= 40e-3 ? BucklingCurve._A : BucklingCurve._B)
                    : (T_f <= 100e-3 ? BucklingCurve._B : BucklingCurve._D);
            }

            return slender
                ? (T_f <= 40e-3 ? BucklingCurve._B : BucklingCurve._C)
                : (T_f <= 100e-3 ? BucklingCurve._C : BucklingCurve._D);
        }
        private BucklingCurve GetPRSProfileCurve(Axis axis)
        {
            return axis switch
            {
                Axis.Y => T_f <= 40e-3 ? BucklingCurve._B : BucklingCurve._C,
                Axis.Z => T_f <= 40e-3 ? BucklingCurve._C : BucklingCurve._D,
                _ => BucklingCurve._C
            };
        }


        public IGeometry Clone()
        {
            // Retourne une copie indépendante
            return new SteelSection(ProfileType, Name, H, B, T_w, T_f, R_1, R_2,
                                    A, A_y, A_z, I_y, I_z, I_t, I_w, W_el_y, W_el_z, W_pl_y, W_pl_z);
        }
        public IGeometry Interpolate(IGeometry other, double t)
        {
            if (other is not SteelSection o)
                throw new InvalidOperationException("Interpolation only allowed with SteelSection");

            return new SteelSection(
                ProfileType,
                $"{Name}-{o.Name}-interp",
                H + (o.H - H) * t,
                B + (o.B - B) * t,
                T_w + (o.T_w - T_w) * t,
                T_f + (o.T_f - T_f) * t,
                R_1 + (o.R_1 - R_1) * t,
                R_2 + (o.R_2 - R_2) * t,
                A + (o.A - A) * t,
                A_y + (o.A_y - A_y) * t,
                A_z + (o.A_z - A_z) * t,
                I_y + (o.I_y - I_y) * t,
                I_z + (o.I_z - I_z) * t,
                I_t + (o.I_t - I_t) * t,
                I_w + (o.I_w - I_w) * t,
                W_el_y + (o.W_el_y - W_el_y) * t,
                W_el_z + (o.W_el_z - W_el_z) * t,
                W_pl_y + (o.W_pl_y - W_pl_y) * t,
                W_pl_z + (o.W_pl_z - W_pl_z) * t
            );
        }

        public override string ToString() => $"SteelSection(Name={Name})";

        /// <summary>
        /// Loads steel sections from CSV. Expects header: type,name,h,b,tw,tf,r1,r2,a,a_y,a_z,i_y,i_z,i_t,...
        /// </summary>
        public static IReadOnlyList<SteelSection> LoadFromCsv(string csvPath)
        {
            var list = new List<SteelSection>();
            var resolved = ResolveCsvPath(csvPath);
            // Open with shared read to avoid file lock issues in test runners
            using (var fs = new System.IO.FileStream(resolved, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            using (var sr = new System.IO.StreamReader(fs))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                        continue;
                    var parts = trimmed.Split(',');
                    if (parts.Length < 14) continue;
                    try
                    {
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
                        double iw = parts.Length > 14 ? double.Parse(parts[14], CultureInfo.InvariantCulture) : 0.0;
                        double w_el_y = parts.Length > 15 ? double.Parse(parts[15], CultureInfo.InvariantCulture) : 0.0;
                        double w_el_z = parts.Length > 16 ? double.Parse(parts[16], CultureInfo.InvariantCulture) : 0.0;
                        double w_pl_y = parts.Length > 17 ? double.Parse(parts[17], CultureInfo.InvariantCulture) : 0.0;
                        double w_pl_z = parts.Length > 18 ? double.Parse(parts[18], CultureInfo.InvariantCulture) : 0.0;
                        list.Add(new SteelSection(type, name, h, b, tw, tf, r1, r2, a, ay, az, iy, iz, it, iw, w_el_y, w_el_z, w_pl_y, w_pl_z));
                    }
                    catch
                    {
                        // ignore malformed
                    }
                }
            }
            return list;
        }

        private static string ResolveCsvPath(string csvPath)
        {
            if (string.IsNullOrWhiteSpace(csvPath)) csvPath = CsvPath;
            // If path exists as given, return
            if (File.Exists(csvPath)) return csvPath;

            // If it's a simple filename, search upward from the test/runtime base dir for a data/ folder
            var fileName = Path.GetFileName(csvPath);
            var dir = new DirectoryInfo(AppContext.BaseDirectory!);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "data", fileName);
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }

            // As a last resort, try CsvPath property location relative to base dir
            var candidate2 = Path.Combine(AppContext.BaseDirectory ?? ".", CsvPath);
            if (File.Exists(candidate2)) return candidate2;

            throw new FileNotFoundException(csvPath);
        }

        // Chemin CSV global paramétrable pour les sections acier
        public static string CsvPath { get; set; } = "data/steel_section.csv";

        /// <summary>
        /// Trouve une section par type de profil et nom, en utilisant CsvPath par défaut si non fourni.
        /// </summary>
        public static SteelSection GetByTypeAndName(string profileType, string name)
        {
            var path = CsvPath;
            var list = LoadFromCsv(path);
            var sec = list.FirstOrDefault(s => string.Equals(s.ProfileType, profileType, StringComparison.OrdinalIgnoreCase)
                                            && string.Equals(s.Name, name, StringComparison.OrdinalIgnoreCase));
            if (sec == null) throw new KeyNotFoundException($"SteelSection not found: {profileType}, {name}");
            return sec;
        }
    }
}
