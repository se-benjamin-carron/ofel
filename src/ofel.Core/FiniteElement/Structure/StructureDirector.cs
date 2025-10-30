using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using ofel.Core;

namespace ofel.Core
{
    /// <summary>
    /// Builds small example structures programmatically using MemberDirector.
    /// </summary>
    public static class StructureDirector
    {
        /// <summary>
        /// Creates a simple 4-member rectangular frame (left column, right column, top beam, bottom beam).
        /// - span: distance between left and right supports (X axis)
        /// - height: column height (Y axis)
        /// - mesh: element length for finite element meshing
        /// If section or material are null, lightweight defaults will be created.
        /// </summary>
        public static FiniteElementStructure CreateDefault4MemberFrame(
            double span = 8.0,
            double height = 2.5,
            double mesh = 0.25,
            SteelSection? section = null,
            SteelMaterial? material = null,
            BuildingUse use = BuildingUse.RoofOnly)
        {
            // fallback defaults when CSV data isn't available
            if (material == null)
            {
                material = new SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);
            }
            if (section == null)
            {
                // minimal placeholder section
                section = new SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0.0, 0.0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6);
            }

            // Define the four corner points
            var bottomLeft = new Point(0.0, 0.0, 0.0);
            var top = new Point(0.0, 0.0, height);
            var bottomMiddle = new Point(span / 2, 0.0, 0.0);
            var bottomRight = new Point(span, 0.0, 0.0);

            // Create members using the LineMemberDirector (returns domain Member objects).
            var director = new LineMemberDirector();
            Member leftRoof = director.CreateFromTwoPoints(bottomLeft, top, section, material, true).WithHingedSupport(0.0);
            Member rightRoof = director.CreateFromTwoPoints(bottomRight, top, section, material, false).WithHingedSupport(0.0);
            Member Truss = director.CreateFromTwoPoints(top, bottomMiddle, section, material, false);
            Member bottomBeam = director.CreateFromTwoPoints(bottomLeft, bottomRight, section, material, false);

            var femembers = new List<FiniteElementMember>
            {
                new FiniteElementMember(leftRoof),
                new FiniteElementMember(rightRoof),
                new FiniteElementMember(Truss),
                new FiniteElementMember(bottomBeam)
            };

            var structure = new FiniteElementStructure(femembers, mesh, use);
            return structure;
        }
    }
}
