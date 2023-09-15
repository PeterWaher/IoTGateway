using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Minutes(x)
    /// </summary>
    public class Minute : FunctionOneScalarVariable
    {
        /// <summary>
        /// Minutes(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Minutes">Minutes of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Minute(ScriptNode Argument, int Start, int Minutes, Expression Expression)
            : base(Argument, Start, Minutes, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Minute);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "Minutes" };

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
			else if (Argument.AssociatedObjectValue is System.DateTimeOffset TPO)
				return new DoubleNumber(TPO.Minute);
			else if (Obj is System.TimeSpan TS)
				return new DoubleNumber(TS.TotalMinutes);
			else if (Argument.AssociatedObjectValue is IMinutes D)
				return new DoubleNumber(D.Minutes);
			else
				throw new ScriptRuntimeException("Unable to extract number of minutes.", this);
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override Task<IElement> EvaluateScalarAsync(IElement Argument, Variables Variables)
		{
			return Task.FromResult(this.EvaluateScalar(Argument, Variables));
		}
	}
}
