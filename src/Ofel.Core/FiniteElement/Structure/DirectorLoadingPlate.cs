using MathNet.Numerics;
using Ofel.Core.Load;
using Ofel.Core.SectionParameter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.FiniteElement.Structure
{
    public class LoadingPlatePurlinBuilder
    {
        public Member Create(LoadingPlate plate, double position, List<Force> forces)
        {
            Member purlin = new Member();
            MemberDirector director = new MemberDirector();
            Point pStart = plate.Corners[0].Interpolate(plate.Corners[3], position);
            Point pEnd = plate.Corners[1].Interpolate(plate.Corners[2], position);

            SteelMaterial defaultMaterial = SteelMaterial.GetByNameAndStandard("S235", "NF EN 10025-2");
            SteelSection defaultGeometry = SteelSection.GetByTypeAndName("HEA", "HEA120");

            director.SetPointsDataLinear(purlin, pStart, pEnd, defaultGeometry, defaultMaterial);
            director.SetCharacteristics(purlin, AllAssemblyPurlin(plate.GantriesPositions));
            director.SetForces(purlin, forces);
            director.SetRoll(purlin, plate.Angle);
            return purlin;
        }
        public IEnumerable<Characteristic> AllAssemblyPurlin(double[] positions)
        {
            return positions.Select(p => (Characteristic)new AssemblyChar(p)).ToList();
        }
    }

    public class LoadingPlateWidthMemberBuilder
    {
        public Member Create(LoadingPlate plate, double length, double angle)
        {
            Member width = new Member();
            MemberDirector director = new MemberDirector();
            Point ptStart = new Point(0, 0, 0);
            Point ptEnd = new Point(length * Math.Cos(plate.Angle), 0, length * Math.Sin(plate.Angle));
            SteelMaterial defaultMaterial = SteelMaterial.GetByNameAndStandard("S235", "NF EN 10025-2");
            SteelSection defaultGeometry = SteelSection.GetByTypeAndName("HEA", "HEA120");
            director.SetPointsDataLinear(width, ptStart, ptEnd, defaultGeometry, defaultMaterial);
            director.SetCharacteristics(width, AllSupportWidthPurlin(plate.PurlinsPositions));
            return width;
        }

        public Point ComputeEndPoint(Point start, double length, double angle)
        {
            double radAngle = angle * (Math.PI / 180.0);
            double x = start.X + length * Math.Cos(radAngle);
            double z = start.Z + length * Math.Sin(radAngle);
            return new Point(x, start.Y, z);
        }

        public IEnumerable<Characteristic> AllSupportWidthPurlin(double[] positions)
        {
            DegreesOfFreedom dof = new DegreesOfFreedom(false, false, false, true, true, true);
            DegreesOfFreedom dof1 = new DegreesOfFreedom(false, false, false, false, true, true);
            if (positions.Length < 2)
                throw new ArgumentException("Il doit y avoir au moins deux positions", nameof(positions));
            return positions
                .Select((p, index) => (Characteristic)new SupportChar(p, index == 0 ? dof1 : dof))
                .ToList();
        }
    }
}
