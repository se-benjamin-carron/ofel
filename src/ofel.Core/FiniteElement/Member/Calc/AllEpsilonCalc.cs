using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;



namespace Ofel.Core
{
    /// <summary>
    /// Utility to compute the list of MainEpsilon for a member.
    /// Returns a fully-processed List&lt;MainEpsilon&gt; (sorted, expanded and cleaned).
    /// </summary>
    public static class AllEpsilonCalc
    {
        public static List<MainEpsilon> ComputeAllEpsilons(Member member, List<MainEpsilon> mainEpsilons, double mesh, int minVals = 2, int maxVals = 16)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));
            if (mainEpsilons == null) throw new ArgumentNullException(nameof(mainEpsilons));
            if (mesh <= 0.0) throw new ArgumentOutOfRangeException(nameof(mesh), "mesh must be > 0");

            var all = CleanAllMainEpsilons(mainEpsilons)
                      .OrderBy(me => me.Epsilon)
                      .ToList();
            int sizeMain = all.Count;
            double memberLength = member.GetMemberLength();

            for (int i = 0; i < sizeMain - 1; i++)
            {
                var a = all[i];
                var b = all[i + 1];
                double interval = (b.Epsilon - a.Epsilon);
                double intervalLength = interval * memberLength;
                IGeometry section = member.GetInterpolatedGeometry(a.Epsilon);
                int desired = Math.Max(1, (int)Math.Round(intervalLength / mesh));

                int n;
                if (intervalLength < 0.5)
                {
                    n = Math.Min(Math.Max(desired, 1), maxVals);
                }
                else
                {
                    n = Math.Min(Math.Max(desired, minVals), maxVals);
                }
                double step = interval / n;

                for (int k = 1; k < n; k++)
                {
                    double val = a.Epsilon + k * step;
                    if (val < 0.0) val = 0.0;
                    if (val > 1.0) val = 1.0;
                    var main = new MainEpsilon(val, KindMainEpsilon.Default);
                    main.SetGeometry(section);
                    main.SetMaterial(member.Material);
                    all.Add(main);
                }
            }

            return all.OrderBy(me => me.Epsilon).ToList();
        }

        public static List<MainEpsilon> CleanAllMainEpsilons(IEnumerable<MainEpsilon> mains)
        {
            // Grouper par Epsilon (tolérance pour flottants)
            const double tol = 5e-7;
            var grouped = mains
                .GroupBy(m => mains.FirstOrDefault(x => Math.Abs(x.Epsilon - m.Epsilon) < tol)?.Epsilon ?? m.Epsilon)
                .ToList();

            var cleaned = new List<MainEpsilon>();

            foreach (var group in grouped)
            {
                // Look for a SpringChar, NaturalHingeChar or UnNaturalHingeChar in this group
                var special = group.FirstOrDefault(m =>
                    m.Kind == KindMainEpsilon.SpringChar ||
                    m.Kind == KindMainEpsilon.NaturalHingeChar ||
                    m.Kind == KindMainEpsilon.UnNaturalHingeChar);

                if (special != null)
                {
                    // If a SpringChar, double it
                    if (special.Kind == KindMainEpsilon.SpringChar)
                    {
                        cleaned.Add(special);
                        cleaned.Add(new MainEpsilon(special.Epsilon, special.Kind));
                    }
                    else
                    {
                        cleaned.Add(special);
                    }
                }
                else
                {
                    // If no special kind, keep only one MainEpsilon (set to Default)
                    var m = group.First();
                    cleaned.Add(new MainEpsilon(m.Epsilon, KindMainEpsilon.Default));
                }
            }

            return cleaned;
        }
    }
}
