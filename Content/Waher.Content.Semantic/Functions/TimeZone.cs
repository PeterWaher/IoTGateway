using System;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// TimeZone(x)
	/// </summary>
	public class TimeZone : FunctionOneSemanticVariable
	{
		/// <summary>
		/// TimeZone(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public TimeZone(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(TimeZone);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(ISemanticElement Argument, Variables Variables)
		{
			if (Argument.AssociatedObjectValue is DateTimeOffset TP)
				return new DayTimeDurationLiteral(Duration.FromTimeSpan(TP.Offset));
			else
				throw new ScriptRuntimeException("Expected a Date and Time value.", this);
		}
	}
}
