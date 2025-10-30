using System;
using System.Collections.Generic;
using System.IO;

namespace ofel.Core
{
    /// <summary>
    /// Steel material definition loaded from steel_material.csv.
    /// </summary>
    public sealed class SteelMaterial : Material
    {
        public string Standard { get; }
        public double Fy { get; }
        public double Fu { get; }

        public SteelMaterial(string name, string standard, double fy, double fu, double e, double g, double rho, double alpha)
            : base(name, e, g, rho, alpha)
        {
            Standard = standard;
            Fy = fy;
            Fu = fu;
        }

        /// <summary>
        /// Loads steel materials from a CSV file. Expects header: Name,Standard,fy,fu,E,G,rho
        /// </summary>
        public static IReadOnlyList<SteelMaterial> LoadFromCsv(string csvPath)
        {
            var materials = new List<SteelMaterial>();
            foreach (var line in File.ReadLines(csvPath))
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
                // alpha (thermal expansion) is optional in older CSVs; default to 0.0 if missing
                double alpha = 0.0;
                if (parts.Length > 7) double.TryParse(parts[7], out alpha);
                materials.Add(new SteelMaterial(name, standard, fy, fu, e, g, rho, alpha));
            }
            return materials;
        }

        /// <summary>
        /// Retrieves a material by name and standard from the CSV file.
        /// </summary>
        public static SteelMaterial GetByNameAndStandard(string name, string standard, string csvPath="data/steel_material.csv")
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