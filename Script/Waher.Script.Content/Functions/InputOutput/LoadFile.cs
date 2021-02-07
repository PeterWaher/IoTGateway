using System;
using System.IO;
using Waher.Content;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Content.Functions.InputOutput
{
	/// <summary>
	/// LoadFile(FileName)
	/// </summary>
	public class LoadFile : FunctionMultiVariate
	{
		/// <summary>
		/// LoadFile(FileName)
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadFile(ScriptNode FileName, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { FileName }, argumentTypes1Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// LoadFile(FileName, ContentType)
		/// </summary>
		/// <param name="FileName">File name.</param>
		/// <param name="ContentType">Content Type</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public LoadFile(ScriptNode FileName, ScriptNode ContentType, int Start, int Length, Expression Expression)
			: base(new ScriptNode[] { FileName, ContentType }, argumentTypes2Scalar, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName
		{
			get { return "loadfile"; }
		}

		/// <summary>
		/// Default Argument names
		/// </summary>
		public override string[] DefaultArgumentNames => new string[] { "FileName", "ContentType" };

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Arguments">Function arguments.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement[] Arguments, Variables Variables)
		{
			string FileName = Arguments[0].AssociatedObjectValue?.ToString();
			string ContentType;
			
			FileName = Path.Combine(Directory.GetCurrentDirectory(), FileName);

			if (Arguments.Length > 1)
				ContentType = Arguments[1].AssociatedObjectValue?.ToString();
			else
				ContentType = InternetContent.GetContentType(Path.GetExtension(FileName));

			byte[] Bin = File.ReadAllBytes(FileName);
			object Decoded = InternetContent.Decode(ContentType, Bin, new Uri(FileName));

			return Expression.Encapsulate(Decoded);
		}
	}
}
