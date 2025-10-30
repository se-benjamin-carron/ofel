namespace ofel.Core
{
    // Placeholders minimalistes pour les types attendus
    public interface IGeometry
    {
        public string Name { get; }
        public double A { get; }
        public double A_y { get; }
        public double A_z { get; }
        public double I_y { get; }
        public double I_z { get; }
        public double I_t { get; }
        public double I_w { get; }
        string Type { get; }
    /// <summary>
    /// Interpolates this geometry with another geometry by ratio in [0,1].
    /// </summary>
    IGeometry Interpolate(IGeometry other, double t);
    }

    public interface IMaterial
    {
        string Name { get; }
    }
}
