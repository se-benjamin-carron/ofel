using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

public abstract class BaseTestWithResults
{
    // Static constructor runs once when the test assembly is loaded by the test runner.
    // It attempts to initialize (truncate) the shared results file so each run starts fresh
    // even when tests are launched directly via `dotnet test` (without the wrapper).
    static BaseTestWithResults()
    {
        try
        {
            var resultsDir = Path.GetDirectoryName(ResultsFilePath) ?? ".";
            if (!Directory.Exists(resultsDir)) Directory.CreateDirectory(resultsDir);

            // Try to create/truncate the file exclusively. If another process has it open, ignore.
            using (var fs = new FileStream(ResultsFilePath, FileMode.Create, FileAccess.Write, FileShare.None))
            using (var writer = new StreamWriter(fs, Encoding.UTF8))
            {
                writer.WriteLine($"--- Test run started at {DateTime.Now:o} ---");
                writer.Flush();
            }
        }
        catch (IOException)
        {
            // Could not obtain exclusive access to truncate the file; probably another runner
            // (or the wrapper) already initialized it. In that case we silently continue so tests
            // can still append safely.
        }
        catch
        {
            // Swallow any other errors to avoid failing test startup.
        }
    }

    protected static readonly string ResultsFilePath = @"c:\Users\bcarr\ofel\ofel\tests\Ofel.Tests\TestsResults.txt";

    protected void WriteResults(IEnumerable<string> expected, IEnumerable<string> calculated, string testName)
    {
        // Append results safely (file opened with FileShare.ReadWrite) and encoded as UTF8.
        using (var fs = new FileStream(ResultsFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
        using (var writer = new StreamWriter(fs, Encoding.UTF8))
        {
            writer.WriteLine($"==== {testName} ====");
            writer.WriteLine("Expected:");
            foreach (var line in expected)
                writer.WriteLine(line);
            writer.WriteLine("Calculated:");
            foreach (var line in calculated)
                writer.WriteLine(line);
            writer.WriteLine();
            writer.Flush();
        }
    }
}