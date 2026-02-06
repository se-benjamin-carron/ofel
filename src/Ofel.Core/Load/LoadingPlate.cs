using MathNet.Numerics.LinearAlgebra;
using Ofel.Core;
using Ofel.Core.Data;
using Ofel.Core.FiniteElement.Structure;
using Ofel.Core.Load.Climatic;
using Ofel.Core.Utils;
using Ofel.MatrixCalc;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ofel.Core.SectionParameter;


namespace Ofel.Core.Load
{
    public class LoadingPlate
    {
        /// <summary>
        /// Positions of the corner of the plate in 3D space, ordered as follows:
        /// 0 - top-left
        /// 1 - top-right
        /// 2 - bottom-right
        /// 3 - bottom-left.
        /// </summary>
        public Point[] Corners { get; set; } = new Point[4];

        public double Angle { get; set; }
        public FiniteElementMember FeMember { get; set; }

        /// <summary>
        /// Purlin positions from 0 and 1 along the width of the plate (0 = top edge, 1 = bottom edge)
        /// </summary>
        public double[] PurlinsPositions { get; set; }
        /// <summary>
        /// Gantri Positions from 0 and 1 along the length of the plate (0 = left edge, 1 = right edge)
        /// </summary>
        public double[] GantriesPositions { get; set; }

        public List<List<(double, Force)>> AllForces { get; set; }

        public LoadingPlate(Point[] points, double[] purlinsPositions, double[] gantriesPositions, double mesh)
        {
            Corners = points;
            PurlinsPositions = purlinsPositions;
            GantriesPositions = gantriesPositions;
            AllForces = new List<List<(double, Force)>>();
            double Length = Corners[0].DistanceTo(Corners[3]);
            List<double> angle = IntervalData.ComputeAngles(Corners[0], Corners[3]);
            Angle = angle[0];
            FeMember = this.CreateFiniteElementMember();
            FeMember.Compute(mesh);
        }

        public void AddPermanentLoad(double q)
        {
            var permanentLoad = GetPermanentLoad(q);
            AllForces.AddRange(permanentLoad);
        }

        public void AddSnowLoad1Slope(SnowCharacteristics snowChar, SnowCoefficient snowCoef)
        {
            var snowLoad = GetLoadingSnow1Slope(snowChar, snowCoef, Math.Abs(Angle));
            AllForces.AddRange(snowLoad);
        }

        public void AddWindLoad1SlopeCTICM(WindCharacteristics[] windChar, WindCoefficient windCoef, double coefficientFrottement, double[] coefficientObstruction)
        {
            var windLoad = GetLoadingWind1Slope(windChar, windCoef, coefficientFrottement, coefficientObstruction);
            AllForces.AddRange(windLoad);
        }

        public List<Dictionary<(ForceKind, int), List<(double, ForceValue)>>> GetForcesOnPurlin(double mesh = 0.5)
        {
            var feStructure = new FiniteElementStructure(new List<FiniteElementMember> { FeMember });
            feStructure.Prepare(mesh);
            // For each Loading case / For each purlin / Each Position associated with the ForceValue
            List<Dictionary<(ForceKind, int), List<(double, ForceValue)>>> resultsPerPurlin = CreatePurlinUnloaded();
            for (int i = 0; i < AllForces.Count; i++)
            {
                var forceToApply = AllForces[i];
                (ForceKind, int) loadCase = (forceToApply[0].Item2.Kind, forceToApply[0].Item2.KindCase);
                foreach (var (epsilon, force) in forceToApply)
                {
                    var specificLoading = GetSpecificLoadingOnEachPurlin(feStructure, force);
                    AddWidthLoadingToPurlins(resultsPerPurlin, loadCase, specificLoading, epsilon);
                }
            }
            return resultsPerPurlin;
        }

        public void AddWidthLoadingToPurlins(
            List<Dictionary<(ForceKind, int), List<(double, ForceValue)>>> resultsPerPurlin,
            (ForceKind, int) loadCase,
            ForceValue[] specificLoading, double epsilon)
        {
            for (int i = 0; i < specificLoading.Length; i++)
            {
                if (resultsPerPurlin[i].ContainsKey(loadCase))
                {
                    resultsPerPurlin[i][loadCase].Add((epsilon, specificLoading[i]));
                }
                else
                {
                    resultsPerPurlin[i][loadCase] = new List<(double, ForceValue)> { (epsilon, specificLoading[i]) };
                }
            }
        }

        public List<Member> GetPurlinsMember(double mesh)
        {
            var allPurlins = new List<Member>();
            var effortsOnPurlins = GetForcesOnPurlin(mesh);
            var rotationMatrix = RotationMatrixClass.SphericalRotation6x6(Angle, 0, 0);
            //var test = Vector<double>.Build.Dense(new double[] { 4, 0, 4, 0, 0, 0 });
            //var result = RotationMatrixClass.SwitchVectorFromGlobalToLocal(rotationMatrix, test);
            for (int i = 0; i < PurlinsPositions.Length; i++)
            {
                double position = PurlinsPositions[i];
                var builder = new LoadingPlatePurlinBuilder();
                var allForces = new List<Force>();
                foreach (var (kind, load_case) in effortsOnPurlins[i].Keys)
                {
                    var distributedForcesByEpsilon = effortsOnPurlins[i][(kind, load_case)];
                    var rotated = new List<(double, ForceValue)>();

                    foreach (var (epsilon, force) in distributedForcesByEpsilon)
                    {
                        var rotatedForce =
                            RotationMatrixClass.SwitchVectorFromGlobalToLocal(rotationMatrix, force.GetVector());
                        ForceValue newForce = new ForceValue(rotatedForce[0], rotatedForce[1], rotatedForce[2], rotatedForce[3], rotatedForce[4], rotatedForce[5]);
                        rotated.Add((epsilon, newForce));
                    }
                    allForces.Add(new LocalLinearForce(kind, load_case, rotated));
                }
                var member = builder.Create(this, PurlinsPositions[i], allForces);
                allPurlins.Add(member);
            }
            return allPurlins;
        }

        public List<Dictionary<(ForceKind, int), List<(double, ForceValue)>>> CreatePurlinUnloaded()
        {
            int n = PurlinsPositions.Count(); // nombre de sous-listes que tu veux
            var allForces = new List<Dictionary<(ForceKind, int), List<(double, ForceValue)>>>(n);

            for (int i = 0; i < n; i++)
            {
                allForces.Add(new Dictionary<(ForceKind, int), List<(double, ForceValue)>>()); // chaque sous-liste commence vide
            }
            return allForces;
        }


        public FiniteElementMember CreateFiniteElementMember()
        {
            var builder = new LoadingPlateWidthMemberBuilder();
            double length = Corners[0].DistanceTo(Corners[3]);
            var widthMember = builder.Create(this, length, Angle);
            var feMember = new FiniteElementMember(widthMember);
            return feMember;
        }

        public List<(double, Force)> GetPermanentLoad(double q)
        {
            var load_q = new GlobalLinearForce(
                kind: ForceKind.PermanentLoad,
                load_case: 1,
                distributedForcesByEpsilon: new List<(double Epsilon, ForceValue Value)>
                {
                ( 0.0, new ForceValue(0, 0, -q)),
                ( 1.0, new ForceValue(0, 0, -q)) }
            );
            return new List<(double, Force)>
                    {
                        (0.0, load_q),
                        (1.0, load_q)
                    };
        }

        public List<List<(double, Force)>> GetLoadingSnow1Slope(SnowCharacteristics snowChar, SnowCoefficient snowCoef, double angle)
        {
            double s_acc = snowChar.AccidentelSnow;
            double mu1 = snowCoef.Mu1;
            double s_k = snowChar.AltitudeAddedSnow + snowChar.CharacteristicSnow;
            double c_e = snowChar.ExpositionCoefficient;
            double q_k = s_k * mu1 * c_e * Math.Cos(Calculator.ToRadians(angle));
            double q_acc = s_acc * mu1 * c_e * Math.Cos(Calculator.ToRadians(angle));
            var load_char = new GlobalLinearForce(
                kind: ForceKind.Snow,
                load_case: 1,
                distributedForcesByEpsilon: new List<(double Epsilon, ForceValue Value)>
                {
                ( 0.0, new ForceValue(0, 0, -q_k)),
                ( 1.0, new ForceValue(0, 0, -q_k)) }
            );

            var load_acc = new GlobalLinearForce(
                kind: ForceKind.Accidental,
                load_case: 1,
                distributedForcesByEpsilon: new List<(double Epsilon, ForceValue Value)>
                {
                ( 0.0, new ForceValue(0, 0, -q_acc)),
                ( 1.0, new ForceValue(0, 0, -q_acc)) }
            );
            return new List<List<(double, Force)>>
                {
                    new List<(double, Force)>
                    {
                        (0.0, load_char),
                        (1.0, load_char)
                    },
                    new List<(double, Force)>
                    {
                        (0.0, load_acc),
                        (1.0, load_acc)
                    }
                };
        }
        public List<List<(double, Force)>> GetLoadingWind1Slope(WindCharacteristics[] windChar, WindCoefficient windCoef, double coefficientFrottement, double[] coefficientObstruction)
        {
            var forceWindFrontAndTail = this.getWindFrontAndTail1Slope(windChar, windCoef);
            var forceWindSide = this.getWindSide1Slope(windChar, windCoef, coefficientObstruction, coefficientFrottement);
            return forceWindFrontAndTail.Concat(forceWindSide).ToList();
        }

        public List<List<(double, Force)>> getWindFrontAndTail1Slope(WindCharacteristics[] windChar, WindCoefficient windCoef)
        {
            List<List<(double, Force)>> forces = new List<List<(double, Force)>>();
            WindCoefficientCTICM liftingCoef = windCoef.CoefficientLifting;
            WindCoefficientCTICM collapsingCoef = windCoef.CoefficientCollasping;

            for (int i = 0; i < 2; i++)
            {
                double[] positions = (i == 0) ? new double[] { 0.0, 0.9, 0.9001, 1.0 } : new double[] { 0.0, 0.1, 0.1001, 1.0 };
                double[] coefficient_lifting = (i == 0) ? new double[] {
                    liftingCoef.Coefficient3,
                    liftingCoef.Coefficient2,
                    liftingCoef.Coefficient1,
                    liftingCoef.Coefficient1 } : new double[] {
                        liftingCoef.Coefficient1,
                        liftingCoef.Coefficient1,
                        liftingCoef.Coefficient2,
                        liftingCoef.Coefficient3 };
                double[] coefficient_collapsing = (i == 0) ? new double[] {
                    collapsingCoef.Coefficient3,
                    collapsingCoef.Coefficient2,
                    collapsingCoef.Coefficient1,
                    collapsingCoef.Coefficient1 } : new double[] {
                        collapsingCoef.Coefficient1,
                        collapsingCoef.Coefficient1,
                        collapsingCoef.Coefficient2,
                        collapsingCoef.Coefficient3 };

                double q_p = windChar[i].PeakPressure;
                LocalLinearForce w_collapse = new LocalLinearForce(
                    kind: ForceKind.Wind,
                    load_case: i + 1,
                    distributedForcesByEpsilon: new List<(double Epsilon, ForceValue Value)>
                    {
                        ( positions[0], new ForceValue(0, 0, coefficient_collapsing[0]*q_p) ),
                        ( positions[1], new ForceValue(0, 0, coefficient_collapsing[1]*q_p) ),
                        ( positions[2], new ForceValue(0, 0, coefficient_collapsing[2]*q_p) ),
                        ( positions[3], new ForceValue(0, 0, coefficient_collapsing[3]*q_p) )
                    }
                );
                LocalLinearForce w_lifting = new LocalLinearForce(
                    kind: ForceKind.Wind,
                    load_case: i + 2,
                    distributedForcesByEpsilon: new List<(double Epsilon, ForceValue Value)>
                    {
                        ( positions[0], new ForceValue(0, 0, coefficient_lifting[0]*q_p) ),
                        ( positions[1], new ForceValue(0, 0, coefficient_lifting[1]*q_p) ),
                        ( positions[2], new ForceValue(0, 0, coefficient_lifting[2]*q_p) ),
                        ( positions[3], new ForceValue(0, 0, coefficient_lifting[3]*q_p) )
                    }
                );

                List<List<(double, Force)>> all_w = new List<List<(double, Force)>>
                {
                    new List<(double, Force)>
                    {
                        (0.0, w_collapse),
                        (1.0, w_collapse)
                    },
                    new List<(double, Force)>
                    {
                        (0.0, w_lifting),
                        (1.0, w_lifting)
                    },
                };

                forces.AddRange(all_w);
            }
            return forces;
        }
        public List<List<(double, Force)>> getWindSide1Slope(WindCharacteristics[] windChar, WindCoefficient windCoef, double[] coefficientObstruction, double coefficientFriction)
        {
            List<List<(double, Force)>> forces = new List<List<(double, Force)>>();
            WindCoefficientCTICM liftingCoef = windCoef.CoefficientFrictionLifting;
            WindCoefficientCTICM collapsingCoef = windCoef.CoefficientFrictionCollapsing;

            for (int i = 0; i < 2; i++)
            {
                int friction_coef = (i == 0) ? 1 : -1;
                double q_p = windChar[i + 2].PeakPressure;
                double coefficient_lifting = liftingCoef.CoefficientF;

                double coefficient_collapsing = collapsingCoef.CoefficientF;

                LocalLinearForce w_collapse = new LocalLinearForce(
                    kind: ForceKind.Wind,
                    load_case: 2 * (i + 1) + 1,
                    distributedForcesByEpsilon: new List<(double Epsilon, ForceValue Value)>
                    {
                        ( 0.0, new ForceValue(0.0, friction_coef * coefficientFriction * q_p, coefficient_collapsing * q_p) ),
                        ( 1.0, new ForceValue(0.0, friction_coef * coefficientFriction * q_p, coefficient_collapsing * q_p) )
                    }
                );

                LocalLinearForce w_lifting = new LocalLinearForce(
                    kind: ForceKind.Wind,
                    load_case: 2 * (i + 1) + 2,
                    distributedForcesByEpsilon: new List<(double Epsilon, ForceValue Value)>
                    {
                        ( 0.0, new ForceValue(0.0, friction_coef * coefficientFriction * q_p, coefficient_lifting * q_p) ),
                        ( 1.0, new ForceValue(0.0, friction_coef * coefficientFriction * q_p, coefficient_lifting * q_p) )
                    }
                );

                List<List<(double, Force)>> all_w = new List<List<(double, Force)>>
                {
                    new List<(double, Force)>
                    {
                        (0.0, w_collapse),
                        (1.0, w_collapse)
                    },
                    new List<(double, Force)>
                    {
                        (0.0, w_lifting),
                        (1.0, w_lifting)
                    },
                };

                forces.AddRange(all_w);
            }
            return forces;

        }

        public ForceValue[] GetSpecificLoadingOnEachPurlin(
           FiniteElementStructure feStructure,
           Force forceToApply)
        {

            var forceVector = forceToApply.GetGlobalForceVector(feStructure.FiniteElementMembers[0].Intervals);
            Vector<double> structureForce = AssemblyMatrixClass.AssembleVector(feStructure.AssemblyMatrices[0], forceVector);
            var supports = feStructure.SolveSupportsOneForce(forceVector);

            return SupportReactionsToForce(supports, forceToApply);
        }

        public ForceValue[] SupportReactionsToForce(
            Dictionary<string, Dictionary<int, Vector<double>>> supportReactions, Force forceToApply)
        {
            var firstDict = supportReactions.Values.First();
            var resultCase = new ForceValue[firstDict.Count];
            for (int i = 0; i < firstDict.Count; i++)
            {
                int key = firstDict.Keys.ElementAt(i);
                var v = firstDict[key];
                resultCase[i] = new ForceValue(
                    v[0], v[1], v[2],
                    v[3], v[4], v[5]);
            }
            return resultCase;
        }
    }
}
