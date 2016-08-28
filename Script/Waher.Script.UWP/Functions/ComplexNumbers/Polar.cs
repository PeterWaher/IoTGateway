using System;
using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Abstraction.Sets;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.ComplexNumbers
{
    /// <summary>
    /// Polar(x,y)
    /// </summary>
    public class Polar : FunctionTwoScalarVariables
    {
        /// <summary>
        /// Polar(x,y)
        /// </summary>
        /// <param name="Argument1">Argument 1.</param>
        /// <param name="Argument2">Argument 2.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public Polar(ScriptNode Argument1, ScriptNode Argument2, int Start, int Length, Expression Expression)
            : base(Argument1, Argument2, Start, Length, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName
        {
            get { return "polar"; }
        }

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(double Argument1, double Argument2, Variables Variables)
        {
            return new ComplexNumber(Complex.FromPolarCoordinates(Argument1, Argument2));
        }

    }
}
