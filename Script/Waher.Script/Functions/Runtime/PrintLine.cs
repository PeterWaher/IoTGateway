﻿using System.Threading.Tasks;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;
using Waher.Script.Objects;

namespace Waher.Script.Functions.Runtime
{
	/// <summary>
	/// Prints to the console, ending with a new line.
	/// </summary>
	public class PrintLine : FunctionOneVariable
	{
		/// <summary>
		/// Prints to the console, ending with a new line.
		/// </summary>
		/// <param name="Argument">Argument.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PrintLine(ScriptNode Argument, int Start, int Length, Expression Expression)
			: base(Argument, Start, Length, Expression)
		{
		}

		/// <summary>
		/// Prints to the console, ending with a new line.
		/// </summary>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		/// <param name="Expression">Expression containing script.</param>
		public PrintLine(int Start, int Length, Expression Expression)
			: base(new ConstantElement(new StringValue(string.Empty), Start, Length, Expression),
				  Start, Length, Expression)
		{
		}

		/// <summary>
		/// Name of the function
		/// </summary>
		public override string FunctionName => nameof(PrintLine);

		/// <summary>
		/// Optional aliases. If there are no aliases for the function, null is returned.
		/// </summary>
		public override string[] Aliases
		{
			get { return new string[] { "println" }; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			IElement E = this.Argument.Evaluate(Variables);
			string Msg = E.AssociatedObjectValue is string s ? s : E.ToString();
			Variables.ConsoleOut?.WriteLine(Msg);
			return E;
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override async Task<IElement> EvaluateAsync(Variables Variables)
		{
			IElement E = await this.Argument.EvaluateAsync(Variables);
			string Msg = E.AssociatedObjectValue is string s ? s : E.ToString();
			Variables.ConsoleOut?.WriteLine(Msg);
			return E;
		}

		/// <summary>
		/// Evaluates the function.
		/// </summary>
		/// <param name="Argument">Function argument.</param>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Function result.</returns>
		public override IElement Evaluate(IElement Argument, Variables Variables)
		{
			return ObjectValue.Null;
		}
	}
}
