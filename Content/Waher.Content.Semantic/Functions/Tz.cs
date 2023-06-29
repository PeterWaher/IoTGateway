using System;
using System.Text;
using Waher.Content.Semantic.Model.Literals;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;

namespace Waher.Content.Semantic.Functions
{
	/// <summary>
	/// Tz(x)
	/// </summary>
	public class Tz : FunctionOneSemanticVariable
	{
		/// <summary>
		/// Tz(x)
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Tz(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Tz);

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(ISemanticElement Argument, Variables Variables)
		{
			if (Argument.AssociatedObjectValue is DateTimeOffset TPO)
			{
				if (TPO.Offset == TimeSpan.Zero)
					return new StringLiteral("Z");
				else
				{
					StringBuilder sb = new StringBuilder();

					if (TPO.Offset < TimeSpan.Zero)
						sb.Append('-');

					sb.Append(TPO.Offset.Hours.ToString("d2"));
					sb.Append(':');
					sb.Append(TPO.Offset.Minutes.ToString("d2"));

					return new StringLiteral(sb.ToString());
				}
			}
			else if (Argument.AssociatedObjectValue is DateTime)
				return new StringLiteral(string.Empty);
			else
				throw new ScriptRuntimeException("Expected a Date and Time value.", this);
		}
	}
}
