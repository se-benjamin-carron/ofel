using System;

namespace ofel.Core
{
    /// <summary>
    /// Base class for materials.
    /// </summary>
    public abstract class Material
    {
        public string Name { get; protected set; }

        public double E { get; }
        public double G { get; }
        public double Rho { get; }
        public double Alpha { get; }

        protected Material(string name, double e, double g, double rho, double alpha)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            E = e;
            G = g;
            Rho = rho;
            Alpha = alpha;
        }
    }
}