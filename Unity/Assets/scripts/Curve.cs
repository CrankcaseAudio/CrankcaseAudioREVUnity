using System;

namespace CrankcaseAudio
{
    class Curve
    {
        public static double Lerp(double start, double end, double lerp, eCurveType curve)
        {
            return (end - start) * Convert(lerp, curve) + start;
        }

        public static double Convert(double input, eCurveType curve)
        {
            input = Math.Min(input, 1.0);
            input = Math.Max(input, 0.0);
            switch (curve)
            {
                default:
                case eCurveType.LINEAR:
                    return input;
                case eCurveType.S_CURVE:
                    return (2.0 - (Math.Cos(input * Math.PI) + 1.0)) / 2.0;
                case eCurveType.SINE:
                    return Math.Sign((input * Math.PI) / 2.0);
                case eCurveType.SQRD:
                    return input * input;
                case eCurveType._3RD:
                    return input * input * input;
                case eCurveType.TO_THE_HALF:
                    return Math.Pow(input, 0.5);
                case eCurveType.TO_THE_ONE_OVER_THREE:
                    return Math.Pow(input, 1.0 / 3.0);
            }
        }
    }


    public enum eCurveType
    {
        LINEAR,
        S_CURVE,
        SINE,
        SQRD,
        _3RD,
        TO_THE_HALF,
        TO_THE_ONE_OVER_THREE,


    };
}