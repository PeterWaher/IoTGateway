using System;
using System.Collections.Generic;
using System.Text;
using Waher.Script.Abstraction.Elements;
using Waher.Script.Exceptions;
using Waher.Script.Objects;

namespace Waher.Script.Model
{
	/// <summary>
	/// Represents a variable reference.
	/// </summary>
	public sealed class VariableReference : ScriptNode
	{
		private string variableName;

		/// <summary>
		/// Represents a variable reference.
		/// </summary>
		/// <param name="VariableName">Variable name.</param>
		/// <param name="Start">Start position in script expression.</param>
		/// <param name="Length">Length of expression covered by node.</param>
		public VariableReference(string VariableName, int Start, int Length)
			: base(Start, Length)
		{
			this.variableName = VariableName;
		}

		/// <summary>
		/// Variable Name.
		/// </summary>
		public string VariableName
		{
			get { return this.variableName; }
		}

		/// <summary>
		/// Evaluates the node, using the variables provided in the <paramref name="Variables"/> collection.
		/// </summary>
		/// <param name="Variables">Variables collection.</param>
		/// <returns>Result.</returns>
		public override IElement Evaluate(Variables Variables)
		{
			Variable v;

			if (Variables.TryGetVariable(this.variableName, out v))
				return v.ValueElement;

			IElement ValueElement;

			if (Expression.TryGetConstant(this.variableName, out ValueElement))
				return ValueElement;

			if (Types.IsRootNamespace(this.variableName))
				return new Namespace(this.variableName);

			throw new ScriptRuntimeException("Variable not found: " + this.variableName, this);
		}

	}
}
