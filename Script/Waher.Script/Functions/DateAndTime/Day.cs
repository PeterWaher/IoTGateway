using System.Threading.Tasks;
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
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases => new string[] { "Days" };

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
                return new DoubleNumber(TP.Day);
			else if (Argument.AssociatedObjectValue is System.DateTimeOffset TPO)
				return new DoubleNumber(TPO.Day);
			else if (Obj is System.TimeSpan TS)
				return new DoubleNumber(TS.TotalDays);
			else if (Argument.AssociatedObjectValue is IDays D)
				return new DoubleNumber(D.Days);
			else
				throw new ScriptRuntimeException("Unable to extract number of days.", this);
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
