using System.ComponentModel.DataAnnotations;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ofel.Core.Utils;
using Ofel.MatrixCalc;

namespace Ofel.Core
{
    public enum ForceLoadingTime
    {
        Instantaneous,
        ShortTerm,
        MiddleTerm,
        LongTerm,
        Permanent
    }
    public enum ForceKind
    {
        OwnWeight,
        PermanentLoad,
        Wind,
        Snow,
        Accidental,
        LiveLoad,
        Thermal,
        Collision,
        Seismic,
        Default
    }
    public enum ForceShape
    {
        LocalPunctualForce,
        GlobalPunctualForce,
        LocalLinearForce,
        GlobalLinearForce,
        InternalEffort,
    }

    /// <summary>
    /// Represents a 6-component force (Fx,Fy,Fz,Mx,My,Mz) at a position.
    /// </summary>
    public sealed class ForceValue
    {
        public double Fx { get; }
        public double Fy { get; }
        public double Fz { get; }
        public double Mx { get; }
        public double My { get; }
        public double Mz { get; }

        public ForceValue(double fx, double fy, double fz, double mx = 0.0, double my = 0.0, double mz = 0.0)
        {
            Fx = fx;
            Fy = fy;
            Fz = fz;
            Mx = mx;
            My = my;
            Mz = mz;
        }

        public Vector<double> GetVector()
        {
            return Vector<double>.Build.Dense(new double[] { Fx, Fy, Fz, Mx, My, Mz });
        }
        public bool Equals(ForceValue other, double threshold = 1e-6)
        {
            if (other is null) return false;

            return Math.Abs(Fx - other.Fx) < threshold
                && Math.Abs(Fy - other.Fy) < threshold
                && Math.Abs(Fz - other.Fz) < threshold
                && Math.Abs(Mx - other.Mx) < threshold
                && Math.Abs(My - other.My) < threshold
                && Math.Abs(Mz - other.Mz) < threshold;
        }


    }
    public abstract class Force
    {
        public ForceKind Kind { get; protected set; }
        public ForceShape Shape { get; protected set; }
        public int KindCase { get; protected set; }

        public List<(double Epsilon, ForceValue Value)> ForcesByEpsilon { get; protected set; }
            = new();

        public Vector<double> GlobalForce { get; protected set; }
            = Vector<double>.Build.Dense(0);

        protected Force(ForceShape shape)
        {
            Shape = shape;
            Kind = ForceKind.Default;
            KindCase = 0;
        }

        public List<MainEpsilon> ToMainEpsilon()
            => ForcesByEpsilon
                .Select(f => new MainEpsilon(f.Epsilon, KindMainEpsilon.Force))
                .ToList();

        public virtual Vector<double> GetGlobalForceVector(List<IntervalData> intervals)
        {
            int n = intervals?.Count ?? 0;
            return Vector<double>.Build.Dense(6 * n, 0.0);
        }

        public static (ForceKind, int) StringToForceId(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
                throw new ArgumentException("Input vide");

            var parts = input.Split('_', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length != 2)
                throw new ArgumentException($"Format invalide : {input}");

            if (!_forceMap.TryGetValue(parts[0], out var kind))
                throw new ArgumentException($"Type de force inconnu : {parts[0]}");

            if (!int.TryParse(parts[1], out int id))
                throw new ArgumentException($"Indice invalide : {parts[1]}");

            return (kind, id);
        }

        private static readonly Dictionary<string, ForceKind> _forceMap =
    new(StringComparer.OrdinalIgnoreCase)
    {
        ["W"] = ForceKind.Wind,
        ["G"] = ForceKind.PermanentLoad,
        ["Q"] = ForceKind.LiveLoad,
        ["S"] = ForceKind.Snow,
        ["E"] = ForceKind.Seismic,
        ["T"] = ForceKind.Thermal,
        ["PP"] = ForceKind.OwnWeight,
        ["C"] = ForceKind.Collision
    };

        public abstract Force Clone();
    }

    /// <summary>
    /// Base abstraction for forces applied to a Member.
    /// ForceKind should be a short string identifying the subtype (e.g. "global-linear").
    /// Each force can provide its local force vector given rotation and epsilon data.
    /// </summary>
    public abstract class Force<TSelf> : Force where TSelf : Force<TSelf>
    {

        protected abstract TSelf CloneTyped();

        public sealed override Force Clone()
            => CloneTyped();
        protected Force(ForceShape shape) : base(shape)
        {
            Kind = ForceKind.Default;
            KindCase = 0;
            ForcesByEpsilon = new List<(double Epsilon, ForceValue Value)>();
            GlobalForce = Vector<double>.Build.Dense(0);
        }
        public Vector<double> GetForceAtEpsilon(double epsilon, bool isRight)
        {
            // Linear interpolation between the two nearest defined forces
            if (ForcesByEpsilon.Count == 0)
                return Vector<double>.Build.Dense(6, 0.0);

            var sortedForces = ForcesByEpsilon.OrderBy(f => f.Epsilon).ToList();

            if (epsilon < sortedForces.First().Epsilon)
            {
                var f = sortedForces.First().Value;
                return Vector<double>.Build.Dense(new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });
            }
            if (epsilon > sortedForces.Last().Epsilon)
            {
                var f = sortedForces.Last().Value;
                return Vector<double>.Build.Dense(new double[] { 0.0, 0.0, 0.0, 0.0, 0.0, 0.0 });
            }

            for (int i = 0; i < sortedForces.Count - 1; i++)
            {
                var left = sortedForces[i];
                var right = sortedForces[i + 1];
                if (epsilon >= left.Epsilon && epsilon <= right.Epsilon)
                {
                    double t = (epsilon - left.Epsilon) / (right.Epsilon - left.Epsilon);
                    var fLeft = left.Value;
                    var fRight = right.Value;
                    if (!isRight)
                    {
                        return Vector<double>.Build.Dense(new double[]
                        {
                            t * fRight.Fx + (1 - t) * fLeft.Fx,
                            t * fRight.Fy + (1 - t) * fLeft.Fy,
                            t * fRight.Fz + (1 - t) * fLeft.Fz,
                            t * fRight.Mx + (1 - t) * fLeft.Mx,
                            t * fRight.My + (1 - t) * fLeft.My,
                            t * fRight.Mz + (1 - t) * fLeft.Mz
                        });
                    }
                    else
                    {
                        return Vector<double>.Build.Dense(new double[]
                        {
                            (1 - t) * fLeft.Fx + t * fRight.Fx,
                            (1 - t) * fLeft.Fy + t * fRight.Fy,
                            (1 - t) * fLeft.Fz + t * fRight.Fz,
                            (1 - t) * fLeft.Mx + t * fRight.Mx,
                            (1 - t) * fLeft.My + t * fRight.My,
                            (1 - t) * fLeft.Mz + t * fRight.Mz
                        });
                    }

                }
            }
            return Vector<double>.Build.Dense(6, 0.0); // Should not reach here
        }

        protected Vector<double>[] GetLocalVector(Vector<double> forceLeft, Vector<double> forceRight, double length)
        {
            double uniformXload = forceLeft[0];
            double linearXLoad = forceRight[0] - forceLeft[0];
            double uniformYLoad = forceLeft[1];
            double linearYLoad = forceRight[1] - forceLeft[1];
            double uniformZLoad = forceLeft[2];
            double linearZLoad = forceRight[2] - forceLeft[2];
            // Each helper returns an array [leftVector, rightVector].
            var ux = GetUniformlyDistributedForce(uniformXload, length, "X");
            var lx = GetLinearDistributedForce(linearXLoad, length, "X");
            var uy = GetUniformlyDistributedForce(uniformYLoad, length, "Y");
            var ly = GetLinearDistributedForce(linearYLoad, length, "Y");
            var uz = GetUniformlyDistributedForce(uniformZLoad, length, "Z");
            var lz = GetLinearDistributedForce(linearZLoad, length, "Z");

            var left = Vector<double>.Build.Dense(6, 0.0);
            var right = Vector<double>.Build.Dense(6, 0.0);

            // accumulate X contributions
            left += ux[0]; right += ux[1];
            left += lx[0]; right += lx[1];

            // accumulate Y contributions
            left += uy[0]; right += uy[1];
            left += ly[0]; right += ly[1];

            // accumulate Z contributions
            left += uz[0]; right += uz[1];
            left += lz[0]; right += lz[1];

            return new Vector<double>[] { left, right };
        }

        protected Vector<double>[] GetUniformlyDistributedForce(double forcePerLength, double length, string axis)
        {
            if (axis == "X")
            {
                Vector<double> forceX = Vector<double>.Build.Dense(new double[]
                {
                    forcePerLength * length/2,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                });
                return new Vector<double>[] { forceX, forceX };
            }
            else if (axis == "Y")
            {
                Vector<double> forceYLeft = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    forcePerLength * length/2,
                    0.0,
                    0.0,
                    0.0,
                    forcePerLength * length * length / 12
                });
                Vector<double> forceYRight = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    forcePerLength * length/2,
                    0.0,
                    0.0,
                    0.0,
                    -forcePerLength * length * length / 12
                });
                return new Vector<double>[] { forceYLeft, forceYRight };
            }
            else if (axis == "Z")
            {
                Vector<double> forceZLeft = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    0.0,
                    forcePerLength * length/2,
                    0.0,
                    forcePerLength * length * length / 12,
                    0.0
                });
                Vector<double> forceZRight = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    0.0,
                    forcePerLength * length/2,
                    0.0,
                    -forcePerLength * length * length / 12,
                    0.0
                });
                return new Vector<double>[] { forceZLeft, forceZRight };
            }
            Vector<double> defaultVector = Vector<double>.Build.Dense(6, 0.0);
            return new Vector<double>[] { defaultVector, defaultVector };
        }

        protected Vector<double>[] GetLinearDistributedForce(double forcePerLengthRight, double length, string axis)
        {
            if (axis == "X")
            {
                Vector<double> forceX = Vector<double>.Build.Dense(new double[]
                {
                    forcePerLengthRight * length/4,
                    0.0,
                    0.0,
                    0.0,
                    0.0,
                    0.0
                });
                return new Vector<double>[] { forceX, forceX };
            }
            else if (axis == "Y")
            {
                Vector<double> forceYLeft = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    3*forcePerLengthRight * length/20,
                    0.0,
                    0.0,
                    0.0,
                    forcePerLengthRight * length * length / 30
                });
                Vector<double> forceYRight = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    7*forcePerLengthRight * length/20,
                    0.0,
                    0.0,
                    0.0,
                    -forcePerLengthRight * length * length / 20
                });
                return new Vector<double>[] { forceYLeft, forceYRight };
            }
            else if (axis == "Z")
            {
                Vector<double> forceZLeft = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    0.0,
                    3*forcePerLengthRight * length/20,
                    0.0,
                    forcePerLengthRight * length * length / 30,
                    0.0

                });
                Vector<double> forceZRight = Vector<double>.Build.Dense(new double[]
                {
                    0.0,
                    0.0,
                    7*forcePerLengthRight * length/20,
                    0.0,
                    -forcePerLengthRight * length * length / 20,
                    0.0
                });
                return new Vector<double>[] { forceZLeft, forceZRight };
            }
            Vector<double> defaultVector = Vector<double>.Build.Dense(6, 0.0);
            return new Vector<double>[] { defaultVector, defaultVector };
        }

    }
    public class GlobalLinearForce : Force<GlobalLinearForce>
    {
        // Dictionary mapping epsilon position -> full 6-component force value at that section
        // The key is the epsilon (double) and the value contains Fx,Fy,Fz,Mx,My,Mz

        public GlobalLinearForce(ForceKind kind, int load_case, List<(double Epsilon, ForceValue Value)> distributedForcesByEpsilon)
            : base(ForceShape.GlobalLinearForce)
        {
            Kind = kind;
            KindCase = load_case;
            ForcesByEpsilon = distributedForcesByEpsilon ?? new List<(double, ForceValue)>();
        }

        public override Vector<double> GetGlobalForceVector(List<IntervalData> intervals)
        {
            Vector<double> result = Vector<double>.Build.Dense(6 * (intervals.Count + 1), 0.0);
            for (int i = 0; i < intervals.Count; i++)
            {
                double lengthInterval = intervals[i].Point1.DistanceTo(intervals[i].Point2);
                Vector<double> forceLeftGlobal = GetForceAtEpsilon(intervals[i].Epsilon1, false);
                Vector<double> forceRightGlobal = GetForceAtEpsilon(intervals[i].Epsilon2, true);
                var rotationMatrix6x6 = intervals[i].RotationMatrix.SubMatrix(0, 6, 0, 6);
                // Apply the forces to the result vector
                Vector<double> forceLeftLocal = RotationMatrixClass.SwitchVectorFromGlobalToLocal(rotationMatrix6x6, forceLeftGlobal);
                Vector<double> forceRightLocal = RotationMatrixClass.SwitchVectorFromGlobalToLocal(rotationMatrix6x6, forceRightGlobal);
                var intervalLocalPair = GetLocalVector(forceLeftLocal, forceRightLocal, lengthInterval);
                // combine left and right equivalent nodal contributions into a single local vector
                Vector<double> intervalGlobalVectorLeft = RotationMatrixClass.SwitchVectorFromLocalToGlobal(
                    rotationMatrix6x6, intervalLocalPair[0]);
                Vector<double> intervalGlobalVectorRight = RotationMatrixClass.SwitchVectorFromLocalToGlobal(
                    rotationMatrix6x6, intervalLocalPair[1]);
                int idxLeft = 6 * i;
                int idxRight = 6 * (i + 1);

                result.SetSubVector(idxLeft, 6, result.SubVector(idxLeft, 6) + intervalGlobalVectorLeft);
                result.SetSubVector(idxRight, 6, result.SubVector(idxRight, 6) + intervalGlobalVectorRight);
            }
            return result;
        }
        protected override GlobalLinearForce CloneTyped()
        {
            return new GlobalLinearForce(Kind, KindCase, ForcesByEpsilon);
        }
    }

    public class GlobalPunctualForce : Force<GlobalPunctualForce>
    {
        public GlobalPunctualForce(ForceKind kind, int load_case, List<(double Epsilon, ForceValue Value)> distributedForcesByEpsilon)
            : base(ForceShape.GlobalPunctualForce)
        {
            Kind = kind;
            KindCase = load_case;
            ForcesByEpsilon = distributedForcesByEpsilon ?? new List<(double, ForceValue)>();
        }
        public override Vector<double> GetGlobalForceVector(List<IntervalData> intervals)
        {
            if (intervals == null)
                return Vector<double>.Build.Dense(0);

            int nodes = intervals.Count + 1; // n intervals -> n+1 nodes
            Vector<double> result = Vector<double>.Build.Dense(6 * nodes, 0.0);

            if (ForcesByEpsilon == null || ForcesByEpsilon.Count == 0)
                return result;

            // Formalism: punctual force corresponds to a single force applied at one epsilon
            var punctual = ForcesByEpsilon[0];
            const double tol = 1e-9;

            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].Epsilon1 == intervals[i].Epsilon2)
                {
                    continue; // skip zero-length intervals
                }

                if (System.Math.Abs(punctual.Epsilon - intervals[i].Epsilon1) <= tol)
                {
                    double coef = intervals[i].Epsilon1 == 0.0 ? 1.0 : 2.0;
                    result.SetSubVector(6 * i, 6, punctual.Value.GetVector() / coef);
                }

                if (System.Math.Abs(punctual.Epsilon - intervals[i].Epsilon2) <= tol)
                {
                    double coef = intervals[i].Epsilon2 == 1.0 ? 1.0 : 2.0;
                    result.SetSubVector(6 * (i + 1), 6, punctual.Value.GetVector() / coef);
                }
            }

            return result;
        }
        protected override GlobalPunctualForce CloneTyped()
        {
            return new GlobalPunctualForce(Kind, KindCase, ForcesByEpsilon);
        }
    }

    public class LocalLinearForce : Force<LocalLinearForce>
    {
        public LocalLinearForce(ForceKind kind, int load_case, List<(double Epsilon, ForceValue Value)> distributedForcesByEpsilon)
            : base(ForceShape.LocalLinearForce)
        {
            Kind = kind;
            KindCase = load_case;
            ForcesByEpsilon = distributedForcesByEpsilon;
        }
        public override Vector<double> GetGlobalForceVector(List<IntervalData> intervals)
        {
            Vector<double> result = Vector<double>.Build.Dense(6 * (intervals.Count + 1), 0.0);
            for (int i = 0; i < intervals.Count; i++)
            {
                double lengthInterval = intervals[i].Point1.DistanceTo(intervals[i].Point2);
                Vector<double> forceLeftLocal = GetForceAtEpsilon(intervals[i].Epsilon1, false);
                Vector<double> forceRightLocal = GetForceAtEpsilon(intervals[i].Epsilon2, true);
                // Apply the forces to the result vector
                var rotationMatrix6x6 = intervals[i].RotationMatrix.SubMatrix(0, 6, 0, 6);
                var intervalLocalPair = GetLocalVector(forceLeftLocal, forceRightLocal, lengthInterval);
                Vector<double> intervalGlobalVectorLeft = RotationMatrixClass.SwitchVectorFromLocalToGlobal(
                    rotationMatrix6x6, intervalLocalPair[0]);
                Vector<double> intervalGlobalVectorRight = RotationMatrixClass.SwitchVectorFromLocalToGlobal(
                    rotationMatrix6x6, intervalLocalPair[1]);
                int idxLeft = 6 * i;
                int idxRight = 6 * (i + 1);

                result.SetSubVector(idxLeft, 6, result.SubVector(idxLeft, 6) + intervalGlobalVectorLeft);
                result.SetSubVector(idxRight, 6, result.SubVector(idxRight, 6) + intervalGlobalVectorRight);
            }
            return result;
        }

        protected override LocalLinearForce CloneTyped()
        {
            return new LocalLinearForce(Kind, KindCase, ForcesByEpsilon);
        }

    }
    public class LocalPunctualForce : Force<LocalPunctualForce>
    {
        // Remplacer 'LocalPosition' par 'Epsilon' pour rester cohérent
        public LocalPunctualForce(ForceKind kind, int load_case, List<(double Epsilon, ForceValue Value)> distributedForcesByEpsilon)
            : base(ForceShape.LocalPunctualForce)
        {
            KindCase = load_case;
            Kind = kind;
            ForcesByEpsilon = distributedForcesByEpsilon;
        }

        public override Vector<double> GetGlobalForceVector(List<IntervalData> intervals)
        {
            if (intervals == null)
                return Vector<double>.Build.Dense(0);

            int nodes = intervals.Count + 1; // n intervals -> n+1 nodes
            Vector<double> result = Vector<double>.Build.Dense(6 * nodes, 0.0);

            if (ForcesByEpsilon == null || ForcesByEpsilon.Count == 0)
                return result;

            // Formalism: punctual force corresponds to a single force applied at one epsilon
            var punctual = ForcesByEpsilon[0];
            const double tol = 1e-9;

            for (int i = 0; i < intervals.Count; i++)
            {
                if (intervals[i].Epsilon1 == intervals[i].Epsilon2)
                {
                    continue; // skip zero-length intervals
                }

                if (System.Math.Abs(punctual.Epsilon - intervals[i].Epsilon1) <= tol)
                {
                    double coef = intervals[i].Epsilon1 == 0.0 ? 1.0 : 2.0;
                    var localForce = punctual.Value.GetVector();
                    var rotationMatrix6x6 = intervals[i].RotationMatrix.SubMatrix(0, 6, 0, 6);
                    var globalForce = RotationMatrixClass.SwitchVectorFromLocalToGlobal(rotationMatrix6x6, localForce);
                    result.SetSubVector(6 * i, 6, result.SubVector(6 * i, 6) + globalForce / coef);
                }

                if (System.Math.Abs(punctual.Epsilon - intervals[i].Epsilon2) <= tol)
                {
                    double coef = intervals[i].Epsilon2 == 1.0 ? 1.0 : 2.0;
                    var localForce = punctual.Value.GetVector();
                    var rotationMatrix6x6 = intervals[i].RotationMatrix.SubMatrix(0, 6, 0, 6);
                    var globalForce = RotationMatrixClass.SwitchVectorFromLocalToGlobal(rotationMatrix6x6, localForce);
                    result.SetSubVector(6 * i, 6, result.SubVector(6 * i, 6) + globalForce / coef);
                }
            }

            return result;
        }
        protected override LocalPunctualForce CloneTyped()
        {
            return new LocalPunctualForce(Kind, KindCase, ForcesByEpsilon);
        }
    }

    public class OwnWeight : Force<OwnWeight>
    {
        public OwnWeight()
            : base(ForceShape.InternalEffort)
        {
            KindCase = 1;
            Kind = ForceKind.OwnWeight;
        }
        public override Vector<double> GetGlobalForceVector(List<IntervalData> intervals)
        {
            Vector<double> result = Vector<double>.Build.Dense(6 * (intervals.Count + 1), 0.0);
            for (int i = 0; i < intervals.Count; i++)
            {
                double lengthInterval = intervals[i].Point1.DistanceTo(intervals[i].Point2);
                double massPoint = intervals[i].Geometry.A * intervals[i].Material.Rho * lengthInterval * 9.81 / 2;

                // Apply the forces to the result vector
                Vector<double> intervalGlobalVector = Vector<double>.Build.Dense(new double[]
                {
                            0.0,
                            0.0,
                            (double)(-massPoint),
                            0.0,
                            0.0,
                            0.0,
                            0.0,
                            0.0,
                            (double)(-massPoint),
                            0.0,
                            0.0,
                            0.0
                });
                int idx = 6 * i;

                result.SetSubVector(idx, 12, result.SubVector(idx, 12) + intervalGlobalVector);

            }
            return result;
        }

        protected override OwnWeight CloneTyped()
        {
            return new OwnWeight();
        }
    }
    public class Thermic : Force<Thermic>
    {
        public Thermic(double temperatureCase, double temperatureStart)
            : base(ForceShape.InternalEffort)
        {
            TemperatureStart = temperatureStart;
            TemperatureCase = temperatureCase;
            KindCase = 1;
            Kind = ForceKind.Thermal;

        }
        public double TemperatureStart { get; set; }
        public double TemperatureCase { get; set; }
        public override Vector<double> GetGlobalForceVector(List<IntervalData> intervals)
        {
            Vector<double> result = Vector<double>.Build.Dense(6 * (intervals.Count + 1), 0.0);
            for (int i = 0; i < intervals.Count; i++)
            {
                double lengthInterval = intervals[i].Point1.DistanceTo(intervals[i].Point2);
                double deltaT = TemperatureCase - TemperatureStart;
                double thermalForce = intervals[i].Material.E * intervals[i].Material.Alpha * deltaT / 2;

                // Apply the forces to the result vector
                Vector<double> intervalLocalVector = Vector<double>.Build.Dense(new double[]
                {
                            (double)(-thermalForce),
                            0.0,
                            0.0,
                            0.0,
                            0.0,
                            0.0,
                            (double)(-thermalForce),
                            0.0,
                            0.0,
                            0.0,
                            0.0,
                            0.0
                });
                Vector<double> intervalGlobalVector = RotationMatrixClass.SwitchVectorFromLocalToGlobal(intervals[i].RotationMatrix, intervalLocalVector);
                int idx = 6 * i;

                result.SetSubVector(idx, 12, result.SubVector(idx, 12) + intervalGlobalVector);

            }
            return result;
        }

        protected override Thermic CloneTyped()
        {
            return new Thermic(TemperatureCase, TemperatureStart);
        }
    }
}
