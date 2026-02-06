using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Utils
{
    public static class MatrixPrinter
    {
        /// <summary>
        /// Produce an aligned ASCII table of matrix values.
        /// </summary>
        /// <param name="M">Matrix to format (must be non-null).</param>
        /// <param name="precision">Number of significant digits (G format) or fractional digits when fixed=true.</param>
        /// <param name="useFixedPoint">If true uses fixed-point ("F") with <paramref name="precision"/> decimals; otherwise uses general ("G").</param>
        /// <param name="includeIndices">If true include row/column indices as headers.</param>
        /// <returns>Formatted string.</returns>
        public static string ToAlignedString(Matrix<double> M, int precision = 6, bool useFixedPoint = true, bool includeIndices = true)
        {
            if (M is null) throw new ArgumentNullException(nameof(M));
            int rows = M.RowCount;
            int cols = M.ColumnCount;

            // Format all values to strings first and compute max width per column
            string[,] strs = new string[rows, cols];
            int[] colWidth = new int[cols];
            string format = useFixedPoint ? $"F{precision}" : $"G{precision}";
            var ci = CultureInfo.InvariantCulture;

            for (int j = 0; j < cols; j++)
            {
                colWidth[j] = 0;
                for (int i = 0; i < rows; i++)
                {
                    double v = M[i, j];
                    string s;
                    if (double.IsNaN(v)) s = "NaN";
                    else if (double.IsPositiveInfinity(v)) s = "Inf";
                    else if (double.IsNegativeInfinity(v)) s = "-Inf";
                    else s = v.ToString(format, ci);
                    strs[i, j] = s;
                    if (s.Length > colWidth[j]) colWidth[j] = s.Length;
                }
                // ensure at least room for column index header if included
                if (includeIndices)
                {
                    string header = $"c{j}";
                    if (header.Length > colWidth[j]) colWidth[j] = header.Length;
                }
            }

            // compute width for row index column
            int rowIndexWidth = includeIndices ? Math.Max($"r{rows - 1}".Length, 2) : 0;

            var sb = new StringBuilder();

            // header line (column indices)
            if (includeIndices)
            {
                sb.Append(' ', rowIndexWidth);
                sb.Append(" | ");
                for (int j = 0; j < cols; j++)
                {
                    sb.Append($"{"c" + j}".PadLeft(colWidth[j]));
                    if (j < cols - 1) sb.Append(' ');
                }
                sb.AppendLine();
                // separator
                sb.Append(new string('-', rowIndexWidth));
                sb.Append("-+-");
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(new string('-', colWidth[j]));
                    if (j < cols - 1) sb.Append(' ');
                }
                sb.AppendLine();
            }

            // rows
            for (int i = 0; i < rows; i++)
            {
                if (includeIndices)
                {
                    sb.Append($"r{i}".PadLeft(rowIndexWidth));
                    sb.Append(" | ");
                }
                for (int j = 0; j < cols; j++)
                {
                    sb.Append(strs[i, j].PadLeft(colWidth[j]));
                    if (j < cols - 1) sb.Append(' ');
                }
                sb.AppendLine();
            }

            return sb.ToString();
        }

        /// <summary>
        /// Write an aligned matrix representation to the console.
        /// </summary>
        public static void PrintToConsole(Matrix<double> M, int precision = 6, bool useFixedPoint = true, bool includeIndices = true)
        {
            Console.WriteLine(ToAlignedString(M, precision, useFixedPoint, includeIndices));
        }

        /// <summary>
        /// Save the aligned representation to a UTF-8 text file.
        /// </summary>
        public static void SaveAsText(Matrix<double> M, string path, int precision = 6, bool useFixedPoint = false, bool includeIndices = true)
        {
            var content = ToAlignedString(M, precision, useFixedPoint, includeIndices);
            File.WriteAllText(path, content, Encoding.UTF8);
        }
    }
}
