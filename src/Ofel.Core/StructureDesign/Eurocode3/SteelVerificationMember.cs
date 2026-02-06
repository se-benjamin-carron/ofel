using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public class ResultEurocode3Format
    {
        public Dictionary<string, SteelSectionVerificationULS> Uls { get; set; }
        public Dictionary<string, SteelSectionVerificationULS> UlsAcc { get; set; }
        public Dictionary<string, SteelSectionVerificationULS> SlsCar { get; set; }

        public ResultEurocode3Format()
        {
            Uls = new Dictionary<string, SteelSectionVerificationULS>();
            UlsAcc = new Dictionary<string, SteelSectionVerificationULS>();
            SlsCar = new Dictionary<string, SteelSectionVerificationULS>();
        }

        public ResultEurocode3Format(Dictionary<string, SteelSectionVerificationULS> uls,
            Dictionary<string, SteelSectionVerificationULS> ulsAcc,
            Dictionary<string, SteelSectionVerificationULS> sls)
        {
            Uls = uls;
            UlsAcc = ulsAcc;
            SlsCar = sls;
        }
    }

    public class SteelVerificationMemberContext
    {
        public required List<IntervalBuckling> BucklingY { get; set; }
        public required List<IntervalBuckling> BucklingZ { get; set; }
        public required List<IntervalBuckling> BucklingTorsion { get; set; }
        public required List<IntervalLateralBuckling> LateralBucklingUp { get; set; }
        public required List<IntervalLateralBuckling> LateralBucklingDown { get; set; }
        public required StabilisingForceType IsStabilisingForce { get; set; } = StabilisingForceType.Neutral;
        public required LateralBucklingType LateralBucklingType { get; set; } = LateralBucklingType.Default;
        public required InteractionInstabilitiesType interactionInstabilityType { get; set; } = InteractionInstabilitiesType.AnnexA;

    }

    public class SteelVerificationMember
    {
        public required ResultEurocode3Format Results { get; set; }

        public required SteelVerificationMemberContext Context { get; set; }

        private static List<T> ExtractInterval<T>(
            IEnumerable<T> source,
            double epsilon) where T : IInterval
        {
            return source.Where(i => i.Contains(epsilon)).ToList();
        }
        static T Pick<T>(List<T> list, int index)
        {
            if (list == null || list.Count == 0)
                throw new InvalidOperationException("La liste est vide");

            return list.Count > index ? list[index] : list[0];
        }

        private SteelVerificationInstabilitiesContext BuildContext(
            int index,
            List<IntervalBuckling> by,
            List<IntervalBuckling> bz,
            List<IntervalBuckling> bt,
            List<IntervalLateralBuckling> lt,
            List<FlexionCoefficient> c1c2,
            List<ShapeCoefficient> cmi0)
        {
            return new SteelVerificationInstabilitiesContext
            {
                IntervalBY = Pick(by, index),
                IntervalBZ = Pick(bz, index),
                IntervalBT = Pick(bt, index),
                IntervalLT = Pick(lt, index),
                CoefficientC1C2 = Pick(c1c2, index),
                Cmi0 = Pick(cmi0, index),
                IsStabilisingForce = Context.IsStabilisingForce,
                LateralBucklingType = Context.LateralBucklingType,
                InteractionType = Context.interactionInstabilityType
            };
        }

        public void ComputeUlsInternal(Dictionary<string, CombinationsData> results, List<MainEpsilon> allEpsilons, SteelResistanceCoefficient coefMember)
        {
            List<double> allEpsilon = allEpsilons.Select(e => e.Epsilon).ToList();
            var C1C2 = new List<FlexionCoefficient> { FlexionCoefficient.CreateFromLinearShape(1.0) };
            var Cmi0 = new List<ShapeCoefficient> { ShapeCoefficient.CreateDefault() };
            foreach (string key in results.Keys)
            {
                CombinationsData combination = results[key];
                for (int i = 0; i < combination.InternalEffortsCase.N.Length; i++)
                {
                    SteelSection? steelSection = allEpsilons[i].Geometry as SteelSection;
                    SteelMaterial? steelMaterial = allEpsilons[i].Material as SteelMaterial;
                    double epsilon = allEpsilon[i];
                    var effortOnPoint = new ForceValue(
                        combination.InternalEffortsCase.N[i],
                        combination.InternalEffortsCase.Vy[i],
                        combination.InternalEffortsCase.Vz[i],
                        combination.InternalEffortsCase.Mx[i],
                        combination.InternalEffortsCase.My[i],
                        combination.InternalEffortsCase.Mz[i]
                    );
                    SteelVerificationContext pureContext = new SteelVerificationContext(effort: effortOnPoint, section: steelSection, material: steelMaterial, coef: coefMember);

                    var contextInstabilities = GetInstabilitiesContextEpsilon(epsilon, effortOnPoint.My, C1C2, Cmi0);

                    var verification1 = new SteelSectionVerificationULS(pureContext, contextInstabilities[0]);
                    if (contextInstabilities.Count == 2)
                    {
                        var verification2 = new SteelSectionVerificationULS(pureContext, contextInstabilities[1]);
                        Results.Uls[key] = verification1.Ratio > verification2.Ratio ? verification1 : verification2;
                    }
                    else
                    {
                        Results.Uls[key] = verification1;
                    }
                }
            }
        }
        public List<SteelVerificationInstabilitiesContext> GetInstabilitiesContextEpsilon(double epsilon, double momentY, List<FlexionCoefficient> C1C2, List<ShapeCoefficient> Cmi0)
        {
            List<IntervalBuckling> intervalBy = ExtractInterval(Context.BucklingY, epsilon);
            List<IntervalBuckling> intervalBz = ExtractInterval(Context.BucklingZ, epsilon);
            List<IntervalBuckling> intervalBt = ExtractInterval(Context.BucklingTorsion, epsilon);
            List<IntervalLateralBuckling> intervalLatUp = ExtractInterval(Context.LateralBucklingUp, epsilon);
            List<IntervalLateralBuckling> intervalLatDown = ExtractInterval(Context.LateralBucklingDown, epsilon);
            List<IntervalLateralBuckling> intervalLat = momentY > 0 ? intervalLatUp : intervalLatDown;
            bool hasTwoCases = new IEnumerable<IInterval>[]
            {
                intervalBy,
                intervalBz,
                intervalBt,
                intervalLat
            }.Any(l => l.Count() == 2);
            if (hasTwoCases)
            {
                return new List<SteelVerificationInstabilitiesContext>
                {
                    BuildContext(0, intervalBy, intervalBz, intervalBt, intervalLat, C1C2, Cmi0),
                    BuildContext(1, intervalBy, intervalBz, intervalBt, intervalLat, C1C2, Cmi0),
                };
            }
            else
            {
                return new List<SteelVerificationInstabilitiesContext>
                {
                    BuildContext(0, intervalBy, intervalBz, intervalBt, intervalLat, C1C2, Cmi0),
                };
            }

        }


    }
}
