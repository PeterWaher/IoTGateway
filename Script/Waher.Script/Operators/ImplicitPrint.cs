﻿using System;
using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Represents an implicit string to be printed.
	/// </summary>
	public class ImplicitPrint : ScriptLeafNode
	{
		private readonly string content;

		/// <summary>
		/// Represents an implicit string to be printed.
		/// </summary>
		/// <param name="Content">Content.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public ImplicitPrint(string Content, int Start, int Length, Expression Expression)
			: base(Start, Length, Expression)
		{
			this.content = Content;
		}

		/// <summary>
		/// Content
		/// </summary>
		public string Content => this.content;

		/// <summary>
		/// If the node (or its decendants) include asynchronous evaluation. Asynchronous nodes should be evaluated using
		/// <see cref="ScriptNode.EvaluateAsync(Variables)"/>.
		/// </summary>
		public override bool IsAsynchronous => true;

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			return this.EvaluateAsync(Variables).Result;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			string s = await Expression.TransformAsync(this.content, "((", "))", Variables);
			Variables.ConsoleOut?.Write(s);
			return new StringValue(s);
		}

		/// <inheritdoc/>
		public override bool Equals(object obj)
		{
			return obj is ImplicitPrint O &&
				this.content.Equals(O.content) &&
				base.Equals(obj);
		}

		/// <inheritdoc/>
		public override int GetHashCode()
		{
			int Result = base.GetHashCode();
			Result ^= Result << 5 ^ this.content.GetHashCode();
			return Result;
		}

	}
}
