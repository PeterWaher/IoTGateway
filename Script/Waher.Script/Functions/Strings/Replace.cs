using System.Text.RegularExpressions;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Strings
{
	/// <summary>
	/// Replace(String,From,To)
	/// </summary>
	public class Replace : FunctionMultiVariate
	{
		private readonly bool useRegex;

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
			this.useRegex = false;
		}

		/// <summary>
		/// Replaceenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="String">String to operate on.</param>
		/// <param name="From">Substring to replace.</param>
		/// <param name="To">Substrings will be replaced with this.</param>
		/// <param name="UseRegex">If <paramref name="From"/> is a regular expression.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Replace(ScriptNode String, ScriptNode From, ScriptNode To, bool UseRegex, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String, From, To }, argumentTypes3Scalar, Start, Length, Expression)
		{
			this.useRegex = UseRegex;
		}

		/// <summary>
		/// Replaceenates the elements of a vector, optionally delimiting the elements with a Delimiter.
		/// </summary>
		/// <param name="String">String to operate on.</param>
		/// <param name="From">Substring to replace.</param>
		/// <param name="To">Substrings will be replaced with this.</param>
		/// <param name="Options">Regex options</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Replace(ScriptNode String, ScriptNode From, ScriptNode To, ScriptNode Options, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { String, From, To, Options }, argumentTypes4Scalar, Start, Length, Expression)
		{
			this.useRegex = true;
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

			if (!this.useRegex)
				return new StringValue(s.Replace(From, To));

			RegexOptions Options;

			if (Arguments.Length > 3)
				Options = GetOptions(Arguments[3].AssociatedObjectValue?.ToString(), this);
			else
				Options = RegexOptions.Singleline;

			Regex Expression = new Regex(From, Options);
			Match M = Expression.Match(s);

			while (M.Success)
			{
				s = s.Substring(0, M.Index) + To + s.Substring(M.Index + M.Length);
				M = Expression.Match(s, M.Index);
			}

			return new StringValue(s);
		}

		/// <summary>
		/// Converts a string-representation of regex options into an
		/// enumeration value.
		/// </summary>
		/// <param name="Options">String-representation of options.</param>
		/// <param name="Node">Node performing the calcaultion.</param>
		/// <returns>Options.</returns>
		/// <exception cref="ScriptRuntimeException">If options are not recognized.</exception>
		public static RegexOptions GetOptions(string Options, ScriptNode Node)
		{
			RegexOptions Result = RegexOptions.Singleline;

			if (!string.IsNullOrEmpty(Options))
			{
				foreach (char ch in Options)
				{
					switch (ch)
					{
						case 'i':
							Result |= RegexOptions.IgnoreCase;
							break;

						case 'm':
							Result &= ~RegexOptions.Singleline;
							Result |= RegexOptions.Multiline;
							break;

						case 'x':
							Result |= RegexOptions.IgnorePatternWhitespace;
							break;

						default:
							throw new ScriptRuntimeException("Regular expression option not supported: " + ch, Node);
					}
				}
			}

			return Result;
		}
	}
}
