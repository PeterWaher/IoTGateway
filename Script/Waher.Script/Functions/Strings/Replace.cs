using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Replace(String,From,To)
	/// </summary>
	public class Replace : FunctionMultiVariate
	{
		/// <summary>
		/// Replaceenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="String">String to operate on.</param>
		/// <param name="From">Substring to replace.</param>
		/// <param name="To">Substrings will be replaced with this.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Replace(ScriptNode String, ScriptNode From, ScriptNode To, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String, From, To }, argumentTypes3Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(Replace);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "String", "From", "To" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			string s = Arguments[0].AssociatedObjectValue?.ToString() ?? string.Empty;
			string From = Arguments[1].AssociatedObjectValue?.ToString() ?? string.Empty;
			string To = Arguments[2].AssociatedObjectValue?.ToString() ?? string.Empty;

			return new StringValue(s.Replace(From,To));
		}
	}
}
