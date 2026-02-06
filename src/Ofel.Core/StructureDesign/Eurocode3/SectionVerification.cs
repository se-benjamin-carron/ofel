using MathNet.Numerics.Distributions;
using Ofel.Core.Data;
using Ofel.Core.SectionParameter;
using Ofel.Core.StructureDesign.Eurocode3;
using System;
using System.Xml;
using static Ofel.Core.StructureDesign.Eurocode3.SteelBucklingFactory;

namespace Ofel.Core.StructureDesign.Eurocode3
{
    public enum Axis
    {
        Y,
        Z
    }

    internal interface ISectionVerification
    {
        double Ratio { get; }
        double GetRatio();
    }

    public class SteelSectionVerificationULS : ISectionVerification
    {
        public SteelVerificationPure Pure { get; private set; }
        public SteelVerificationInstabilities Instabilities { get; private set; }

        public SteelSectionVerificationULS(
            SteelVerificationContext ctx,
            SteelVerificationInstabilitiesContext ctxtI)
        {
            Pure = new SteelVerificationPure(ctx);
            Instabilities = new SteelVerificationInstabilities(ctx, ctxtI, Pure);
        }

        public void ComputePure(SteelVerificationContext ctxt)
        {
            Pure = new SteelVerificationPure(ctxt);

        }
        public void ComputeInstabilities(SteelVerificationContext ctxt, SteelVerificationInstabilitiesContext ctxtI)
        {
            Instabilities = new SteelVerificationInstabilities(ctxt, ctxtI, Pure);
        }
        public double Ratio => GetRatio();
        public double GetRatio()
        {
            return Math.Max(
                Pure.GetRatio(),
                Instabilities.GetRatio()
            );
        }
    }
    public sealed class SteelVerificationContext
    {
        public ForceValue Effort { get; }
        public SteelSection Section { get; }
        public SteelMaterial Material { get; }
        public SteelResistanceCoefficient Coef { get; }
        public ClassSection SectionClass { get; }

        public SteelVerificationContext(
            ForceValue effort,
            SteelSection section,
            SteelMaterial material,
            SteelResistanceCoefficient coef)
        {
            Effort = effort;
            Section = section;
            Material = material;
            Coef = coef;

            SectionClass = ClassSectionFactory.Create(
                new ClassSectionInput(section, material, effort.Fx)
            );
        }
    }
    public sealed class SteelVerificationInstabilitiesContext
    {
        public required IntervalBuckling IntervalBY { get; init; }
        public required IntervalBuckling IntervalBZ { get; init; }
        public required IntervalBuckling IntervalBT { get; init; }
        public required IntervalLateralBuckling IntervalLT { get; init; }
        public required FlexionCoefficient CoefficientC1C2 { get; init; } = FlexionCoefficient.CreateFromLinearShape(1.0);
        public required ShapeCoefficient Cmi0 { get; init; } = ShapeCoefficient.CreateDefault();
        public required StabilisingForceType IsStabilisingForce { get; init; } = StabilisingForceType.Neutral;
        public required LateralBucklingType LateralBucklingType { get; init; } = LateralBucklingType.Default;
        public required InteractionInstabilitiesType InteractionType { get; init; } = InteractionInstabilitiesType.AnnexA;
    }

    public sealed class SteelVerificationPure
    {
        public SteelTorsion Torsion { get; }
        public SteelShear ShearY { get; }
        public SteelShear ShearZ { get; }
        public double ReductionCoef { get; }

        public SteelNormal Normal { get; }
        public SteelFlexion FlexionY { get; }
        public SteelFlexion FlexionZ { get; }
        public SteelInteraction Interaction { get; }

        public SteelVerificationPure(SteelVerificationContext ctxt)
        {
            Torsion = SteelTorsionFactory.Create(
                 new SteelTorsionInput() { Context = ctxt }
            );

            ShearY = SteelShearFactory.Create(
                new SteelShearInput() { Context = ctxt, Axis = Axis.Y, Torsion = Torsion }
            );

            ShearZ = SteelShearFactory.Create(
                new SteelShearInput() { Context = ctxt, Axis = Axis.Z, Torsion = Torsion }
            );
            ReductionCoef = GetReductionCoef();

            Normal = SteelNormalFactory.Create(
                new SteelNormalInput() { Context = ctxt, ReductionCoef = ReductionCoef }
            );

            FlexionY = SteelFlexionFactory.Create(
                new SteelFlexionInput() { Context = ctxt, Axis = Axis.Y, ReductionCoef = ReductionCoef }
            );

            FlexionZ = SteelFlexionFactory.Create(
                new SteelFlexionInput() { Context = ctxt, Axis = Axis.Z, ReductionCoef = ReductionCoef }
            );

            Interaction = SteelInteractionFactory.Create(
                new SteelInteractionInput() { Context = ctxt, FlexionY = FlexionY, FlexionZ = FlexionZ, Normal = Normal }
            );
        }
        public double GetRatio()
        {
            return new[]
            {
                Torsion.Ratio,
                ShearY.Ratio,
                ShearZ.Ratio,
                Normal.Ratio,
                FlexionY.Ratio,
                FlexionZ.Ratio,
                Interaction.Ratio
            }.Max();
        }

        public double GetReductionCoef()
        {
            return (1 - ShearY.Rho) * (1 - ShearZ.Rho);
        }
    }

    public sealed class SteelVerificationInstabilities
    {
        public SteelBuckling BucklingY { get; }
        public SteelBuckling BucklingZ { get; }
        public SteelBucklingTorsion BucklingTorsion { get; }
        public SteelLateralBuckling LateralBuckling { get; }
        public SteelInteractionInstabilities Interaction { get; }

        public SteelVerificationInstabilities(
            SteelVerificationContext ctxt,
            SteelVerificationInstabilitiesContext ctxtI,
            SteelVerificationPure pure)
        {
            BucklingY = SteelBucklingFactory.Create(
                new SteelBucklingDefaultInput() { Context = ctxt, Axis = Axis.Y, Interval = ctxtI.IntervalBY }
            );

            BucklingZ = SteelBucklingFactory.Create(
                new SteelBucklingDefaultInput() { Context = ctxt, Axis = Axis.Z, Interval = ctxtI.IntervalBZ }
            );
            var bucklingTorsionInput = new SteelBucklingTorsionInput()
            {
                Context = ctxt,
                Interval = ctxtI.IntervalBT,
                BucklingY = BucklingY,
                BucklingZ = BucklingZ
            };
            BucklingTorsion = new SteelBucklingTorsion(bucklingTorsionInput);

            LateralBuckling = SteelLateralBucklingFactory.Create(
                new SteelLateralBucklingInput() { Context = ctxt, Interval = ctxtI.IntervalLT, CoefficientC1C2 = ctxtI.CoefficientC1C2, IsStabilisingForce = ctxtI.IsStabilisingForce, Type = ctxtI.LateralBucklingType }
            );

            Interaction = SteelInteractionInstabilitiesFactory.Create(
                new SteelInteractionInstabilitiesInput() { Context = ctxt, BucklingY = BucklingY, BucklingZ = BucklingZ, Cmi0 = ctxtI.Cmi0, CoefficientC1C2 = ctxtI.CoefficientC1C2, FlexionY = pure.FlexionY, FlexionZ = pure.FlexionZ, LateralBuckling = LateralBuckling, Normal = pure.Normal, Torsion = BucklingTorsion, Type = ctxtI.InteractionType }
            );
        }
        public double GetRatio()
        {
            return new[]
            {
                BucklingY.Ratio,
                BucklingZ.Ratio,
                BucklingTorsion.Ratio,
                LateralBuckling.Ratio,
                Interaction.Ratio
            }.Max();
        }

    }

}
