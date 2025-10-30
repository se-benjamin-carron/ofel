using System;
using System.Linq;
using System.Collections.Generic;
using Xunit;

namespace Ofel.Tests
{
    public class MemberDirectorTests : BaseTestWithResults
    {
        [Fact]
        public void LineMemberDirector_CreatesLineMember_WithAssemblyAndSupports()
        {
            // Arrange
            var director = new ofel.Core.LineMemberDirector();
            var start = new ofel.Core.Point(0.0, 0.0, 0.0);
            var end = new ofel.Core.Point(3.0, 4.0, 0.0);
            var section = new ofel.Core.SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0, 0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6);
            var material = new ofel.Core.SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);

            // Act
            var member = director.CreateFromTwoPoints(start, end, section, material, true);

            // Compute main/all epsilons (without building intervals) so we can print them for debugging
            var mains = ofel.Core.MainEpsilonCalc.ComputeMainEpsilons(member);
            var alls = ofel.Core.AllEpsilonCalc.ComputeAllEpsilons(member, mains, 1.0);

            // Format lists for console and results file
            var mainLines = mains.Select(e => $"{e.Epsilon:0.######} [{e.Kind}]").ToArray();
            var allLines = alls.Select(e => $"{e.Epsilon:0.######} [{e.Kind}]").ToArray();

            // Print to console
            Console.WriteLine("--- LineMemberDirector MainEpsilons ---");
            foreach (var l in mainLines) Console.WriteLine(l);
            Console.WriteLine("--- LineMemberDirector AllEpsilons ---");
            foreach (var l in allLines) Console.WriteLine(l);

            // Append to the shared results file for offline inspection
            WriteResults(new string[] { "MainEpsilons:" }.Concat(mainLines), new string[] { "AllEpsilons:" }.Concat(allLines), "MemberDirectorTests.LineMemberDirector_CreatesLineMember");

            // Assert
            Assert.NotNull(member);
            Assert.Equal(2, member.PointsData.Count);
            // length should be 5 (3-4-5 triangle)
            Assert.True(Math.Abs(member.Length - 5.0) < 1e-9);

            var assemblies = member.GetCharacteristicsByKind("assembly").ToList();
            Assert.True(assemblies.Count >= 2, "Expected assembly characteristics at both ends");

            // write summary
            WriteResults(new string[] { "LineMemberDirector" }, new string[] { "OK" }, "MemberDirectorTests.LineMemberDirector_CreatesLineMember");
        }

        // [Fact]
        // public void CurveMemberDirector_CreatesMember_FromPointsList()
        // {
        //     // Arrange
        //     var director = new ofel.Core.CurveMemberDirector();
        //     var pts = new List<ofel.Core.Point>
        //     {
        //         new ofel.Core.Point(0,0,0),
        //         new ofel.Core.Point(1,0,0),
        //         new ofel.Core.Point(2,1,0)
        //     };
        //     var section = new ofel.Core.SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0, 0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6);
        //     var material = new ofel.Core.SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);

        //     // Act
        //     var member = director.CreateFromPointsList(pts, section, material, true);

        //     // Compute main/all epsilons (without building intervals) so we can print them for debugging
        //     var mains = ofel.Core.MainEpsilonCalc.ComputeMainEpsilons(member);
        //     var alls = ofel.Core.AllEpsilonCalc.ComputeAllEpsilons(member, mains, 0.5);

        //     var mainLines = mains.Select(e => $"{e.Epsilon:0.######} [{e.Kind}]").ToArray();
        //     var allLines = alls.Select(e => $"{e.Epsilon:0.######} [{e.Kind}]").ToArray();

        //     Console.WriteLine("--- CurveMemberDirector MainEpsilons ---");
        //     foreach (var l in mainLines) Console.WriteLine(l);
        //     Console.WriteLine("--- CurveMemberDirector AllEpsilons ---");
        //     foreach (var l in allLines) Console.WriteLine(l);

        //     WriteResults(new string[] { "MainEpsilons:" }.Concat(mainLines), new string[] { "AllEpsilons:" }.Concat(allLines), "MemberDirectorTests.CurveMemberDirector_CreatesMember");

        //     // Assert
        //     Assert.NotNull(member);
        //     Assert.Equal(3, member.PointsData.Count);

        //     var assemblies = member.GetCharacteristicsByKind("assembly").ToList();
        //     Assert.True(assemblies.Count >= 2, "Expected assembly characteristics at ends of curve member");

        //     WriteResults(new string[] { "CurveMemberDirector" }, new string[] { "OK" }, "MemberDirectorTests.CurveMemberDirector_CreatesMember");
        // }

        // [Fact]
        // public void LineMemberDirector_MainAllEpsilonsAndIntervals_AreCoherent()
        // {
        //     // Arrange
        //     var director = new ofel.Core.LineMemberDirector();
        //     var start = new ofel.Core.Point(0.0, 0.0, 0.0);
        //     var end = new ofel.Core.Point(4.0, 0.0, 0.0);
        //     var section = new ofel.Core.SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0, 0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6);
        //     var material = new ofel.Core.SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);
        //     var memberDomain = director.CreateFromTwoPoints(start, end, section, material, true);
        //     var fem = new ofel.Core.FiniteElementMember(memberDomain, 1.0f);

        //     // Act
        //     fem.Compute();

        //     // Assert main epsilons
        //     Assert.NotNull(fem.MainEpsilons);
        //     Assert.True(fem.MainEpsilons.Count >= 2);
        //     Assert.Equal(0.0, fem.MainEpsilons.First().Epsilon, 6);
        //     Assert.Equal(1.0, fem.MainEpsilons.Last().Epsilon, 6);

        //     // Assert all epsilons
        //     Assert.NotNull(fem.AllEpsilons);
        //     Assert.True(fem.AllEpsilons.Count >= fem.MainEpsilons.Count);
        //     Assert.Equal(0.0, fem.AllEpsilons.First().Epsilon, 6);
        //     Assert.Equal(1.0, fem.AllEpsilons.Last().Epsilon, 6);
        //     for (int i = 1; i < fem.AllEpsilons.Count; i++)
        //     {
        //         Assert.True(fem.AllEpsilons[i].Epsilon > fem.AllEpsilons[i - 1].Epsilon - 1e-9);
        //     }

        //     // Assert intervals
        //     Assert.NotNull(fem.Intervals);
        //     Assert.Equal(fem.AllEpsilons.Count - 1, fem.Intervals.Count);
        //     for (int i = 0; i < fem.Intervals.Count; i++)
        //     {
        //         var interval = fem.Intervals[i];
        //         var e1 = fem.AllEpsilons[i].Epsilon;
        //         var e2 = fem.AllEpsilons[i + 1].Epsilon;
        //         Assert.InRange(interval.Epsilon1, 0.0, 1.0);
        //         Assert.InRange(interval.Epsilon2, 0.0, 1.0);
        //         Assert.True(interval.Epsilon2 > interval.Epsilon1);
        //         // points correspond
        //         var p1 = fem.Member.GetInterpolatedPoint(interval.Epsilon1);
        //         var p2 = fem.Member.GetInterpolatedPoint(interval.Epsilon2);
        //         Assert.True(interval.Point1.DistanceTo(p1) < 1e-6);
        //         Assert.True(interval.Point2.DistanceTo(p2) < 1e-6);
        //     }

        //     WriteResults(new string[] { "LineMemberEpsilonsIntervals" }, new string[] { "OK" }, "MemberDirectorTests.LineMemberDirector_MainAllEpsilonsAndIntervals");
        // }

        // [Fact]
        // public void CurveMemberDirector_MainAllEpsilonsAndIntervals_AreCoherent()
        // {
        //     // Arrange
        //     var director = new ofel.Core.CurveMemberDirector();
        //     var pts = new List<ofel.Core.Point>
        //     {
        //         new ofel.Core.Point(0,0,0),
        //         new ofel.Core.Point(1,0,0),
        //         new ofel.Core.Point(2,1,0),
        //         new ofel.Core.Point(3,1,0)
        //     };
        //     var section = new ofel.Core.SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0, 0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6);
        //     var material = new ofel.Core.SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);
        //     var memberDomain = director.CreateFromPointsList(pts, section, material, true);
        //     var fem = new ofel.Core.FiniteElementMember(memberDomain, 0.5f);

        //     // Act
        //     fem.Compute();

        //     // Assert main epsilons
        //     Assert.NotNull(fem.MainEpsilons);
        //     Assert.True(fem.MainEpsilons.Count >= 2);
        //     Assert.Equal(0.0, fem.MainEpsilons.First().Epsilon, 6);
        //     Assert.Equal(1.0, fem.MainEpsilons.Last().Epsilon, 6);

        //     // Assert all epsilons and intervals
        //     Assert.NotNull(fem.AllEpsilons);
        //     Assert.NotNull(fem.Intervals);
        //     Assert.Equal(fem.AllEpsilons.Count - 1, fem.Intervals.Count);

        //     for (int i = 0; i < fem.Intervals.Count; i++)
        //     {
        //         var interval = fem.Intervals[i];
        //         var p1 = fem.Member.GetInterpolatedPoint(interval.Epsilon1);
        //         var p2 = fem.Member.GetInterpolatedPoint(interval.Epsilon2);
        //         Assert.True(interval.Point1.DistanceTo(p1) < 1e-6);
        //         Assert.True(interval.Point2.DistanceTo(p2) < 1e-6);
        //     }

        //     WriteResults(new string[] { "CurveMemberEpsilonsIntervals" }, new string[] { "OK" }, "MemberDirectorTests.CurveMemberDirector_MainAllEpsilonsAndIntervals");
        // }
    }
}
