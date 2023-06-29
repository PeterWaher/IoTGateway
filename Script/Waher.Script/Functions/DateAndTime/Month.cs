using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Month(x)
    /// </summary>
    public class Month : FunctionOneScalarVariable
    {
        /// <summary>
        /// Month(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Month">Month of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Month(ScriptNode Argument, int Start, int Month, Expression Expression)
            : base(Argument, Start, Month, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Month);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
            if (Argument.AssociatedObjectValue is System.DateTime TP)
                return new DoubleNumber(TP.Month);
            else
                throw new ScriptRuntimeException("Expected Date and Time value.", this);
		}
	}
}
