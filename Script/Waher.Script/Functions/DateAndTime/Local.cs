using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.DateAndTime
{
    /// <summary>
    /// Local(x)
    /// </summary>
    public class Local : FunctionOneScalarVariable
    {
        /// <summary>
        /// Local(x)
        /// </summary>
        /// <param name="Argument">Argument.</param>
        /// <param name="Start">Start position in script expression.</param>
        /// <param name="Local">Local of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
        public Local(ScriptNode Argument, int Start, int Local, Expression Expression)
            : base(Argument, Start, Local, Expression)
        {
        }

        /// <summary>
        /// Name of the function
        /// </summary>
        public override string FunctionName => nameof(Local);

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
                return new DateTimeValue(TP.ToLocalTime());
			else if (Obj is System.DateTimeOffset TPO)
				return new ObjectValue(TPO.ToLocalTime());
			else
				throw new ScriptRuntimeException("Unable to convert to local date and time.", this);
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
