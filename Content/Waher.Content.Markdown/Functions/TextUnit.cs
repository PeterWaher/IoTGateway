using System;
using Waher.Script;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Content.Markdown.Functions
{
	/// <summary>
	/// Selects the singular or plural form of a unit to be used in text, based on the associated number.
	/// </summary>
	public class TextUnit : FunctionMultiVariate
	{
		/// <summary>
		/// Selects the singular or plural form of a unit to be used in text, based on the associated number.
		/// </summary>
		/// <param name="Number">Number</param>
		/// <param name="SingularUnit">Singular unit.</param>
		/// <param name="PluralUnit">Plural unit.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public TextUnit(ScriptNode Number, ScriptNode SingularUnit, ScriptNode PluralUnit, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Number, SingularUnit, PluralUnit }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(TextUnit);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Number", "SingularUnit", "PluralUnit" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			double Nr = Expression.ToDouble(Arguments[0].AssociatedObjectValue);

			if (Math.Abs(Nr) == 1)
				return Arguments[1];
			else
				return Arguments[2];
		}
	}
}
