namespace Ofel.Infrastructure.IO;

public static class OfelPaths
{
    public static string DataDirectory
    {
        get
        {
            var dir = new DirectoryInfo(AppContext.BaseDirectory);

            while (dir != null)
            {
                var candidate = Path.Combine(dir.FullName, "data");
                if (Directory.Exists(candidate))
                    return candidate;

                dir = dir.Parent;
            }

            throw new DirectoryNotFoundException("Cannot find 'data' directory.");
        }
    }

    public static string DbPath =>
        Path.Combine(DataDirectory, "ofel.db");

    public static string SteelMaterialCsv =>
        Path.Combine(DataDirectory, "steel_material.csv");

    public static string SteelSectionCsv =>
        Path.Combine(DataDirectory, "steel_section.csv");
}
