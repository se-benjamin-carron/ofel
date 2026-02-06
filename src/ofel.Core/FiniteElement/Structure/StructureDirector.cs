using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Ofel.Core;
using static Ofel.Core.Force;
using Ofel.Core.SectionParameter;



namespace Ofel.Core
{
    /// <summary>
    /// Builds small example structures programmatically using MemberDirector.
    /// </summary>
    public static class StructureDirector
    {

        public static FiniteElementStructure CreateCantileverBeam(
                double length = 2.0,
                SteelSection? section = null,
                SteelMaterial? material = null)
        {
            // fallback defaults when CSV data isn't available
            if (material == null)
            {
                // Prefer to load a real material from CSV (S235) if available; fall back to a light default.
                try
                {
                    material = SteelMaterial.GetByNameAndStandard("S235", "NF EN 10025-2");
                }
                catch
                {
                    // CSV missing or entry not found -> keep a safe in-code fallback
                    material = new SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);
                }
            }
            if (section == null)
            {
                // Prefer to load HEA120 from CSV if available; otherwise use a minimal placeholder section
                try
                {
                    section = SteelSection.GetByTypeAndName("HEA", "HEA120");
                }
                catch
                {
                    section = new SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0.0, 0.0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6, 0, 0, 0, 0);
                }
            }

            // Define the four corner points
            var member = new Member();

            var pointFirst = new Point(0.0, 0.0, 0.0);
            var pointSecond = new Point(length, 0.0, 0.0);
            MemberDirector director = new MemberDirector();
            director.SetPointsDataLinear(member, pointFirst, pointSecond, section, material);
            director.AddFixedSupport(member, 0.0);
            // Create members using the LineMemberDirector (returns domain Member objects).

            GlobalPunctualForce Fed = new GlobalPunctualForce(ForceKind.Wind, 1, new List<(double Epsilon, ForceValue Value)>
            {
                (1.0, new ForceValue(0.0, 0.0, 10000.0, 0.0, 0.0, 0.0))
            });
            director.AddForce(member, Fed);
            var femembers = new List<FiniteElementMember>
            {
                new FiniteElementMember(member),
            };

            var structure = new FiniteElementStructure(femembers);
            return structure;
        }

        public static FiniteElementStructure CreateDefaultRoof(
            double span = 8.0,
            double height = 2.5,
            SteelSection? section = null,
            SteelMaterial? material = null)
        {
            // fallback defaults when CSV data isn't available
            if (material == null)
            {
                // Prefer to load a real material from CSV (S235) if available; fall back to a light default.
                try
                {
                    material = SteelMaterial.GetByNameAndStandard("S235", "NF EN 10025-2");
                }
                catch
                {
                    // CSV missing or entry not found -> keep a safe in-code fallback
                    material = new SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);
                }
            }
            if (section == null)
            {
                // Prefer to load IPE200 from CSV if available; otherwise use a minimal placeholder section
                try
                {
                    section = SteelSection.GetByTypeAndName("IPE", "IPE200");
                }
                catch
                {
                    section = new SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0.0, 0.0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6, 0, 0, 0, 0);
                }
            }

            // Define the four corner points
            var Left = new Point(0.0, 0.0, 0.0);
            var top = new Point(span / 2, 0.0, height);
            var Right = new Point(span, 0.0, 0.0);

            MemberDirector director = new MemberDirector();

            var leftRoof = new Member();
            director.SetPointsDataLinear(leftRoof, Left, top, section, material);
            director.AddHingedSupport(leftRoof, 0.0, new DegreesOfFreedom(false, false, false, true, true, true));
            director.AddCharacteristic(leftRoof, new AssemblyChar(1.0));


            var rightRoof = new Member();
            director.SetPointsDataLinear(rightRoof, top, Right, section, material);
            director.AddHingedSupport(rightRoof, 1.0, new DegreesOfFreedom(false, false, false, false, true, true));
            director.AddCharacteristic(rightRoof, new AssemblyChar(0.0));
            var F_ed1 = new GlobalPunctualForce(ForceKind.Collision, 1, new List<(double Epsilon, ForceValue Value)>
            {
                (1.0, new ForceValue(0.0, 0.0, -5000.0, 0.0, 0.0, 0.0))
            });
            var F_ed2 = new GlobalPunctualForce(ForceKind.Wind, 1, new List<(double Epsilon, ForceValue Value)>
            {
                (1.0, new ForceValue(500.00, 0.0, 5000.0, 0.0, 0.0, 0.0))
            });

            director.AddForce(leftRoof, F_ed1);
            director.AddForce(leftRoof, F_ed2);
            var femembers = new List<FiniteElementMember>
            {
                new FiniteElementMember(leftRoof),
                new FiniteElementMember(rightRoof),
            };

            var structure = new FiniteElementStructure(femembers);
            return structure;
        }

        public static FiniteElementStructure CreateBeamLinearForce(
            double span = 8.0,
            double height = 2.5,
            SteelSection? section = null,
            SteelMaterial? material = null)
        {
            // fallback defaults when CSV data isn't available
            if (material == null)
            {
                // Prefer to load a real material from CSV (S235) if available; fall back to a light default.
                try
                {
                    material = SteelMaterial.GetByNameAndStandard("S235", "NF EN 10025-2");
                }
                catch
                {
                    // CSV missing or entry not found -> keep a safe in-code fallback
                    material = new SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);
                }
            }
            if (section == null)
            {
                // Prefer to load IPE200 from CSV if available; otherwise use a minimal placeholder section
                try
                {
                    section = SteelSection.GetByTypeAndName("IPE", "IPE200");
                }
                catch
                {
                    section = new SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0.0, 0.0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6, 0, 0, 0, 0);
                }
            }

            // Define the four corner points
            var Left = new Point(0.0, 0.0, height);
            var Right = new Point(span, 0.0, 0.0);

            // Create members using the LineMemberDirector (returns domain Member objects).
            var director = new MemberDirector();

            var beam = new Member();
            director.SetPointsDataLinear(beam, Left, Right, section, material);
            director.AddHingedSupport(beam, 0.2, new DegreesOfFreedom(false, false, false, true, true, true));
            director.AddHingedSupport(beam, 0.8, new DegreesOfFreedom(false, false, false, false, true, true));
            var F_ed1 = new LocalLinearForce(ForceKind.Collision, 1, new List<(double Epsilon, ForceValue Value)>
            {
                (0.0, new ForceValue(0.0, 0.0, -5000.0, 0.0, 0.0, 0.0)), (1.0, new ForceValue(0.0, 0.0, -5000.0, 0.0, 0.0, 0.0))
            });
            var F_ed2 = new GlobalLinearForce(ForceKind.Wind, 1, new List<(double Epsilon, ForceValue Value)>
            {
                (0.0, new ForceValue(0.0, 0.0, -5000.0, 0.0, 0.0, 0.0)), (1.0, new ForceValue(0.0, 0.0, -5000.0, 0.0, 0.0, 0.0))

            });
            director.AddForce(beam, F_ed1);
            director.AddForce(beam, F_ed2);
            director.AddForce(beam, new OwnWeight());
            var femembers = new List<FiniteElementMember>
            {
                new FiniteElementMember(beam),
            };

            var structure = new FiniteElementStructure(femembers);
            return structure;
        }

        public static FiniteElementStructure CreateHingedBeam(
            double length = 8.0,
            SteelSection? section = null,
            SteelMaterial? material = null)
        {
            // fallback defaults when CSV data isn't available
            if (material == null)
            {
                // Prefer to load a real material from CSV (S235) if available; fall back to a light default.
                try
                {
                    material = SteelMaterial.GetByNameAndStandard("S235", "NF EN 10025-2");
                }
                catch
                {
                    // CSV missing or entry not found -> keep a safe in-code fallback
                    material = new SteelMaterial("DefaultSteel", "STD", 355.0, 510.0, 210e9, 81e9, 7850.0, 1.2e-5);
                }
            }
            if (section == null)
            {
                // Prefer to load IPE200 from CSV if available; otherwise use a minimal placeholder section
                try
                {
                    section = SteelSection.GetByTypeAndName("IPE", "IPE200");
                }
                catch
                {
                    section = new SteelSection("GEN", "DEF", 0.1, 0.05, 0.005, 0.005, 0.0, 0.0, 0.001, 0.001, 0.001, 1e-6, 1e-6, 1e-6, 1e-6, 0, 0, 0, 0);
                }
            }

            // Define the four corner points
            var Left = new Point(0.0, 0.0, 0.0);
            var Right = new Point(0.0, 0.0, length);

            // Create members using the LineMemberDirector (returns domain Member objects).
            var director = new MemberDirector();
            var beam = new Member();
            director.SetPointsDataLinear(beam, Left, Right, section, material);
            director.AddHingedSupport(beam, 0.0);
            director.AddHingedSupport(beam, 1.0, new DegreesOfFreedom(false, false, false, true, true, false));
            LocalPunctualForce F_ed1 = new LocalPunctualForce(ForceKind.Collision, 1, new List<(double Epsilon, ForceValue Value)>
            {
                (0.5, new ForceValue(0.0, 0.0, -5000.0, 0.0, 0.0, 0.0))
            });
            LocalPunctualForce F_ed2 = new LocalPunctualForce(ForceKind.Wind, 1, new List<(double Epsilon, ForceValue Value)>
            {
                (0.75, new ForceValue(1000.0, 2500.0, 0.0, 0.0, 0.0, 0.0))
            });

            director.AddForce(beam, F_ed1);
            director.AddForce(beam, F_ed2);
            var femembers = new List<FiniteElementMember>
            {
                new FiniteElementMember(beam),
            };

            var structure = new FiniteElementStructure(femembers);
            return structure;
        }
    }
}
