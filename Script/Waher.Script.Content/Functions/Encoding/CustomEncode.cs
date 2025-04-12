using System.Collections.Generic;
using System.Text;
using Waher.Content;
using Waher.Content.Binary;
using Waher.Runtime.IO;
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
					KeyValuePair<string, string>[] Fields = CommonTypes.ParseFieldValues(ContentType.Trim());
					string Charset = null;

					foreach (KeyValuePair<string, string> P in Fields)
					{
						if (string.Compare(P.Key, "charset", true) == 0)
						{
							Charset = P.Value;
							break;
						}
					}

					if (Charset is null)
					{
						Bin = Strings.Utf8WithBom.GetBytes(s);
						ContentType += "; charset=utf-8";
					}
					else
					{
						System.Text.Encoding Encondig = System.Text.Encoding.GetEncoding(Charset);
						Bin = Encondig.GetBytes(s);

					}
				}
				else
					throw new ScriptRuntimeException("Expected binary or string content.", this);
			}

			return new ObjectValue(new CustomEncoding(ContentType, Bin));
		}

	}
}
