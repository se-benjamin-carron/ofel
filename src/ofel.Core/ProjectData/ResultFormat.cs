using System;
using System.Formats.Asn1;
using System.Text.RegularExpressions;
using MathNet.Numerics.LinearAlgebra;

namespace Ofel.Core
{

    public enum CombinationType
    {
        ULSFondamental,
        ULSAccidental,
        SLS,
        SLSQuasiPermanent,
        SLSFrequent,
    }

    public enum LoadingDuration
    {
        Permanent,
        LongTerm,
        MediumTerm,
        ShortTerm,
        Instantaneous
    }

    public class ForceResults
    {
        public double[] N { get; set; }
        public double[] Vy { get; set; }
        public double[] Vz { get; set; }
        public double[] Mx { get; set; }
        public double[] My { get; set; }
        public double[] Mz { get; set; }

        public ForceResults(double[] n, double[] vy, double[] vz, double[] mx, double[] my, double[] mz)
        {
            N = n ?? Array.Empty<double>();
            Vy = vy ?? Array.Empty<double>();
            Vz = vz ?? Array.Empty<double>();
            Mx = mx ?? Array.Empty<double>();
            My = my ?? Array.Empty<double>();
            Mz = mz ?? Array.Empty<double>();
        }
        public ForceResults()
        {
            N = Array.Empty<double>();
            Vy = Array.Empty<double>();
            Vz = Array.Empty<double>();
            Mx = Array.Empty<double>();
            My = Array.Empty<double>();
            Mz = Array.Empty<double>();
        }
    }

    public class DisplacementsResults
    {
        public double[] Ux { get; set; }
        public double[] Uy { get; set; }
        public double[] Uz { get; set; }
        public double[] Thetax { get; set; }
        public double[] Thetay { get; set; }
        public double[] Thetaz { get; set; }

        public DisplacementsResults(double[] ux, double[] uy, double[] uz, double[] thetax, double[] thetay, double[] thetaz)
        {
            Ux = ux ?? Array.Empty<double>();
            Uy = uy ?? Array.Empty<double>();
            Uz = uz ?? Array.Empty<double>();
            Thetax = thetax ?? Array.Empty<double>();
            Thetay = thetay ?? Array.Empty<double>();
            Thetaz = thetaz ?? Array.Empty<double>();
        }
        public DisplacementsResults()
        {
            Ux = Array.Empty<double>();
            Uy = Array.Empty<double>();
            Uz = Array.Empty<double>();
            Thetax = Array.Empty<double>();
            Thetay = Array.Empty<double>();
            Thetaz = Array.Empty<double>();
        }

    }

    public class CombinationsData
    {
        public ForceResults InternalEffortsCase { get; set; }
        public DisplacementsResults DisplacementsCase { get; set; }
        public DisplacementsResults? LocalDisplacementsCase { get; set; }
        public DisplacementsResults? DisplacementsPermanent { get; set; }
        public DisplacementsResults? LocalDisplacementsPermanent { get; set; }

        public CombinationsData(
            ForceResults internalEffortsCase,
            DisplacementsResults displacementsCase,
            DisplacementsResults? displacementsPermanent = null,
            DisplacementsResults? localDisplacementsCase = null,
            DisplacementsResults? localDisplacementsPermanent = null)
        {
            InternalEffortsCase = internalEffortsCase ?? new ForceResults();
            DisplacementsCase = displacementsCase ?? new DisplacementsResults();
            DisplacementsPermanent = displacementsPermanent;
            LocalDisplacementsCase = localDisplacementsCase;
            LocalDisplacementsPermanent = localDisplacementsPermanent;
        }

        // private LoadingDuration GetDuration(string loadingCase, double altitude = 0.0)
        // {
        //     if (loadingCase.Contains("W") || loadingCase.Contains("acc"))
        //         return LoadingDuration.Instantaneous;
        //     else if (loadingCase.Contains("S") && altitude < 1000.0)
        //         return LoadingDuration.ShortTerm;
        //     else if (loadingCase.Contains("S") && altitude >= 1000.0)
        //         return LoadingDuration.MediumTerm;
        //     else if (loadingCase.Contains("Q"))
        //         return LoadingDuration.LongTerm;
        //     else
        //         return LoadingDuration.Permanent;
        // }
    }

    /// <summary>
    /// Associates a normalized position (epsilon) with a Point and its Geometry.
    /// </summary>
    public class ResultFormat
    {
        public Dictionary<string, CombinationsData> UniqueCase { get; set; }
        public Dictionary<string, CombinationsData> Uls { get; set; }
        public Dictionary<string, CombinationsData> UlsAcc { get; set; }
        public Dictionary<string, CombinationsData> SlsCar { get; set; }
        public Dictionary<string, CombinationsData> SlsQp { get; set; }
        public Dictionary<string, CombinationsData> SlsFrequent { get; set; }

        public ResultFormat(Dictionary<string, CombinationsData> uniqueCase)
        {
            UniqueCase = uniqueCase;
            Uls = new Dictionary<string, CombinationsData>();
            UlsAcc = new Dictionary<string, CombinationsData>();
            SlsCar = new Dictionary<string, CombinationsData>();
            SlsQp = new Dictionary<string, CombinationsData>();
            SlsFrequent = new Dictionary<string, CombinationsData>();
        }

        public ResultFormat(Dictionary<string, CombinationsData> uls,
            Dictionary<string, CombinationsData> ulsAcc,
            Dictionary<string, CombinationsData> slsQp,
            Dictionary<string, CombinationsData> slsCc,
            Dictionary<string, CombinationsData> slsFrequent)
        {
            UniqueCase = new Dictionary<string, CombinationsData>();
            Uls = uls;
            UlsAcc = ulsAcc;
            SlsQp = slsQp;
            SlsCar = slsCc;
            //SlsRare = slsRare;
            SlsFrequent = slsFrequent;
        }
        public ResultFormat()
        {
            UniqueCase = new Dictionary<string, CombinationsData>();
            Uls = new Dictionary<string, CombinationsData>();
            UlsAcc = new Dictionary<string, CombinationsData>();
            SlsCar = new Dictionary<string, CombinationsData>();
            SlsQp = new Dictionary<string, CombinationsData>();
            //SlsRare = new Dictionary<string, CombinationsData>();
            SlsFrequent = new Dictionary<string, CombinationsData>();
        }
    }
}
