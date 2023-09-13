using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Seconds(x)
    /// </summary>
    public class Second : FunctionOneScalarVariable
    {
        /// <summary>
        /// Seconds(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Seconds">Seconds of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Second(ScriptNode Argument, int Start, int Seconds, Expression Expression)
            : base(Argument, Start, Seconds, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Second);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "Seconds" };

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
                return new DoubleNumber(TP.Second);
			else if (Obj is System.TimeSpan TS)
				return new DoubleNumber(TS.TotalSeconds);
			else if (Argument.AssociatedObjectValue is ISeconds D)
				return new DoubleNumber(D.Seconds);
			else
				throw new ScriptRuntimeException("Unable to extract number of seconds.", this);
		}
	}
}
