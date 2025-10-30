namespace ofel.Core
{
    /// <summary>
    /// Représente un point 3D avec un identifiant.
    /// </summary>
    public sealed class Point
    {
    public double X { get; }
    public double Y { get; }
    public double Z { get; }

        public Point(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Computes the Euclidean distance to another point.
        /// </summary>
        public double DistanceTo(Point other)
        {
            if (other is null) throw new System.ArgumentNullException(nameof(other));
            double dx = other.X - this.X;
            double dy = other.Y - this.Y;
            double dz = other.Z - this.Z;
            return (double)System.Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }
    }
}
