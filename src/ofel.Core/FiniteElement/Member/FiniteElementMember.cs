using MathNet.Numerics.LinearAlgebra;
using Ofel.Core;
using Ofel.Core.Utils;
using Ofel.MatrixCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Ofel.Core
{
    /// <summary>
    /// Small wrapper tying a `Member` domain object to finite-element settings for that member.
    /// Currently only holds a mesh size (float).
    /// </summary>
    public sealed class FiniteElementMember
    {
        /// <summary>
        /// The associated Member domain object.
        /// </summary>
        public Member Member { get; }

        public FiniteElementMember(Member member)
        {
            Member = member ?? throw new ArgumentNullException(nameof(member));
        }

        public List<MainEpsilon> MainEpsilons = null!;
        public List<MainEpsilon> AllEpsilons = null!;
        public List<IntervalData> Intervals = null!;
        public Matrix<double> GlobalStiffnessMatrix { get; set; } = null!;
        public Matrix<double> GlobalMassMatrix { get; set; } = null!;
        public Matrix<double> GlobalUniformStressMatrix { get; set; } = null!;
        public Dictionary<string, Vector<double>> EffortsByLoadCase
        {
            get; set;
        } = new Dictionary<string, Vector<double>>();

        public Dictionary<string, Dictionary<int, Vector<double>>> GlobalDisplacements
            = new Dictionary<string, Dictionary<int, Vector<double>>>();
        public ResultFormat ResultsCombination { get; set; } = new ResultFormat();

        /// <summary>
        /// Geometry associated to each epsilon point.
        /// </summary>
        /// <summary>
        /// Compute FE discretisation and simple stiffness matrices and applied loads.
        /// Steps:
        /// 1) compute main epsilons from characteristics (and include 0 and 1)
        /// 2) subdivide each main interval according to Mesh to produce AllEpsilons
        /// 3) build one 6x6 stiffness matrix per interval (very simple heuristic)
        /// 4) sum them to get GlobalStiffness
        /// 5) compute applied forces at each epsilon by asking each Force for its local value
        ///
        /// Note: this is a high-level implementation with simple placeholders for stiffness
        /// calculation; replace with domain formulas as needed.
        /// </summary>
        public void Compute(double mesh)
        {
            MainEpsilons = MainEpsilonCalc.ComputeMainEpsilons(Member);
            AllEpsilons = AllEpsilonCalc.ComputeAllEpsilons(Member, MainEpsilons, mesh);
            Intervals = IntervalDataCalc.ComputeAllIntervalDatas(Member, AllEpsilons);
            // Assemble interval matrices into member-level global matrices
            if (Intervals != null && Intervals.Count > 0)
            {
                int nIntervals = Intervals.Count;
                int nodes = nIntervals + 1;
                int memberDofs = 6 * nodes;

                var globalK = Matrix<double>.Build.Dense(memberDofs, memberDofs, 0.0);
                var globalM = Matrix<double>.Build.Dense(memberDofs, memberDofs, 0.0);
                var globalStress = Matrix<double>.Build.Dense(memberDofs, memberDofs, 0.0);

                for (int i = 0; i < nIntervals; i++)
                {
                    var interval = Intervals[i];
                    // connectivity: [6*i .. 6*i+5] then [6*(i+1) .. 6*(i+1)+5]
                    var conn = new List<int>();
                    for (int d = 0; d < 6; d++) conn.Add(6 * i + d);
                    for (int d = 0; d < 6; d++) conn.Add(6 * (i + 1) + d);

                    var A = AssemblyMatrixClass.GetAssemblyMatrix(memberDofs, conn);

                    if (interval.GlobalStiffnessMatrix != null)
                        globalK += AssemblyMatrixClass.AssembleMatrix(A, interval.GlobalStiffnessMatrix);
                    if (interval.GlobalMassMatrix != null)
                        globalM += AssemblyMatrixClass.AssembleMatrix(A, interval.GlobalMassMatrix);
                    if (interval.GlobalUniformStressMatrix != null)
                        globalStress += AssemblyMatrixClass.AssembleMatrix(A, interval.GlobalUniformStressMatrix);
                }
                GlobalStiffnessMatrix = globalK;
                GlobalMassMatrix = globalM;
                GlobalUniformStressMatrix = globalStress;
            }
        }


        public static bool IsValidMainEpsilonList(IEnumerable<MainEpsilon> list)
        {
            var arr = list.ToArray();
            for (int i = 0; i < arr.Length; i++)
            {
                var kind1 = arr[i].Kind;
                for (int j = i + 1; j < arr.Length; j++)
                {
                    var kind2 = arr[j].Kind;
                    bool isNaturalConflict =
                        (kind1 == KindMainEpsilon.NaturalHingeChar && kind2 == KindMainEpsilon.SpringChar) ||
                        (kind1 == KindMainEpsilon.SpringChar && kind2 == KindMainEpsilon.NaturalHingeChar);
                    bool isUnnaturalConflict =
                        (kind1 == KindMainEpsilon.UnNaturalHingeChar && kind2 == KindMainEpsilon.SpringChar) ||
                        (kind1 == KindMainEpsilon.SpringChar && kind2 == KindMainEpsilon.UnNaturalHingeChar);
                    if (isNaturalConflict || isUnnaturalConflict)
                        return false;
                }
            }
            return true;
        }

        public Tuple<List<PointStructureData>, List<PointStructureData>> GetPointsStructureData(List<PointStructureData> pointsAllStructure, ref int nextPointIdStructure)
        {
            var pointsMember = new List<PointStructureData>();
            var epsilonList = Intervals.Select(i => i.Epsilon1).ToList();
            var lastEpsilon2 = Intervals.Last().Epsilon2;
            epsilonList.Add(lastEpsilon2);
            foreach (var epsilon in epsilonList)
            {
                var (pt, IsAssembly, IsSupport, d_o_f) = GetDataPoint(epsilon);
                bool existing = false;
                foreach (PointStructureData pas in pointsAllStructure)
                {
                    var distance = pas.Point.DistanceTo(pt);
                    var share = pas.IsPossibleToSharePoint();
                    var existngfalse = !existing;
                    if (distance < 5e-7 && share && !existing)
                    {
                        if (pas.IsAssembly && IsSupport)
                        {
                            pas.IsSupport = true;
                            pas.SupportConditions = d_o_f;
                        }
                        if (pas.IsSupport && IsAssembly)
                        {
                            pas.IsAssembly = true;
                        }
                        existing = true;
                        pointsMember.Add(pas);
                    }
                }
                if (!existing)
                {
                    PointStructureData psd = new PointStructureData(pt, IsAssembly, IsSupport, d_o_f, ref nextPointIdStructure);
                    pointsMember.Add(psd);
                    pointsAllStructure.Add(psd);
                }
            }
            return Tuple.Create(pointsMember, pointsAllStructure);
        }

        public Tuple<Point, bool, bool, DegreesOfFreedom> GetDataPoint(double epsilon)
        {
            Point pt = Member.GetInterpolatedPoint(epsilon);
            bool IsAssembly = Member.IsAssembly(epsilon);
            bool IsSupport = Member.IsSupport(epsilon);
            DegreesOfFreedom d_o_f = new DegreesOfFreedom(true, true, true, true, true, true);
            if (IsSupport)
            {
                d_o_f = Member.getSupportDataFromEpsilon(epsilon);
            }
            return Tuple.Create(pt, IsAssembly, IsSupport, d_o_f);
        }

        public void MapResultsToMemberIntervals(Dictionary<string, Vector<double>> memberGlobalDisplacements)
        {
            foreach (var (loading_case, displacements) in memberGlobalDisplacements)
            {
                for (int i = 0; i < Intervals.Count; i++)
                {
                    Vector<double> mainEpsilonGlobalDisplacements = displacements.SubVector(i * 6, 6);
                    IntervalData intervalData = Intervals[i];
                    Vector<double> local_displacements = RotationMatrixClass.SwitchVectorFromGlobalToLocal(
                        intervalData.RotationMatrix,
                        mainEpsilonGlobalDisplacements);

                    intervalData.LocalDisplacements[loading_case] = local_displacements;
                    intervalData.computeInternalEfforts(loading_case);
                }
            }
        }

        public Dictionary<string, Dictionary<string, Vector<double>>> GetMemberDisplacementsCombinations(
            Dictionary<string, Dictionary<string, Vector<double>>> allDisplacementsCombinations,
            Matrix<double> assemblyMatrix
        )
        {
            return allDisplacementsCombinations.ToDictionary(
                kv => kv.Key,
                kv => kv.Value.ToDictionary(
                    caseKv => caseKv.Key,
                    caseKv => AssemblyMatrixClass.DisAssembleVector(assemblyMatrix.Transpose(), caseKv.Value)
                )
            );
        }

        public void AdaptGlobalCombinationsToLocal(
            Dictionary<string, Dictionary<string, Vector<double>>> allDisplacementsCombinations,
            Matrix<double> assemblyMatrix)
        {
            ResultFormat resultFormat = new ResultFormat();
            var GlobalMemberDisplacements = GetMemberDisplacementsCombinations(allDisplacementsCombinations, assemblyMatrix);
            resultFormat.Uls = InternalDataFromGlobalDisplacements(GlobalMemberDisplacements["ULS_F"]);
            resultFormat.UlsAcc = InternalDataFromGlobalDisplacements(GlobalMemberDisplacements["ULS_Acc"]);
            resultFormat.SlsQp = InternalDataFromGlobalDisplacements(GlobalMemberDisplacements["SLS_QP"]);
            resultFormat.SlsFrequent = InternalDataFromGlobalDisplacements(GlobalMemberDisplacements["SLS_Frequent"]);
            resultFormat.SlsCar = InternalDataFromGlobalDisplacements(GlobalMemberDisplacements["SLS_Car"]);

            ResultsCombination = resultFormat;
        }

        public Dictionary<string, CombinationsData> InternalDataFromGlobalDisplacements(Dictionary<string, Vector<double>> memberGlobalDisplacements)
        {
            double[] all_epsilon = AllEpsilons.Select(e => e.Epsilon).ToArray();
            double[] allEpsilonMiddle = AllEpsilons
                .Zip(AllEpsilons.Skip(1), (a, b) => (a.Epsilon + b.Epsilon) / 2.0)
                .ToArray();
            Dictionary<string, CombinationsData> allResult = new Dictionary<string, CombinationsData>();
            foreach (var (loading_case, memberCaseDisplacements) in memberGlobalDisplacements)
            {
                List<Vector<double>> allInternalEffortsCase = new List<Vector<double>>();
                List<Vector<double>> allDisplacementsCase = new List<Vector<double>>();
                for (int i = 0; i < Intervals.Count; i++)
                {

                    Vector<double> memberDisplacementsIntervals = memberCaseDisplacements.SubVector(i * 6, 12);
                    Vector<double> localDisplacements = RotationMatrixClass.SwitchVectorFromGlobalToLocal(
                        Intervals[i].RotationMatrix,
                        memberDisplacementsIntervals);
                    var leftDisplacements = localDisplacements.SubVector(0, 6);
                    var rightDisplacements = localDisplacements.SubVector(6, 6);
                    var internalEffortsInterval12x1 = Intervals[i].StiffnessMatrix * localDisplacements;
                    var internalEffortInterval6x1 = Vector<double>.Build.Dense(6);
                    for (int j = 0; j < 6; j++)
                    {
                        internalEffortInterval6x1[j] = (internalEffortsInterval12x1[j] - internalEffortsInterval12x1[j + 6]) / 2.0;
                    }
                    allDisplacementsCase.Add(leftDisplacements);
                    if (i == Intervals.Count - 1)
                    {
                        allDisplacementsCase.Add(rightDisplacements);
                    }
                    allInternalEffortsCase.Add(internalEffortInterval6x1);
                }
                double[,] interpolatedEfforts = InterpolateArray(
                    allEpsilonMiddle,
                    allInternalEffortsCase.ToArray(),
                    all_epsilon);
                int cols = allDisplacementsCase.Count();

                ForceResults internalEfforts = new ForceResults(
                    Enumerable.Range(0, interpolatedEfforts.GetLength(1)).Select(i => interpolatedEfforts[0, i]).ToArray(),
                    Enumerable.Range(0, interpolatedEfforts.GetLength(1)).Select(i => interpolatedEfforts[1, i]).ToArray(),
                    Enumerable.Range(0, interpolatedEfforts.GetLength(1)).Select(i => interpolatedEfforts[2, i]).ToArray(),
                    Enumerable.Range(0, interpolatedEfforts.GetLength(1)).Select(i => interpolatedEfforts[3, i]).ToArray(),
                    Enumerable.Range(0, interpolatedEfforts.GetLength(1)).Select(i => interpolatedEfforts[4, i]).ToArray(),
                    Enumerable.Range(0, interpolatedEfforts.GetLength(1)).Select(i => interpolatedEfforts[5, i]).ToArray()
                );
                // Par ce code corrigé :
                var ux = allDisplacementsCase.Select(v => v[0]).ToArray();
                var uy = allDisplacementsCase.Select(v => v[1]).ToArray();
                var uz = allDisplacementsCase.Select(v => v[2]).ToArray();
                var thetax = allDisplacementsCase.Select(v => v[3]).ToArray();
                var thetay = allDisplacementsCase.Select(v => v[4]).ToArray();
                var thetaz = allDisplacementsCase.Select(v => v[5]).ToArray();

                DisplacementsResults displacementsResults = new DisplacementsResults(ux, uy, uz, thetax, thetay, thetaz);
                CombinationsData combinationData = new CombinationsData(internalEfforts, displacementsResults);
                allResult[loading_case] = combinationData;
            }
            return allResult;
        }
        public static double[,] InterpolateArray(double[] x, Vector<double>[] y, double[] x_new)
        {
            int dim = y[0].Count;
            var y_new = new double[dim, x_new.Length];
            for (int k = 0; k < dim; k++)
            {
                for (int i = 0; i < x_new.Length; i++)
                {
                    double xi = x_new[i];
                    int j = Array.FindLastIndex(x, val => val <= xi);
                    if (j < 0) j = 0;
                    if (j >= x.Length - 1) j = x.Length - 2;
                    double x0 = x[j], x1 = x[j + 1];
                    double y0 = y[j][k], y1 = y[j + 1][k];
                    y_new[k, i] = y0 + (y1 - y0) * (xi - x0) / (x1 - x0);
                }
            }
            return y_new;
        }
    }
}


//         private void ComputeIntervalStiffnesses()
//         {
//             _intervalStiffnesses.Clear();
//             // use native stiffness matrix per segment
//             float memberLength = EstimateMemberLength();
//             var mat = Member.Material as SteelMaterial ?? throw new InvalidOperationException("Material must be SteelMaterial");
//             // collect hinge epsilons
//             var hingeEps = new HashSet<float>(Member.Characteristics.OfType<HingeChar>()
//                 .SelectMany(h => h.GetEpsilons()).Select(e => Clamp01(e)));
//             for (int i = 0; i < _allEpsilons.Count - 1; i++)
//             {
//                 var e0 = _allEpsilons[i];
//                 var e1 = _allEpsilons[i+1];
//                 float rel = e1 - e0;
//                 float elemLength = Math.Max(1e-6f, rel * memberLength);
//                 // get section geometry at start
//                 var sec = Member.GetEpsilonGeometry(e0) as SteelSection ?? throw new InvalidOperationException("Geometry must be SteelSection");
//                 // determine hinge conditions
//                 bool startHinge = hingeEps.Contains(e0);
//                 bool endHinge = hingeEps.Contains(e1);
//                 // compute element stiffness via managed Eigen.NET wrapper
//                 float[,] k;
//                 if (startHinge && !endHinge)
//                 {
//                     // first end hinged, second fixed
//                     k = EigenStiffness.HingedFixed(
//                         mat.E, mat.G,
//                         sec.I_y, sec.I_z, sec.It,
//                         sec.A, sec.A_y, sec.A_z,
//                         elemLength,
//                         true, true, true);
//                 }
//                 else if (!startHinge && endHinge)
//                 {
//                     // first fixed, second hinged
//                     k = EigenStiffness.FixedHinged(
//                         mat.E, mat.G,
//                         sec.I_y, sec.I_z, sec.It,
//                         sec.A, sec.A_y, sec.A_z,
//                         elemLength,
//                         true, true, true);
//                 }
//                 else
//                 {
//                     // both fixed
//                     k = EigenStiffness.BothFixed(
//                         mat.E, mat.G,
//                         sec.I_y, sec.I_z, sec.It,
//                         sec.A, sec.A_y, sec.A_z,
//                         elemLength);
//                 }
//                 _intervalStiffnesses.Add(k);
//             }
//         }

//         private void ComputeGlobalStiffness()
//         {
//             _globalStiffness = new float[6,6];
//             foreach (var km in _intervalStiffnesses)
//             {
//                 for (int r = 0; r < 6; r++) for (int c = 0; c < 6; c++) _globalStiffness[r,c] += km[r,c];
//             }
//         }

//         private void ComputeAppliedForces(RotationData? rotationData)
//         {
//             _forcesByEpsilon = new SortedDictionary<float, Vector3>();
//             foreach (var eps in _allEpsilons)
//             {
//                 var total = Vector3.Zero;
//                 var ed = new EpsilonData(eps);
//                 foreach (var f in Member.Forces)
//                 {
//                     try
//                     {
//                         var lf = f.GetLocalForce(rotationData, ed);
//                         total += lf;
//                     }
//                     catch
//                     {
//                         // ignore per-force errors
//                     }
//                 }
//                 _forcesByEpsilon[eps] = total;
//             }
//         }

//         private static float Clamp01(float v) => Math.Min(1f, Math.Max(0f, v));

//         private float EstimateMemberLength()
//         {
//             var pts = Member.Points.Values.ToList();
//             if (pts.Count < 2) return 1f;
//             var first = pts.First();
//             var last = pts.Last();
//             var dx = last.X - first.X;
//             var dy = last.Y - first.Y;
//             var dz = last.Z - first.Z;
//             return (float)Math.Sqrt(dx*dx + dy*dy + dz*dz);
//         }
//     }
// }
