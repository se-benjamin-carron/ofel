using System;
using System.Collections.Generic;
using System.Linq;

namespace ofel.Core
{
    /// <summary>
    /// Utility to compute the list of MainEpsilon for a member.
    /// Returns a fully-processed List&lt;MainEpsilon&gt; (sorted, expanded and cleaned).
    /// </summary>
    public static class MainEpsilonCalc
    {
        public static List<MainEpsilon> ComputeMainEpsilons(Member member)
        {
            if (member == null) throw new ArgumentNullException(nameof(member));

            var mains = new List<MainEpsilon>();
            foreach (var charac in member.Characteristics)
            {
                foreach (var main_e in charac.ToMainEpsilon()) mains.Add(main_e);
            }

            // include main_epsilons coming from forces (distributed/local punctual)
            foreach (var f in member.Forces)
            {
                foreach (var main_e in f.ToMainEpsilon()) mains.Add(main_e);
            }

            mains.Add(new MainEpsilon(0f, KindMainEpsilon.Start));
            mains.Add(new MainEpsilon(1f, KindMainEpsilon.End));

            if (!FiniteElementMember.IsValidMainEpsilonList(mains))
                throw new InvalidOperationException("Conflicting main epsilon kinds detected (e.g. natural hinge and natural spring at same position)");

            mains = GetSortedMainEpsilonList(mains);
            mains = GetAllMainEpsilons(mains);
            return mains;
        }


        public static List<MainEpsilon> GetSortedMainEpsilonList(IEnumerable<MainEpsilon> list)
        {
            return list.OrderBy(e => e.Epsilon).ToList();
        }

        public static List<MainEpsilon> GetAllMainEpsilons(List<MainEpsilon> mains)
        {
            // Iterate over a snapshot to avoid modifying the collection while enumerating.
            var snapshot = mains.ToList();
            var toAdd = new List<MainEpsilon>();
            foreach (var main_epsilon in snapshot)
            {
                // Get neighbouring epsilons for each main epsilon
                var neighbours = main_epsilon.GetNeighbouringEpsilons();
                foreach (var neighbour in neighbours)
                {
                    if (neighbour < 0f || neighbour > 1f) continue; // Skip out-of-bounds
                    if (!mains.Any(e => Math.Abs(e.Epsilon - neighbour) < 5e-7) && !toAdd.Any(e => Math.Abs(e.Epsilon - neighbour) < 5e-7))
                    {
                        // Queue a new MainEpsilon for addition
                        toAdd.Add(new MainEpsilon(neighbour, KindMainEpsilon.Default));
                    }
                }
            }
            if (toAdd.Count > 0) mains.AddRange(toAdd);
            // Return sorted list
            return mains.OrderBy(e => e.Epsilon).ToList();
        }
    }
}