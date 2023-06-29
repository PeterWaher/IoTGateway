using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Year(x)
    /// </summary>
    public class Year : FunctionOneScalarVariable
    {
        /// <summary>
        /// Year(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Year">Year of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Year(ScriptNode Argument, int Start, int Year, Expression Expression)
            : base(Argument, Start, Year, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Year);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
            if (Argument.AssociatedObjectValue is System.DateTime TP)
                return new DoubleNumber(TP.Year);
            else
                throw new ScriptRuntimeException("Expected Date and Time value.", this);
		}
	}
}
