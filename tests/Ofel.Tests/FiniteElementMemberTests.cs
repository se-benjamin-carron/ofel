// using System;
// using System.Collections.Generic;
// using System.Linq;
// using Xunit;
// using ofel.Core;
// using System.Numerics;
// using System.Reflection.PortableExecutable;

// namespace Ofel.Tests
// {
//     public class FiniteElementMemberTests
//     {
//         // Helper: create a straight member from (0,0,0) to (1,0,0) with mesh=0.5
//         private FiniteElementMember CreateSimpleMember()
//         {
//             var points = new SortedDictionary<float, Point>
//             {
//                 { 0, new Point(1, 0f, 0f, 0f) },
//                 { 0, new Point(1, 1f, 0f, 0f) }
//             };
//             var geometry = new SteelSection("IPE", "IPE100", 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 0f, 1f);
//             var material = new SteelMaterial("Test", "Std", 200e9, 250e9, 210e9, 80e9, 7800);
//             var member = new Member(points, geometry, material);
//             return new FiniteElementMember(member, 0.5f);
//         }

//         [Fact]
//         public void ComputeMainEpsilons_ShouldInclude0And1()
//         {
//             var fem = CreateSimpleMember();
//             fem.Compute();
//             var mains = fem.MainEpsilons;
//             Assert.Contains(0f, mains);
//             Assert.Contains(1f, mains);
//         }

//         [Fact]
//         public void ComputeAllEpsilons_ShouldSubdivideIntervals()
//         {
//             var fem = CreateSimpleMember();
//             fem.Compute();
//             var all = fem.AllEpsilons;
//             // mesh=0.5 gives two intervals: [0,0.5],[0.5,1]
//             Assert.Contains(0f, all);
//             Assert.Contains(0.5f, all);
//             Assert.Contains(1f, all);
//             // should be exactly 3 points
//             Assert.Equal(3, all.Count);
//         }

//         [Fact]
//         public void ComputeAngles_StraightLine_ShouldBeZero()
//         {
//             var fem = CreateSimpleMember();
//             fem.Compute();
//             var angles = fem.Angles;
//             // segments along x-axis yield angles 0 to X-axis
//             foreach (var v in angles)
//             {
//                 Assert.Equal(0f, v.X, precision: 3);
//                 Assert.Equal((float)(Math.PI / 2), v.Y, precision: 3);
//                 Assert.Equal((float)(Math.PI / 2), v.Z, precision: 3);
//             }
//         }
//         [Fact]
//         public void CreateFiniteElementMember()
//         {
//             // Arrange
//             var p1 = new Point(1, 0, 0, 0);
//             var p2 = new Point(1, 0, 0, 4);
//             var points = new SortedDictionary<float, Point> { [0] = p1, [1] = p2 };
//             var material = SteelMaterial.GetByNameAndStandard("S235", "EN 10025-2");
//             var section = SteelSection.GetByTypeAndName("IPE", "IPE200");

//             var member = new Member(
//                 points,
//                 section,
//                 material
//             );

//             // Act: Ajoute une caractéristique
//             ICharacteristic assemblyChar = new AssemblyChar(0.5f);
//             member.AddCharacteristic(assemblyChar);

//             var fem = new FiniteElementMember(member, mesh: 1.0f);
//             fem.Compute();
//             // Assert: Vérifie que la caractéristique a bien été ajoutée
//             Assert.Contains(assemblyChar, member.Characteristics);

//             // Affiche les epsilons dans la sortie de test (pour debug)
//             System.Diagnostics.Debug.WriteLine($"AllEpsilons: {string.Join(", ", fem.AllEpsilons)}");
//             System.Diagnostics.Debug.WriteLine($"MainEpsilons: {string.Join(", ", fem.MainEpsilons)}");
//         }
//     }
// }