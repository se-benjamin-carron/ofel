using MathNet.Numerics.LinearAlgebra;
using Ofel.Core.SectionParameter;
using Ofel.MatrixCalc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Globalization;

public static class JsonHandler
{
    private static Matrix<double> ReadMatrixFromJson(string path)
    {
        string json = File.ReadAllText(path);
        var matJson = JsonSerializer.Deserialize<MatrixJson>(json)!;

        var mat = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(matJson.Rows, matJson.Cols);
        for (int i = 0; i < matJson.Rows; i++)
        {
            for (int j = 0; j < matJson.Cols; j++)
            {
                mat[i, j] = matJson.Data[i][j];
            }
        }
        return mat;
    }

    private static void WriteMatrixToJson(string path, Matrix<double> mat)
    {
        var matJson = new MatrixJson
        {
            Rows = mat.RowCount,
            Cols = mat.ColumnCount,
            Data = new double[mat.RowCount][]
        };
        for (int i = 0; i < mat.RowCount; i++)
        {
            matJson.Data[i] = new double[mat.ColumnCount];
            for (int j = 0; j < mat.ColumnCount; j++)
                matJson.Data[i][j] = mat[i, j];
        }
        string json = JsonSerializer.Serialize(matJson, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(path, json);
    }

    public static string GetTestDataPath(string fileName)
    {
        var path = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "JsonTests", fileName);
        return Path.GetFullPath(path);
    }
    public class MatrixJson
    {
        public int Rows { get; set; }
        public int Cols { get; set; }
        public double[][] Data { get; set; } = Array.Empty<double[]>();
    }

    public static void CompareMatrices(
      Matrix<double> actual,
      string expectedJsonPath,
      double relTol = 1e-6,
      bool overwriteNeeded = false)
    {
        // si overwrite, on écrit directement dans le fichier source
        if (overwriteNeeded)
        {
            WriteMatrixToJson(expectedJsonPath, actual);
            return;
        }

        // sinon lecture de la référence
        var expected = ReadMatrixFromJson(expectedJsonPath);

        Assert.Equal(expected.RowCount, actual.RowCount);
        Assert.Equal(expected.ColumnCount, actual.ColumnCount);

        for (int i = 0; i < actual.RowCount; i++)
            for (int j = 0; j < actual.ColumnCount; j++)
            {
                double a = actual[i, j];
                double b = expected[i, j];
                double tol = relTol * Math.Max(1.0, Math.Abs(b));
                Assert.InRange(Math.Abs(a - b), 0.0, tol);
            }
    }

}

namespace Ofel.Core.Tests.MatrixCalc
{

    public class StiffnessMatrixTests
    {
        private static string FindTestData(string fileName)
        {
            // Test project copies TestData to output; look relative to base directory
            var candidate = Path.Combine(AppContext.BaseDirectory, "JsonTests", fileName);
            if (File.Exists(candidate)) return candidate;
            throw new FileNotFoundException(fileName);
        }

        [Fact]
        public void ComputeStiffnessMatrix()
        {
            //var jsonPath = FindTestData("stiffness_example1.json");
            var expectedJsonPath = JsonHandler.GetTestDataPath("stiffness_example1.json");

            // données du cas
            double A = 25.3e-4, It = 6.02e-8, Iy = 606e-8, Iz = 231e-8, Ay = 16.08e-4, Az = 4.83e-4;
            double E = 210e9, G = 81e9, L = 1.0;
            var geom = new SteelSection(profileType: "IPE", name: "IPE 200",
                                        h: 0, b: 0, tw: 0, tf: 0, r1: 0, r2: 0,
                                        a: A, ay: Ay, az: Az, iy: Iy, iz: Iz, it: It,
                                        iw: 0, w_el_y: 0, w_el_z: 0, w_pl_y: 0, w_pl_z: 0);
            var mat = new SteelMaterial("TST", "TEST", 0, 0, E, G, 0, 0);
            var hingeL = new IsHinged(false, false, false);
            var hingeR = new IsHinged(false, false, false);

            var K = MatrixHelpers.Stiffness(geom, mat, hingeL, hingeR, L);

            // vérifier la taille
            Assert.Equal(12, K.RowCount);
            Assert.Equal(12, K.ColumnCount);

            // comparaison avec JSON
            JsonHandler.CompareMatrices(K, expectedJsonPath, 1e-6);

            // sanity check : symétrie
            var tolSym = 1e-9;
            for (int i = 0; i < K.RowCount; i++)
                for (int j = 0; j < K.ColumnCount; j++)
                    Assert.InRange(Math.Abs(K[i, j] - K[j, i]), 0.0, tolSym);
        }
    }
}
