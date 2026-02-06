using System;

namespace Ofel.Core.SectionParameter
{
    /// <summary>
    /// Base class for materials.
    /// </summary>
    public interface IMaterial
    {
        string Name { get; }

        double E { get; }
        double G { get; }
        double Rho { get; }
        double Alpha { get; }

        // Retourne une copie du matériau
        IMaterial Clone();
    }
}
