using System;
using System.Collections.Generic;
using System.IO;

namespace Ofel.Core.SectionParameter
{
    /// <summary>
    /// Steel material definition loaded from steel_material.csv.
    /// </summary>
    public class SteelMaterial : IMaterial
    {
        public string Name { get; }
        public double E { get; }
        public double G { get; }
        public double Rho { get; }
        public double Alpha { get; }

        // Propriétés spécifiques à l'acier
        public string Standard { get; }
        public double Fy { get; }
        public double Fu { get; }
        public SteelMaterial(string name, string standard, double fy, double fu,
                             double e, double g, double rho, double alpha = 12e-6)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Standard = standard;
            Fy = fy;
            Fu = fu;
            E = e;
            G = g;
            Rho = rho;
            Alpha = alpha;
        }
        public double GetEpsilon()
        {
            return Math.Sqrt(235_000_000 / Fy);
        }
        public IMaterial Clone()
        {
            // Retourne une nouvelle instance indépendante
            return new SteelMaterial(Name, Standard, Fy, Fu, E, G, Rho, Alpha);
        }

        /// <summary>
        /// Loads steel materials from a CSV file. Expects header: Name,Standard,fy,fu,E,G,rho
        /// </summary>
        public static IReadOnlyList<SteelMaterial> LoadFromCsv(string csvPath)
        {
            var materials = new List<SteelMaterial>();
            var resolved = ResolveCsvPath(csvPath);
            // Open with shared read so tests can read while other processes may have file handles
            using (var fs = new System.IO.FileStream(resolved, System.IO.FileMode.Open, System.IO.FileAccess.Read, System.IO.FileShare.ReadWrite))
            using (var sr = new System.IO.StreamReader(fs))
            {
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    var trimmed = line.Trim();
                    if (string.IsNullOrWhiteSpace(trimmed) || trimmed.StartsWith("Name", StringComparison.OrdinalIgnoreCase))
                        continue;
                    // split by comma, handle quoted strings
                    var parts = SplitCsvLine(trimmed);
                    if (parts.Length < 7)
                        continue;
                    var name = parts[0].Trim('"');
                    var standard = parts[1].Trim('"');
                    double fy = double.Parse(parts[2]);
                    double fu = double.Parse(parts[3]);
                    double e = double.Parse(parts[4]);
                    double g = double.Parse(parts[5]);
                    double rho = double.Parse(parts[6]);
                    materials.Add(new SteelMaterial(name, standard, fy, fu, e, g, rho));
                }
            }
            return materials;
        }

        // Configurable CSV path for steel materials
        public static string CsvPath { get; set; } = "data/steel_material.csv";

        private static string ResolveCsvPath(string csvPath)
        {
            if (string.IsNullOrWhiteSpace(csvPath)) csvPath = CsvPath;
            if (File.Exists(csvPath)) return csvPath;

            var fileName = Path.GetFileName(csvPath);
            var dir = new DirectoryInfo(AppContext.BaseDirectory!);
            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "data", fileName);
                if (File.Exists(candidate)) return candidate;
                dir = dir.Parent;
            }

            var candidate2 = Path.Combine(AppContext.BaseDirectory ?? ".", CsvPath);
            if (File.Exists(candidate2)) return candidate2;

            throw new FileNotFoundException(csvPath);
        }

        /// <summary>
        /// Retrieves a material by name and standard from the CSV file.
        /// </summary>
        public static SteelMaterial GetByNameAndStandard(string name, string standard, string csvPath = "data/steel_material.csv")
        {
            var materials = LoadFromCsv(csvPath);
            foreach (var mat in materials)
            {
                if (string.Equals(mat.Name, name, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(mat.Standard, standard, StringComparison.OrdinalIgnoreCase))
                {
                    return mat;
                }
            }
            throw new KeyNotFoundException($"SteelMaterial not found: {name}, {standard}");
        }

        private static string[] SplitCsvLine(string line)
        {
            var parts = new List<string>();
            bool inQuote = false;
            var current = new System.Text.StringBuilder();
            foreach (char c in line)
            {
                if (c == '"')
                {
                    inQuote = !inQuote;
                }
                else if (c == ',' && !inQuote)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
            parts.Add(current.ToString());
            return parts.ToArray();
        }
    }
}
