using System;
using System.Collections.Generic;
using Waher.Content;
using Waher.Content.Xml;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.Encoding
{
	/// <summary>
	/// Base64UrlDecode(Base64Url)
	/// </summary>
	public class Base64UrlDecode : FunctionOneScalarVariable
	{
		/// <summary>
		/// Base64UrlDecode(Base64Url)
		/// </summary>
		/// <param name="Base64Url">Base64Url-encoded data</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public Base64UrlDecode(ScriptNode Base64Url, int Start, int Length, Expression Expression)
			: base(Base64Url, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "base64urldecode"; }
		}

		/// <summary>
		/// Evaluates the function on a scalar argument.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement EvaluateScalar(string Argument, Variables Variables)
		{
			return new ObjectValue(Base64Url.Decode(Argument));
		}

	}
}
