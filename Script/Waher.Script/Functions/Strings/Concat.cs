using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
	/// </summary>
	public class Concat : FunctionMultiVariate
	{
		/// <summary>
		/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="Vector">Vector of elements to concatenate.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Concat(ScriptNode Vector, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector }, new ArgumentType[] { ArgumentType.Vector }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Concatenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="Vector">Vector of elements to concatenate.</param>
		/// <param name="Delimiter">Optional delimiter.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Concat(ScriptNode Vector, ScriptNode Delimiter, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { Vector, Delimiter }, new ArgumentType[] { ArgumentType.Vector, ArgumentType.Scalar }, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => "Concat";

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Vector", "Delimiter" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			if (!(Arguments[0] is IVector Vector))
				return Arguments[0];

			string Delimiter = Arguments.Length > 1 ? Arguments[1].AssociatedObjectValue?.ToString() : null;

			StringBuilder Result = new StringBuilder();
			bool First = true;
			string s;

			foreach (IElement Item in Vector.VectorElements)
			{
				if (Item is StringValue S)
					s = S.Value;
				else
					s = Item.AssociatedObjectValue?.ToString() ?? string.Empty;

				if (First)
					First = false;
				else if (!(Delimiter is null))
					Result.Append(Delimiter);

				Result.Append(s);
			}

			return new StringValue(Result.ToString());
		}
	}
}
