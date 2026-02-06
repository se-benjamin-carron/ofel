using System;
using System.Collections;
using System.Drawing;
using System.Linq;
using MathNet.Numerics.LinearAlgebra;
using Ofel.MatrixCalc;
using Ofel.Core.Utils;

namespace Ofel.Core
{
    /// <summary>
    /// Small wrapper tying a `Member` domain object to finite-element settings for that member.
    /// Currently only holds a mesh size (double).
    /// </summary>
    public sealed class FiniteElementStructure
    {
        /// <summary>
        /// Renvoie toutes les combinaisons possibles de dominante et mineurs à partir d'une liste de variables
        /// </summary>
        public static List<VariableRepartition> GetAllVariableRepartitions(List<string> variables)
        {
            var repartitions = new List<VariableRepartition>();
            repartitions.Add(new VariableRepartition("", new List<string>()));
            foreach (var dominant in variables)
            {
                string dominantType = dominant.Substring(0, 1);
                var mineures = variables.Where(v => v != dominant && v.Substring(0, 1) != dominantType).ToList();
                int mineurCount = mineures.Count;
                // Génère toutes les sous-combinaisons de mineurs (puissance de l'ensemble)
                for (int mask = 0; mask < (1 << mineurCount); mask++)
                {
                    var mineurList = new List<string>();
                    for (int j = 0; j < mineurCount; j++)
                    {
                        if ((mask & (1 << j)) != 0)
                        {
                            mineurList.Add(mineures[j]);
                        }
                    }
                    repartitions.Add(new VariableRepartition(dominant, mineurList));
                }
            }
            return repartitions;
        }
        /// <summary>
        /// The associated Member domain object.
        /// </summary>
        public List<FiniteElementMember> FiniteElementMembers { get; }

        private int _pointNextId = 0;

        /// <summary>
        /// Mesh size (characteristic element length) for this member.
        /// </summary>
        public double Mesh { get; set; }
        public List<PointStructureData> PointsStructure = new List<PointStructureData>();
        public List<List<PointStructureData>> PointsStructureDataByMember = new List<List<PointStructureData>>();

        public List<Matrix<double>> AssemblyMatrices = new List<Matrix<double>>();
        public Matrix<double> GlobalStiffnessMatrix { get; set; } = null!;
        public Matrix<double> GlobalMassMatrix { get; set; } = null!;
        public Dictionary<string, Vector<double>> EffortsByLoadCase
        {
            get; set;
        } = new Dictionary<string, Vector<double>>();
        public Matrix<double> FreeDirectionsMatrix { get; set; } = null!;
        public Matrix<double> FixedDirectionsMatrix { get; set; } = null!;
        public Dictionary<int, Matrix<double>> SupportMatrices { get; set; } = new Dictionary<int, Matrix<double>>();
        public Matrix<double> KusMatrix { get; set; } = null!;
        public Matrix<double> KssMatrix { get; set; } = null!;
        public Dictionary<string, Vector<double>> ForceOnFreeDirections
        {
            get; set;
        } = new Dictionary<string, Vector<double>>();
        public Dictionary<string, Vector<double>> ForceOnFixedDirections
        {
            get; set;
        } = new Dictionary<string, Vector<double>>();
        public Dictionary<string, Vector<double>> DisplacementsOnFreeDirections
        {
            get; set;
        } = new Dictionary<string, Vector<double>>();

        public Dictionary<string, Vector<double>> AllDisplacementsCase
        {
            get; set;
        } = new Dictionary<string, Vector<double>>();
        public Dictionary<string, Dictionary<string, Vector<double>>> AllDisplacementsCombinations
        {
            get; set;
        } = new Dictionary<string, Dictionary<string, Vector<double>>>();
        public Dictionary<string, Dictionary<int, Vector<double>>> SupportReactions
        {
            get; set;
        } = new Dictionary<string, Dictionary<int, Vector<double>>>();

        public ProjectResistanceCoefficient GammaCoefficient { get; set; } = new ProjectResistanceCoefficient();
        private ProjectLoadCoefficient LoadCoefficient { get; set; }

        public FiniteElementStructure(List<FiniteElementMember> f_e_member, BuildingUse use = BuildingUse.Office, double altitude = 0.0)
        {
            FiniteElementMembers = f_e_member ?? throw new ArgumentNullException(nameof(f_e_member));
            LoadCoefficient = new ProjectLoadCoefficient(use, altitude);
        }

        public void Prepare(double mesh)
        {
            Mesh = mesh;
            // Ensure each finite-element member has computed its discretisation (MainEpsilons, Intervals, ...)
            foreach (var fem in FiniteElementMembers)
            {
                fem.Compute(Mesh);
            }
            ComputeAllPoints();
            ComputeAllAssemblyMatrix();
            ComputeAllForce();
            ComputeSMatrices();
            (FreeDirectionsMatrix, FixedDirectionsMatrix, SupportMatrices) = GetFreeFixedDirectionsSupportsMatrices();
            KusMatrix = GetKusMatrix();
            KssMatrix = GetKssMatrix();
        }

        public void Compute(bool IsSecondOrder = false)
        {
            foreach (var (loading_case, loading) in EffortsByLoadCase)
            {
                ForceOnFreeDirections[loading_case] = AssemblyMatrixClass.DisAssembleVector(FreeDirectionsMatrix, loading);
                ForceOnFixedDirections[loading_case] = AssemblyMatrixClass.DisAssembleVector(FixedDirectionsMatrix, loading);
            }
            DisplacementsOnFreeDirections = SolveResults(ForceOnFreeDirections);
            SupportReactions = ComputeSupportReactions(DisplacementsOnFreeDirections, ForceOnFixedDirections);
            ComputeAllDisplacements();
            CombineAllDisplacementsFirstOrder();
            for (int i = 0; i < FiniteElementMembers.Count; i++)
            {
                FiniteElementMembers[i].AdaptGlobalCombinationsToLocal(AllDisplacementsCombinations, AssemblyMatrices[i]);
            }
        }

        public Dictionary<string, Dictionary<int, Vector<double>>> SolveSupportsOneForce(Vector<double> lonelyForce)
        {
            Vector<double> forceOnFree = AssemblyMatrixClass.DisAssembleVector(FreeDirectionsMatrix, lonelyForce);
            Vector<double> forceOnFixed = AssemblyMatrixClass.DisAssembleVector(FixedDirectionsMatrix, lonelyForce);
            Dictionary<string, Vector<double>> forceOnFixedPrepared = new Dictionary<string, Vector<double>> { { "test", forceOnFixed } };
            Dictionary<string, Vector<double>> forceOnFreePrepared = new Dictionary<string, Vector<double>> { { "test", forceOnFree } };

            var displacementsOnFree = SolveResults(forceOnFreePrepared);
            Dictionary<string, Dictionary<int, Vector<double>>> supportReactions = ComputeSupportReactions(displacementsOnFree, forceOnFixedPrepared);
            return supportReactions;
        }

        public void ComputeAllPoints()
        {
            List<PointStructureData> pointsAllStructure = new List<PointStructureData>();
            foreach (FiniteElementMember f_e_member in FiniteElementMembers)
            {
                List<PointStructureData> member_points_data = new List<PointStructureData>();
                (member_points_data, pointsAllStructure) = f_e_member.GetPointsStructureData(pointsAllStructure, ref _pointNextId);
                PointsStructureDataByMember.Add(member_points_data);
            }
            PointsStructure = pointsAllStructure;
        }

        public void ComputeAllForce()
        {

            for (int memberIndex = 0; memberIndex < FiniteElementMembers.Count; memberIndex++)
            {
                var f_e_member = FiniteElementMembers[memberIndex];
                var assembly_matrix = AssemblyMatrices[memberIndex];
                foreach (var force in f_e_member.Member.Forces)
                {
                    Vector<double> memberGlobalForce = force.GetGlobalForceVector(f_e_member.Intervals);
                    Vector<double> structureForce = AssemblyMatrixClass.AssembleVector(assembly_matrix, memberGlobalForce);
                    string loading_case = force.Kind + "_" + force.KindCase;
                    if (EffortsByLoadCase.ContainsKey(loading_case))
                    {
                        EffortsByLoadCase[loading_case] += structureForce;
                    }
                    else
                    {
                        EffortsByLoadCase[loading_case] = structureForce;
                    }
                }
            }
        }

        public void ComputeAllAssemblyMatrix()
        {
            List<Matrix<double>> assembly_matrices = new List<Matrix<double>>();
            // For each member we need an assembly matrix that maps the member's local DOFs
            // (built from its AllEpsilons / intervals) to global DOF indices (PointsStructureData ids * 6).
            // The matrix must have RowCount = total global DOFs and ColumnCount = member local DOFs.
            int totalGlobalDofs = _pointNextId * 6;
            for (int mi = 0; mi < FiniteElementMembers.Count; mi++)
            {
                var member = FiniteElementMembers[mi];
                var memberPoints = PointsStructureDataByMember[mi];
                // member.MainEpsilons correspond (in order) to memberPoints
                // Build a map: local node index (in AllEpsilons) -> global point id
                var nodeLocalToGlobal = new Dictionary<int, int>();
                for (int k = 0; k < member.AllEpsilons.Count; k++)
                {
                    var epsilon = member.AllEpsilons[k];
                    // find corresponding index in AllEpsilons
                    int nodeIndex = member.AllEpsilons.FindIndex(a => Math.Abs(a.Epsilon - epsilon.Epsilon) < 1e-9);
                    if (nodeIndex >= 0 && k < memberPoints.Count)
                    {
                        nodeLocalToGlobal[nodeIndex] = memberPoints[k].id;
                    }
                }

                int memberNodes = member.AllEpsilons.Count;
                int memberDofs = memberNodes * 6;
                var A = Matrix<double>.Build.Dense(totalGlobalDofs, memberDofs, 0.0);

                // For each local node, if it maps to a global point, set the 6 DOF identity mapping
                for (int localNode = 0; localNode < memberNodes; localNode++)
                {
                    if (nodeLocalToGlobal.TryGetValue(localNode, out int globalPointId))
                    {
                        for (int d = 0; d < 6; d++)
                        {
                            int row = globalPointId * 6 + d;
                            int col = localNode * 6 + d;
                            A[row, col] = 1.0;
                        }
                    }
                    // else leave column zeros (local DOFs that are internal to member and not present in global points)
                }
                assembly_matrices.Add(A);
            }
            AssemblyMatrices = assembly_matrices;
        }

        public void ComputeSMatrices()
        {
            Matrix<double> massMatrix = Matrix<double>.Build.Dense(_pointNextId * 6, _pointNextId * 6);
            Matrix<double> stiffnessMatrix = Matrix<double>.Build.Dense(_pointNextId * 6, _pointNextId * 6);
            for (int memberIndex = 0; memberIndex < FiniteElementMembers.Count; memberIndex++)
            {
                var member = FiniteElementMembers[memberIndex];
                var assembly_matrix = AssemblyMatrices[memberIndex];
                Matrix<double> k_member = AssemblyMatrixClass.AssembleMatrix(assembly_matrix.Transpose(), member.GlobalStiffnessMatrix);
                Matrix<double> m_member = AssemblyMatrixClass.AssembleMatrix(assembly_matrix.Transpose(), member.GlobalMassMatrix);

                stiffnessMatrix += k_member;
                massMatrix += m_member;
            }
            GlobalStiffnessMatrix = stiffnessMatrix;
            GlobalMassMatrix = massMatrix;
        }

        public Tuple<Matrix<double>, Matrix<double>, Dictionary<int, Matrix<double>>> GetFreeFixedDirectionsSupportsMatrices()
        {
            int size = _pointNextId * 6;
            Dictionary<int, Matrix<double>> SupportMatrices = new Dictionary<int, Matrix<double>>();
            List<int> FixedDirections = new List<int>();
            List<int> FreeDirections = new List<int>();
            List<List<int>> IndexEachSupport = new List<List<int>>();
            foreach (PointStructureData point in PointsStructure)
            {
                FixedDirections.AddRange(point.FixedDirectionsIndices());
                FreeDirections.AddRange(point.FreeDirectionsIndices());
                if (point.IsSupport)
                {
                    List<int> supportIdx = point.FixedDirectionsIndices();
                    SupportMatrices[point.id] = AssemblyMatrixClass.GetAssemblyMatrixSupport(size, supportIdx);
                }
            }
            Matrix<double> free_matrix = AssemblyMatrixClass.GetAssemblyMatrix(size, FreeDirections);
            Matrix<double> fixed_matrix = AssemblyMatrixClass.GetAssemblyMatrix(size, FixedDirections);
            return Tuple.Create(free_matrix, fixed_matrix, SupportMatrices);
        }

        public Matrix<double> GetKusMatrix()
        {
            var assemblyMatrix1 = FreeDirectionsMatrix.Transpose();
            var assemblyMatrix2 = FixedDirectionsMatrix;
            return assemblyMatrix2.Multiply(GlobalStiffnessMatrix).Multiply(assemblyMatrix1);
        }
        public Matrix<double> GetKssMatrix()
        {
            // Kss should be reduced stiffness on fixed DOFs: A_fixed^T * K_global * A_fixed
            return AssemblyMatrixClass.DisAssembleMatrix(FreeDirectionsMatrix, GlobalStiffnessMatrix);
        }

        public Dictionary<string, Vector<double>> SolveResults(Dictionary<string, Vector<double>> ForceApplied)
        {
            var displacementsByLoadCase = new Dictionary<string, Vector<double>>();

            foreach (var (loading_case, force) in ForceApplied)
            {
                Vector<double> displacements;
                try
                {
                    displacements = KssMatrix.Solve(force);
                }
                catch (Exception ex)
                {
                    var svd = KssMatrix.Svd(true);
                    var U = svd.U;
                    var VT = svd.VT;
                    var s = svd.S;

                    int m = KssMatrix.RowCount;
                    int n = KssMatrix.ColumnCount;

                    // Tolerance pour considérer une valeur singulière comme non-nulle
                    double eps = 1e-12;
                    double maxS = s.Count > 0 ? s[0] : 0.0;
                    double tol = Math.Max(m, n) * maxS * eps;

                    var SigmaPlus = Matrix<double>.Build.Dense(n, m, 0.0);
                    for (int i = 0; i < s.Count; i++)
                    {
                        if (Math.Abs(s[i]) > tol)
                        {
                            SigmaPlus[i, i] = 1.0 / s[i];
                        }
                    }

                    var V = VT.Transpose();
                    var pseudoInverse = V.Multiply(SigmaPlus).Multiply(U.Transpose());

                    displacements = pseudoInverse.Multiply(force);
                }
                displacementsByLoadCase[loading_case] = displacements;
            }

            return displacementsByLoadCase;
        }

        public Dictionary<string, Dictionary<int, Vector<double>>> ComputeSupportReactions(Dictionary<string, Vector<double>> displacementsOnFree, Dictionary<string, Vector<double>> forceOnFixed)
        {
            Dictionary<string, Dictionary<int, Vector<double>>> supportReactions = new Dictionary<string, Dictionary<int, Vector<double>>>();
            foreach (var (loading_case, displacements) in displacementsOnFree)
            {
                Vector<double> reaction = KusMatrix * displacements;
                reaction -= forceOnFixed[loading_case];
                Vector<double> globalReaction = FixedDirectionsMatrix.Transpose().Multiply(reaction);

                Dictionary<int, Vector<double>> supportReactionsComb = new Dictionary<int, Vector<double>>();
                foreach (var kv in SupportMatrices)
                {
                    int supportPointId = kv.Key;
                    var supportAssemblyMatrix = kv.Value;
                    supportReactionsComb[supportPointId] = supportAssemblyMatrix.Multiply(globalReaction);
                }
                supportReactions[loading_case] = supportReactionsComb;
            }
            return supportReactions;
        }


        public void ComputeAllDisplacements()
        {
            int size = FixedDirectionsMatrix.RowCount;
            var vector_fixed_direction = Vector<double>.Build.Dense(size);
            Dictionary<string, Vector<double>> allDisplacements = new Dictionary<string, Vector<double>>();
            foreach (var (loading_case, displacements) in DisplacementsOnFreeDirections)
            {
                Vector<double> displacements_fixed = AssemblyMatrixClass.AssembleVector(FixedDirectionsMatrix.Transpose(), vector_fixed_direction);
                Vector<double> displacements_free = AssemblyMatrixClass.AssembleVector(FreeDirectionsMatrix.Transpose(), displacements);
                allDisplacements[loading_case] = displacements_fixed + displacements_free;
            }
            AllDisplacementsCase = allDisplacements;
        }

        public void CombineAllDisplacementsFirstOrder()
        {
            Dictionary<string, Vector<double>> GlobalDisplacementsUlsF = CombineAllDisplacementsCombinations(
                AllDisplacementsCase, LoadCoefficient.Uls.Permanent, LoadCoefficient.Uls.Variable, GetPsiDefaultCoefficients(), GetPsi0Coefficients());
            Dictionary<string, Vector<double>> GlobalDisplacementsUlsAcc = CombineAllDisplacementsCombinations(
                AllDisplacementsCase, LoadCoefficient.UlsAcc.Permanent, LoadCoefficient.UlsAcc.Variable, GetPsi2Coefficients(), GetPsiDefaultCoefficients());
            Dictionary<string, Vector<double>> GlobalDisplacementsSlsCar = CombineAllDisplacementsCombinations(
                AllDisplacementsCase, LoadCoefficient.Sls.Permanent, LoadCoefficient.Sls.Variable, GetPsiDefaultCoefficients(), GetPsi0Coefficients());
            Dictionary<string, Vector<double>> GlobalDisplacementsSlsQuasiPermanent = CombineAllDisplacementsCombinations(
                AllDisplacementsCase, LoadCoefficient.Sls.Permanent, LoadCoefficient.Sls.Variable, GetPsi2Coefficients(), GetPsi2Coefficients());
            Dictionary<string, Vector<double>> GlobalDisplacementsSlsFrequent = CombineAllDisplacementsCombinations(
                AllDisplacementsCase, LoadCoefficient.Sls.Permanent, LoadCoefficient.Sls.Variable, GetPsi2Coefficients(), GetPsi1Coefficients());
            AllDisplacementsCombinations["ULS_F"] = GlobalDisplacementsUlsF;
            AllDisplacementsCombinations["ULS_Acc"] = GlobalDisplacementsUlsAcc;
            AllDisplacementsCombinations["SLS_Car"] = GlobalDisplacementsSlsCar;
            AllDisplacementsCombinations["SLS_QP"] = GlobalDisplacementsSlsQuasiPermanent;
            AllDisplacementsCombinations["SLS_Frequent"] = GlobalDisplacementsSlsFrequent;
        }

        public Dictionary<string, Vector<double>> CombineAllDisplacementsCombinations(
            Dictionary<string, Vector<double>> cases, double[] coefPerm, double[] coefVar,
            Dictionary<string, double> psiMajor, Dictionary<string, double> psiMinors)
        {
            var permanents = cases.Where(kv => kv.Key == "G" || kv.Key == "PP").ToList();
            var variables = cases.Where(kv => kv.Key != "G" && kv.Key != "PP").ToList();

            var combinaisonDict = new Dictionary<string, Vector<double>>();
            int vectorSize = cases.Count > 0 ? cases.First().Value.Count : 0;
            Vector<double> G = cases.ContainsKey("G") ? cases["G"] : Vector<double>.Build.Dense(vectorSize, 0.0);
            Vector<double> OW = cases.ContainsKey("OW") ? cases["OW"] : Vector<double>.Build.Dense(vectorSize, 0.0);
            Vector<double> combinationPerm = G + OW;
            List<VariableRepartition> repartitions = GetAllVariableRepartitions(variables.Select(v => v.Key).ToList());
            int numCombinaison = 1;
            foreach (var coefPermanent in coefPerm)
            {
                foreach (var coefVariable in coefVar)
                {
                    foreach (var repartition in repartitions)
                    {
                        Vector<double> combinationVar = Vector<double>.Build.Dense(
                            cases.First().Value.Count, 0.0);
                        bool skip = false;

                        var major = repartition.Major;
                        var minors = repartition.Minors;
                        string name = "";
                        var combinationTotal = Vector<double>.Build.Dense(vectorSize);
                        if (major != "")
                        {
                            string majorKey = major.Substring(0, 1);
                            double major_psi = psiMajor.ContainsKey(majorKey) ? psiMajor[majorKey] : 1.0;
                            string variable_name = $"+ {Math.Round(coefVariable, 2)}x{major}";
                            combinationVar += cases[major] * major_psi;
                            if (major_psi == 0) { skip = true; break; }
                            foreach (var minor in minors)
                            {
                                string minorKey = minor.Substring(0, 1);
                                double minor_psi = psiMinors.ContainsKey(minorKey) ? psiMinors[minorKey] : 1.0;
                                if (!cases.ContainsKey(minor)) continue;
                                if (minor_psi == 0) { skip = true; break; }
                                combinationVar += cases[minor] * minor_psi;
                                variable_name += $" + {Math.Round(coefVariable * minor_psi, 2)} {minor} ";
                            }
                            if (skip) continue;
                            name = $"{numCombinaison}/ {coefPermanent} (G + PP) {variable_name}";
                            combinationTotal = combinationVar * coefVariable + combinationPerm * coefPermanent;
                        }
                        else
                        {
                            name = $"{numCombinaison}/ {coefPermanent} (G + PP)";
                            combinationTotal = combinationPerm * coefPermanent;
                        }
                        combinaisonDict[name] = combinationTotal;
                        numCombinaison++;
                    }
                }
            }
            return combinaisonDict;
        }


        public Dictionary<string, double> GetPsi0Coefficients()
        {
            Dictionary<string, double> psi0 = new Dictionary<string, double>();
            var psi_snow = LoadCoefficient.Snow;
            var psi_wind = LoadCoefficient.Wind;
            var psi_exploitation = LoadCoefficient.Exploitation;
            var psi_thermal = LoadCoefficient.Temperature;
            var psi_accidental = LoadCoefficient.Accidental;
            psi0["S"] = psi_snow.psi0;
            psi0["W"] = psi_wind.psi0;
            psi0["Q"] = psi_exploitation.psi0;
            psi0["T"] = psi_thermal.psi0;
            psi0["E"] = psi_accidental.psi0;
            psi0["C"] = psi_accidental.psi0;
            return psi0;
        }
        public Dictionary<string, double> GetPsi1Coefficients()
        {
            Dictionary<string, double> psi1 = new Dictionary<string, double>();
            var psi_snow = LoadCoefficient.Snow;
            var psi_wind = LoadCoefficient.Wind;
            var psi_exploitation = LoadCoefficient.Exploitation;
            var psi_thermal = LoadCoefficient.Temperature;
            var psi_accidental = LoadCoefficient.Accidental;
            psi1["S"] = psi_snow.psi1;
            psi1["W"] = psi_wind.psi1;
            psi1["Q"] = psi_exploitation.psi1;
            psi1["T"] = psi_thermal.psi1;
            psi1["E"] = psi_accidental.psi1;
            psi1["C"] = psi_accidental.psi1;
            return psi1;
        }
        public Dictionary<string, double> GetPsi2Coefficients()
        {
            Dictionary<string, double> psi2 = new Dictionary<string, double>();
            var psi_snow = LoadCoefficient.Snow;
            var psi_wind = LoadCoefficient.Wind;
            var psi_exploitation = LoadCoefficient.Exploitation;
            var psi_thermal = LoadCoefficient.Temperature;
            var psi_accidental = LoadCoefficient.Accidental;
            psi2["S"] = psi_snow.psi2;
            psi2["W"] = psi_wind.psi2;
            psi2["Q"] = psi_exploitation.psi2;
            psi2["T"] = psi_thermal.psi2;
            psi2["E"] = psi_accidental.psi2;
            psi2["C"] = psi_accidental.psi2;
            return psi2;
        }

        public Dictionary<string, double> GetPsiDefaultCoefficients()
        {
            Dictionary<string, double> psi2 = new Dictionary<string, double>();
            psi2["S"] = 1;
            psi2["W"] = 1;
            psi2["Q"] = 1;
            psi2["T"] = 1;
            psi2["E"] = 1;
            psi2["C"] = 1;
            return psi2;
        }
    }
}
