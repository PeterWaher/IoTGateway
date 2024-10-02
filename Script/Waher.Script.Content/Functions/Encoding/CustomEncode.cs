using System.Text;
using Waher.Content.Binary;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// CustomEncode(Binary,Content-Type)
	/// </summary>
	public class CustomEncode : FunctionTwoVariables
	{
		/// <summary>
		/// CustomEncode(Binary,Content-Type)
		/// </summary>
		/// <param name="Binary">Binary data</param>
		/// <param name="ContentType">Internet Content-Type.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public CustomEncode(ScriptNode Binary, ScriptNode ContentType, int Start, int Length, Expression Expression)
			: base(Binary, ContentType, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(CustomEncode);

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "Binary", "ContentType" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument1">Function argument 1.</param>
		/// <param name="Argument2">Function argument 2.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument1, IElement Argument2, Variables Variables)
		{
			object Obj = Argument1.AssociatedObjectValue;
			string ContentType = Argument2.AssociatedObjectValue?.ToString() ?? string.Empty;

			if (!(Obj is byte[] Bin))
			{
				if (Obj is string s)
				{
					UTF8Encoding x = new UTF8Encoding(true);
					Bin = Utf8WithBOM.GetBytes(s);

					if (!ContentType.ToLower().Contains("charset"))
						ContentType += "; charset=utf-8";
				}
				else
					throw new ScriptRuntimeException("Expected binary or string content.", this);
			}

			return new ObjectValue(new CustomEncoding(ContentType, Bin));
		}

		private static readonly System.Text.Encoding Utf8WithBOM = new UTF8Encoding(true);

	}
}
