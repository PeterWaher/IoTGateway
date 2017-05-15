using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Represents an implicit string to be printed.
	/// </summary>
	public class ImplicitPrint : ScriptNode
	{
		private string content;

		/// <summary>
		/// Represents an implicit string to be printed.
		/// </summary>
		/// <param name="Content">Content.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public ImplicitPrint(string Content, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.content = Content;
		}

		/// <summary>
		/// Content
		/// </summary>
		public string Content
		{
			get { return this.content; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			string s = Expression.Transform(this.content, "((", "))", Variables);
			Variables.ConsoleOut?.Write(s);
			return new StringValue(s);
		}

	}
}
