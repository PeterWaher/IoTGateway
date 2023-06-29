using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Day(x)
    /// </summary>
    public class Day : FunctionOneScalarVariable
    {
        /// <summary>
        /// Day(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Day">Day of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Day(ScriptNode Argument, int Start, int Day, Expression Expression)
            : base(Argument, Start, Day, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Day);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
            if (Argument.AssociatedObjectValue is System.DateTime TP)
                return new DoubleNumber(TP.Day);
            else
                throw new ScriptRuntimeException("Expected Date and Time value.", this);
		}
	}
}
