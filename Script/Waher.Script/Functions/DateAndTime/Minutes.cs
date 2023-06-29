using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Minutes(x)
    /// </summary>
    public class Minutes : FunctionOneScalarVariable
    {
        /// <summary>
        /// Minutes(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Minutes">Minutes of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Minutes(ScriptNode Argument, int Start, int Minutes, Expression Expression)
            : base(Argument, Start, Minutes, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Minutes);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
            object Obj = Argument.AssociatedObjectValue;

            if (Obj is System.DateTime TP)
                return new DoubleNumber(TP.Minute);
			else if (Obj is System.TimeSpan TS)
				return new DoubleNumber(TS.TotalMinutes);
			else
				throw new ScriptRuntimeException("Expected Date and Time or TimeSpan value.", this);
		}
	}
}
