using System;
using System.Collections.Generic;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Fractals.IFS.Variations.ComplexVariations
{
    public class zASinVariation : FlameVariationZeroParameters
    {
        public zASinVariation(int Start, int Length, Expression Expression)
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