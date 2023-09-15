using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Utc(x)
    /// </summary>
    public class Utc : FunctionOneScalarVariable
    {
        /// <summary>
        /// Utc(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Utc">Utc of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Utc(ScriptNode Argument, int Start, int Utc, Expression Expression)
            : base(Argument, Start, Utc, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Utc);

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
                return new DateTimeValue(TP.ToUniversalTime());
			else if (Obj is System.DateTimeOffset TPO)
				return new ObjectValue(TPO.ToUniversalTime());
			else
				throw new ScriptRuntimeException("Unable to convert to universal time coordinates.", this);
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
