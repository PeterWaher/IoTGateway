using System;
using System.Numerics;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class ZASinVariation : FlameVariationZeroParameters
    {
        public ZASinVariation(int Start, int Length, Expression Expression)
            : base(Start, Length, Expression)
        {
        }

        public override void Operate(ref double x, ref double y)
        {
            Complex z = new Complex(x, y);
            z = -Complex.Log(z * Complex.ImaginaryOne + Complex.Sqrt(1 - z * z)) * Complex.ImaginaryOne;

            x = z.Real;
            y = z.Imaginary;
        }

        public override string FunctionName
        {
            get { return "zASinVariation"; }
        }
    }
}