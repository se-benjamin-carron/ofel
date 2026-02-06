namespace Ofel.Core.Data
{
    public class Verification
    {
        public string Name { get; set; } = string.Empty;
        public IReadOnlyList<IDataType> Inputs { get; }
        public IReadOnlyList<IDataType> Outputs { get; }
        public PercentageData Ratio { get; }

        protected Verification(
            IEnumerable<IDataType> inputs,
            IEnumerable<IDataType> outputs,
            PercentageData? ratio = null)
        {
            var inList = inputs.ToList();
            var outList = outputs.ToList();

            foreach (var d in inList.Concat(outList))
                d.Freeze();

            Inputs = inList.AsReadOnly();
            Outputs = outList.AsReadOnly();
            Ratio = ratio ?? new PercentageData(0);
        }

        public static Verification Create(
            string name,
            IEnumerable<IDataType> inputs,
            IEnumerable<IDataType> outputs,
            PercentageData? ratio = null)
        {
            return new Verification(inputs, outputs, ratio)
            {
                Name = name
            };
        }
    }

    public interface IVerifiable
    {
        Verification ToVerification();
    }
}
