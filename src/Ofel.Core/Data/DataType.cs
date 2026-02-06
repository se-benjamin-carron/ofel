using Ofel.Core.Load.Climatic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ofel.Core.Data
{
    public enum QuantityType
    {
        Undefined,
        Length,
        Area,
        Volume,
        Inertia,
        Speed,
        Percentage,
        Pressure,
        Mass,
        Force,
        Stress,
        Angle,
        Temperature,
        Time,
        Class,
        Moment,
        Text
    }
    public interface IDataType
    {
        QuantityType Quantity { get; }
        string? Name { get; }
        string? Reference { get; }
        string? Formula { get; }
        bool IsFrozen { get; }

        void Freeze();

    }
    public abstract class DataType<TSelf, TValue> : IDataType
        where TSelf : DataType<TSelf, TValue>
    {
        public TValue Value { get; }

        public string? Name { get; private set; }
        public string? Reference { get; private set; }
        public string? Formula { get; private set; }

        public QuantityType Quantity { get; }
        public bool IsFrozen { get; private set; }

        protected DataType(TValue value, QuantityType quantity)
        {
            Value = value;
            Quantity = quantity;
        }

        protected void EnsureNotFrozen()
        {
            if (IsFrozen)
                throw new InvalidOperationException("DataType is frozen.");
        }

        public TSelf SetName(string name)
        {
            EnsureNotFrozen();
            Name = name;
            return (TSelf)this;
        }

        public TSelf SetReference(string reference)
        {
            EnsureNotFrozen();
            Reference = reference;
            return (TSelf)this;
        }

        public TSelf SetFormula(string formula)
        {
            EnsureNotFrozen();
            Formula = formula;
            return (TSelf)this;
        }

        public void Freeze() => IsFrozen = true;

        public override string ToString()
        {
            return $"{Name ?? Quantity.ToString()} = {Value}"
                 + (Formula != null ? $" | Formula: {Formula}" : "")
                 + (Reference != null ? $" | Ref: {Reference}" : "");
        }
    }

    public static class DataTypeDecorators
    {
        public static T WithName<T>(this T data, string name)
             where T : DataType<T, object>
        {
            data.SetName(name);
            return data;
        }
    }

    public sealed class LengthData : DataType<LengthData, double>
    {
        public LengthData(double value)
            : base(value, QuantityType.Length) { }
    }

    public sealed class ResistanceData : DataType<ResistanceData, double>
    {
        public ResistanceData(double value)
            : base(value, QuantityType.Pressure) { }
    }
    public sealed class AreaData : DataType<AreaData, double>
    {
        public AreaData(double value)
            : base(value, QuantityType.Area) { }
    }

    public sealed class VolumeData : DataType<VolumeData, double>
    {
        public VolumeData(double value)
            : base(value, QuantityType.Volume) { }
    }
    public sealed class InertiaData : DataType<InertiaData, double>
    {
        public InertiaData(double value)
            : base(value, QuantityType.Inertia) { }
    }

    public sealed class SpeedData : DataType<SpeedData, double>
    {
        public SpeedData(double value)
            : base(value, QuantityType.Speed) { }
    }

    public sealed class PercentageData : DataType<PercentageData, double>
    {
        public PercentageData(double value)
            : base(value, QuantityType.Percentage) { }
    }
    public sealed class CoefficientData : DataType<CoefficientData, double>
    {
        public CoefficientData(double value)
            : base(value, QuantityType.Undefined) { }
    }
    public sealed class CoefficientArrayData : DataType<CoefficientArrayData, double[]>
    {
        public CoefficientArrayData(double[] value)
            : base(value, QuantityType.Undefined) { }
    }
    public sealed class PressureData : DataType<PressureData, double>
    {
        public PressureData(double value)
            : base(value, QuantityType.Pressure) { }
    }

    public sealed class AngleData : DataType<AngleData, double>
    {
        public AngleData(double value)
            : base(value, QuantityType.Angle) { }
    }

    public sealed class ForceData : DataType<ForceData, double>
    {
        public ForceData(double value)
            : base(value, QuantityType.Force) { }
    }

    public sealed class MomentData : DataType<MomentData, double>
    {
        public MomentData(double value)
            : base(value, QuantityType.Moment) { }
    }

    public sealed class TextData : DataType<TextData, string>
    {
        public TextData(string txt)
            : base(txt, QuantityType.Text) { }
    }

    public sealed class DataVerification : DataType<DataVerification, Verification>
    {
        public DataVerification(Verification value)
            : base(value, QuantityType.Class) { }

        public T Extract<T>(string name)
                where T : class, IDataType
        {
            var result = Value.Outputs.FirstOrDefault(o => o.Name == name);

            if (result is null)
                throw new KeyNotFoundException(
                    $"No output named '{name}' found in verification.");

            if (result is not T typed)
                throw new InvalidCastException(
                    $"Output '{name}' is of type {result.GetType().Name}, not {typeof(T).Name}.");

            return typed;
        }
    }
}
