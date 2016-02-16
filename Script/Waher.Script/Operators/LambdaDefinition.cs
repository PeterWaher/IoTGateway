using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Model;

namespace Waher.Script.Operators
{
	/// <summary>
	/// Lambda Definition.
	/// </summary>
	public class LambdaDefinition : UnaryOperator 
	{
		private string[] argumentNames;
		private ArgumentType[] argumentTypes;

		/// <summary>
		/// Lambda Definition.
		/// </summary>
		/// <param name="ArgumentNames">Argument Names.</param>
		/// <param name="Operand">Operand.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public LambdaDefinition(string[] ArgumentNames, ArgumentType[] ArgumentTypes, ScriptNode Operand, int Start, int Length)
			: base(Operand, Start, Length)
		{
			this.argumentNames = ArgumentNames;
		}

		/// <summary>
		/// Argument Names.
		/// </summary>
		public string[] ArgumentNames
		{
			get { return this.argumentNames; }
		}

		/// <summary>
		/// Argument types.
		/// </summary>
		public ArgumentType[] ArgumentTypes
		{
			get { return this.argumentTypes; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override Element Evaluate(Variables Variables)
		{
			throw new NotImplementedException();	// TODO: Implement
		}

	}
}
