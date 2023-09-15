using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Threading.Functions
{
	/// <summary>
	/// Aborts the background processing of script
	/// </summary>
	public class Abort : FunctionOneScalarVariable
	{
		/// <summary>
		/// Aborts the background processing of script
		/// </summary>
		/// <param name="Milliseconds">Number of milliseconds to sleep.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Abort(ScriptNode Milliseconds, int Start, int Length, Expression Expression)
			: base(Milliseconds, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Abort);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "TaskId" };

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(IElement Argument, Variables Variables)
		{
			if (Argument.AssociatedObjectValue is Guid Guid)
				return new BooleanValue(Background.AbortBackgroundTask(Guid));
			else if (Argument.AssociatedObjectValue is string s)
				return this.EvaluateScalar(s, Variables);
			else
				throw new ScriptRuntimeException("Expected a Task ID.", this);
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

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			if (Guid.TryParse(Argument, out Guid TaskId))
				return new BooleanValue(Background.AbortBackgroundTask(TaskId));
			else
				throw new ScriptRuntimeException("Expected a Task ID.", this);
		}
	}
}
