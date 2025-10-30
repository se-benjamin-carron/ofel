namespace ofel.Core
{
    public static class IntervalDataCalc
    {
        public static List<IntervalData> ComputeAllIntervalDatas(Member member, List<MainEpsilon> allEpsilons)
        {
            List<IntervalData> intervals = new List<IntervalData>();
            var epsilons = allEpsilons.OrderBy(e => e.Epsilon).ToList();
            for (int i = 0; i < epsilons.Count - 1; i++)
            {
                double startEps = epsilons[i].Epsilon;
                double endEps = epsilons[i + 1].Epsilon;
                KindMainEpsilon startKind = epsilons[i].Kind;
                KindMainEpsilon endKind = epsilons[i + 1].Kind;
                Point startPoint = member.GetInterpolatedPoint(startEps);
                Point endPoint = member.GetInterpolatedPoint(endEps);
                IGeometry geometry = member.GetInterpolatedGeometry((float)((startEps + endEps) / 2.0));
                IsHinged hingedCondition = new IsHinged();
                Spring springData = new Spring();

                if (startKind == KindMainEpsilon.SpringChar && endKind == KindMainEpsilon.SpringChar)
                {
                    springData = member.getSpringDataFromEpsilon(startEps);
                }
                if (startKind == KindMainEpsilon.UnNaturalHingeChar)
                {
                    hingedCondition = member.getHingedConditionFromEpsilon(startEps);
                }
                if (endKind == KindMainEpsilon.NaturalHingeChar)
                {
                    hingedCondition = member.getHingedConditionFromEpsilon(endEps);
                }
                var interval = new IntervalData(startEps, endEps, startPoint, endPoint, geometry, member.Material, member.Roll, startKind, endKind,
                                                hingedCondition, springData);
                intervals.Add(interval);
            }
            return intervals;
        }
    }
}