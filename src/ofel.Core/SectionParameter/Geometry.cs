namespace Ofel.Core.SectionParameter
{
    // Placeholders minimalistes pour les types attendus
    public interface IGeometry
    {
        string Name { get; }
        double A { get; }
        double A_y { get; }
        double A_z { get; }
        double I_y { get; }
        double I_z { get; }
        double I_t { get; }
        double I_w { get; }
        string MaterialType { get; }

        /// <summary>
        /// Returns a copy of this geometry.
        /// </summary>
        IGeometry Clone();

        IGeometry Interpolate(IGeometry other, double t);
    }
}
