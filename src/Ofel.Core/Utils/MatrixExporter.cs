using System;
using System.Globalization;
using System.IO;
using MathNet.Numerics.LinearAlgebra;

namespace Ofel.Core.Utils
{
    /// <summary>
    /// Utility helper to export MathNet matrices and vectors to CSV for visualization.
    /// Usage:
    ///   MatrixExporter.ExportMatrixCsv("out.csv", matrix);
    ///   MatrixExporter.ExportVectorCsv("vec.csv", vector);
    /// The CSV will include column headers (col0,col1,...) and row indices if requested.
    /// </summary>
    public static class MatrixExporter
    {
        /// <summary>
        /// Export a Matrix<double> to CSV.
        /// </summary>
        public static void ExportMatrixCsv(string path, Matrix<double> mat, string delimiter = ",", bool includeIndices = true, IFormatProvider formatProvider = null)
        {
            if (mat == null) throw new ArgumentNullException(nameof(mat));
            formatProvider ??= CultureInfo.InvariantCulture;
            using var sw = new StreamWriter(path, false);

            int rows = mat.RowCount;
            int cols = mat.ColumnCount;

            // Header
            if (includeIndices)
            {
                sw.Write("index");
                for (int c = 0; c < cols; c++) sw.Write(delimiter + "col" + c);
                sw.WriteLine();
            }
            else
            {
                for (int c = 0; c < cols; c++)
                {
                    if (c > 0) sw.Write(delimiter);
                    sw.Write("col" + c);
                }
                sw.WriteLine();
            }

            for (int r = 0; r < rows; r++)
            {
                if (includeIndices) sw.Write(r);
                for (int c = 0; c < cols; c++)
                {
                    var v = mat[r, c];
                    if (includeIndices) sw.Write(delimiter + v.ToString("G17", formatProvider));
                    else
                    {
                        if (c > 0) sw.Write(delimiter);
                        sw.Write(v.ToString("G17", formatProvider));
                    }
                }
                sw.WriteLine();
            }
        }

        /// <summary>
        /// Export a Vector<double> to CSV as single column (optionally with index).
        /// </summary>
        public static void ExportVectorCsv(string path, Vector<double> vec, string delimiter = ",", bool includeIndices = true, IFormatProvider formatProvider = null)
        {
            if (vec == null) throw new ArgumentNullException(nameof(vec));
            formatProvider ??= CultureInfo.InvariantCulture;
            using var sw = new StreamWriter(path, false);

            int n = vec.Count;
            if (includeIndices) sw.WriteLine("index" + delimiter + "value");
            else sw.WriteLine("value");

            for (int i = 0; i < n; i++)
            {
                if (includeIndices) sw.Write(i + delimiter + vec[i].ToString("G17", formatProvider));
                else sw.Write(vec[i].ToString("G17", formatProvider));
                sw.WriteLine();
            }
        }

        /// <summary>
        /// Convenience: export matrix to a CSV next to an existing path by adding a suffix.
        /// </summary>
        public static string ExportWithAutoName(string basePathWithoutExt, Matrix<double> mat, string suffix = "_matrix")
        {
            var path = basePathWithoutExt + suffix + ".csv";
            ExportMatrixCsv(path, mat);
            return path;
        }
    }
}
