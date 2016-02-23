using System.Numerics;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
    /// <summary>
    /// Base class for funcions of one numeric variable.
    /// </summary>
    public abstract class FunctionOneNumericVariable : FunctionOneDoubleVariable 
	{
        /// <summary>
        /// Base class for funcions of one numeric variable.
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Length">Length of expression covered by node.</param>
        public FunctionOneNumericVariable(ScriptNode Argument, int Start, int Length)
			: base(Argument, Start, Length)
		{
		}

        /// <summary>
        /// Evaluates the function on a scalar argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public override IElement EvaluateScalar(IElement Argument, Variables Variables)
        {
            DoubleNumber D = Argument as DoubleNumber;
            ComplexNumber C;

            if (D != null)
                return this.EvaluateScalar(D.Value, Variables);
            else if ((C = Argument as ComplexNumber) != null)
                return this.EvaluateScalar(C.Value, Variables);
            else
                throw new ScriptRuntimeException("Expected a numeric argument.", this);
        }

        /// <summary>
        /// Evaluates the function on a complex argument.
        /// </summary>
        /// <param name="Argument">Function argument.</param>
        /// <param name="Variables">Variables collection.</param>
        /// <returns>Function result.</returns>
        public abstract IElement EvaluateScalar(Complex Argument, Variables Variables);

    }
}
